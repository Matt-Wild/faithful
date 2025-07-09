using System.Collections.Generic;
using BepInEx.Configuration;
using System;
using System.Reflection;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace Faithful
{
    internal static class Config
    {
        // Store a reference to BepInEx config file
        static ConfigFile configFile;

        // Store dictionary of all settings
        static Dictionary<string, ISetting> settings = new Dictionary<string, ISetting>();

        public static void Init(ConfigFile _configFile)
        {
            // Assign config file
            configFile = _configFile;

            // Initialise Risk of Options wrapper
            RiskOfOptionsWrapper.Init();
        }

        public static Setting<T> CreateSetting<T>(string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Check for token in settings dictionary
            if (settings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[CONFIG] - Could not create setting for token '{_token}' as token already exists.");
                return null;
            }

            // Create setting
            Setting<T> setting = new Setting<T>(configFile, _token, _section, _key, _defaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);

            // Check if setting is temp
            if (!_token.ToUpper().Contains("TEMP"))
            {
                // Create Risk of Options option
                RiskOfOptionsWrapper.AddOption(setting);
            }

            // Add setting to dictionary
            settings.Add(_token, setting);

            // Return setting
            return setting;
        }

        public static Setting<T> FetchSetting<T>(string _token)
        {
            // Check for token in settings dictionary
            if (!settings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[CONFIG] - Attempted to fetch setting with token '{_token}' but token was not found.");
                return null;
            }

            // Attempt to return casted setting
            try
            {
                // Return casted setting
                return settings[_token] as Setting<T>;
            }
            catch
            {
                // Log warning
                Log.Warning($"[CONFIG] - Could not fetch setting with token '{_token}' as type '{typeof(T).Name}'.");
                return null;
            }
        }

        public static ISetting FetchSetting(string _token)
        {
            // Check for token in settings dictionary
            if (!settings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[CONFIG] - Attempted to fetch setting with token '{_token}' but token was not found.");
                return null;
            }

            // Return setting
            return settings[_token];
        }

        public static string FormatLanguageToken(string _token, string _tokenPrefix = "", string _corruptedItemName = "")
        {
            // Get language string
            string languageString = Utils.GetLanguageString(_token);

            // Get token prefix
            string tokenPrefix = _tokenPrefix;

            // Check for token prefix
            if (tokenPrefix != "")
            {
                // Add underscore to prefix
                tokenPrefix += "_";
            }

            // Cycle through settings
            foreach (KeyValuePair<string, ISetting> setting in settings)
            {
                // Get setting token
                string settingToken = setting.Key;

                // Check for token prefix
                if (tokenPrefix != "")
                {
                    // Check if token prefix is in setting token
                    if (!settingToken.Contains(tokenPrefix)) continue;

                    // Check if setting is not default, is a stat and alt token exists
                    if (!setting.Value.isDefault && setting.Value.isStat && Utils.HasLanguageString($"{_token}_ALT"))
                    {
                        // Return formatted alt token instead
                        return FormatLanguageToken($"{_token}_ALT", _tokenPrefix);
                    }

                    // Remove token prefix from setting token
                    settingToken = settingToken.Replace(tokenPrefix, "");
                }

                // Add language formatting indicator
                settingToken = $"[{settingToken}]";

                // Replace setting token in language string with appropriate value
                languageString = languageString.Replace(settingToken, setting.Value.ValueObject.ToString());
            }

            // Remove empty stacking strings from language string
            languageString = RemoveZeroStackingTags(languageString);

            // Check for CORRUPTED_ITEM token
            if (languageString.Contains("[CORRUPTED_ITEM]"))
            {
                // Check for corrupted item name
                if (!string.IsNullOrWhiteSpace(_corruptedItemName))
                {
                    // Replace unique CORRUPTED_ITEM token
                    languageString = languageString.Replace("[CORRUPTED_ITEM]", Utils.Pluralize(_corruptedItemName));
                }

                // No corrupted item name provided
                else
                {
                    // Error and remove corrupted items sentence
                    Log.Error($"[CONFIG] - Was unable to process corrupted item token for language token '{_token}' as no corrupted item name was provided.");
                    languageString = languageString.Replace(" <style=cIsVoid>Corrupts all [CORRUPTED_ITEM]</style>.", "");
                }
            }

            // Return formatted language string
            return languageString;
        }

        private static string RemoveZeroStackingTags(string _languageString)
        {
            // Regex: match (optional space)(<style=cStack> ... </style>)
            string pattern = @"( ?)<style=cStack>(.*?)<\/style>";

            return Regex.Replace(_languageString, pattern, match =>
            {
                string tagContent = match.Groups[2].Value;

                // Find all digits
                var digits = Regex.Matches(tagContent, @"\d");
                bool onlyZero = digits.Count > 0 && digits.Cast<Match>().All(d => d.Value == "0");
                bool hasNonZero = digits.Cast<Match>().Any(d => d.Value != "0");

                // Remove the tag if all digits are zero and there is at least one digit,
                // or if there are no digits at all (to be safe, you can choose to keep/remove in that case).
                if (digits.Count > 0 && !hasNonZero)
                    return ""; // Remove the tag (and space, if present)
                else
                    return match.Value; // Keep the original tag
            });
        }

        public static void DeleteSetting(string _token)
        {
            // Check for setting
            if (!settings.ContainsKey(_token)) return;

            // Tell setting to delete from config file
            settings[_token].DeleteFromConfigFile();

            // Remove setting
            settings.Remove(_token);
        }

        public static void ResetSettingRandomisers()
        {
            // Cycle through settings
            foreach (KeyValuePair<string, ISetting> setting in settings)
            {
                // Reset is setting has been randomised
                setting.Value.isRandomised = false;
            }
        }

        public static Dictionary<string, ISetting> GetSettings()
        {
            // Return settings
            return settings;
        }
    }

    internal static class RiskOfOptionsWrapper
    {
        // Plugin GUID and mod name
        public static string pluginGUID;
        public static string pluginName;

        // Whether Risk of Options is usable
        static bool enabled = false;

        // Assembly reference for Risk of Options
        static Assembly assembly;

        // Type references for Risk of Options
        static Type baseOptionType;
        static Type checkBoxOptionType;
        static Type stepSliderOptionType;
        static Type intSliderOptionType;
        static Type stringInputFieldOptionType;
        static Type checkBoxConfigType;
        static Type stepSliderConfigType;
        static Type intSliderConfigType;
        static Type inputFieldConfigType;

        // Method references for Risk of Options
        static MethodInfo setModIconMethod;
        static MethodInfo addOptionMethod;

        public static void Init()
        {
            // Cycle through loaded assemblies
            foreach (Assembly currentAssembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Check if Risk of Options is installed
                if (currentAssembly.GetName().Name == "RiskOfOptions")
                {
                    // Assign assembly
                    assembly = currentAssembly;

                    // Risk of Options installed
                    enabled = true;

                    // Get types
                    GetTypes();

                    // Get methods
                    GetMethods();

                    if (Utils.verboseConsole) Log.Info("Risk Of Options wrapper created successfully.");
                }
            }
        }

        public static void UpdateModIcon()
        {
            // Ignore if not enabled
            if (!enabled) return;

            // Check for set mod icon method
            if (setModIconMethod != null)
            {
                // Set Risk of Options mod icon
                setModIconMethod.Invoke(null, [Assets.GetIcon("texCustomExpansionIcon")]);
            }
        }

        public static void AddOption<T>(Setting<T> _setting)
        {
            // Ignore if not enabled
            if (!enabled) return;

            // Check for add option method
            if (addOptionMethod == null) return;

            // Bool behaviour
            if (typeof(T) == typeof(bool))
            {
                // Check for check box option type and check box config type
                if (checkBoxOptionType == null || checkBoxConfigType == null) return;

                // Create check box config
                object checkBoxConfig = Activator.CreateInstance(checkBoxConfigType);

                // Set if restart is required
                checkBoxConfigType.GetField("restartRequired")?.SetValue(checkBoxConfig, _setting.restartRequired);

                // Create check box option
                object checkBoxOption = Activator.CreateInstance(checkBoxOptionType, [_setting.configEntry, checkBoxConfig]);

                try
                {
                    // Add into Risk of Options
                    addOptionMethod.Invoke(null, [checkBoxOption, pluginGUID, pluginName]);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[CONFIG] | Failed to add Risk Of Options option for setting '{_setting.token}'.\nException: {ex}");
                }
            }

            // Float behaviour
            else if (typeof(T) == typeof(float))
            {
                // Check for slider option type and slider config type
                if (stepSliderOptionType == null || stepSliderConfigType == null) return;

                // Create slider config
                object stepSliderConfig = Activator.CreateInstance(stepSliderConfigType);

                // Set min and max fields
                stepSliderConfigType.GetField("min")?.SetValue(stepSliderConfig, Convert.ToSingle(_setting.sliderMin));
                stepSliderConfigType.GetField("max")?.SetValue(stepSliderConfig, Convert.ToSingle(_setting.sliderMax));
                stepSliderConfigType.GetField("increment")?.SetValue(stepSliderConfig, _setting.stepSliderIncrement);

                // Set if restart is required
                stepSliderConfigType.GetField("restartRequired")?.SetValue(stepSliderConfig, _setting.restartRequired);

                // Set value formatting
                stepSliderConfigType.GetProperty("FormatString")?.SetValue(stepSliderConfig, _setting.valueFormatting);

                // Create slider option
                object stepSliderOption = Activator.CreateInstance(stepSliderOptionType, [_setting.configEntry, stepSliderConfig]);

                try
                {
                    // Add into Risk of Options
                    addOptionMethod.Invoke(null, [stepSliderOption, pluginGUID, pluginName]);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[CONFIG] | Failed to add Risk Of Options option for setting '{_setting.token}'.\nException: {ex}");
                }
            }

            // Int behaviour
            else if (typeof(T) == typeof(int))
            {
                // Check for int slider option type and int slider config type
                if (intSliderOptionType == null || intSliderConfigType == null) return;

                // Create int slider config
                object intSliderConfig = Activator.CreateInstance(intSliderConfigType);

                // Set min and max fields
                intSliderConfigType.GetField("min")?.SetValue(intSliderConfig, Convert.ToInt32(_setting.sliderMin));
                intSliderConfigType.GetField("max")?.SetValue(intSliderConfig, Convert.ToInt32(_setting.sliderMax));

                // Set if restart is required
                intSliderConfigType.GetField("restartRequired")?.SetValue(intSliderConfig, _setting.restartRequired);

                // Set value formatting
                intSliderConfigType.GetProperty("FormatString")?.SetValue(intSliderConfig, _setting.valueFormatting);

                // Create int slider option
                object intSliderOption = Activator.CreateInstance(intSliderOptionType, [_setting.configEntry, intSliderConfig]);

                try
                {
                    // Add into Risk of Options
                    addOptionMethod.Invoke(null, [intSliderOption, pluginGUID, pluginName]);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[CONFIG] | Failed to add Risk Of Options option for setting '{_setting.token}'.\nException: {ex}");
                }
            }

            // String behaviour
            else if (typeof(T) == typeof(string))
            {
                // Check for string input field option type and input field config type
                if (stringInputFieldOptionType == null || inputFieldConfigType == null) return;

                // Create input field config
                object inputFieldConfig = Activator.CreateInstance(inputFieldConfigType);

                // Set if restart is required
                inputFieldConfigType.GetField("restartRequired")?.SetValue(inputFieldConfig, _setting.restartRequired);

                // Create string input field option
                object stringInputFieldOption = Activator.CreateInstance(stringInputFieldOptionType, [_setting.configEntry, inputFieldConfig]);

                try
                {
                    // Add into Risk of Options
                    addOptionMethod.Invoke(null, [stringInputFieldOption, pluginGUID, pluginName]);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[CONFIG] | Failed to add Risk Of Options option for setting '{_setting.token}'.\nException: {ex}");
                }
            }
        }

        static void GetTypes()
        {
            // Check for assembly
            if (assembly == null) return;

            // Get base option type
            baseOptionType = assembly.GetType("RiskOfOptions.Options.BaseOption");

            // Check for base option
            if (baseOptionType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'BaseOption' could not be found.");
            }

            // Get check box option type
            checkBoxOptionType = assembly.GetType("RiskOfOptions.Options.CheckBoxOption");

            // Check for check box option
            if (checkBoxOptionType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'CheckBoxOption' could not be found.");
            }

            // Get step slider option type
            stepSliderOptionType = assembly.GetType("RiskOfOptions.Options.StepSliderOption");

            // Check for step slider option
            if (stepSliderOptionType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'StepSliderOption' could not be found.");
            }

            // Get int slider option type
            intSliderOptionType = assembly.GetType("RiskOfOptions.Options.IntSliderOption");

            // Check for int slider option
            if (intSliderOptionType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'IntSliderOption' could not be found.");
            }

            // Get string input field option type
            stringInputFieldOptionType = assembly.GetType("RiskOfOptions.Options.StringInputFieldOption");

            // Check for string input field option
            if (stringInputFieldOptionType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'StringInputFieldOption' could not be found.");
            }

            // Get check box config type
            checkBoxConfigType = assembly.GetType("RiskOfOptions.OptionConfigs.CheckBoxConfig");

            // Check for check box config
            if (checkBoxConfigType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'CheckBoxConfig' could not be found.");
            }

            // Get step slider config type
            stepSliderConfigType = assembly.GetType("RiskOfOptions.OptionConfigs.StepSliderConfig");

            // Check for step slider config
            if (stepSliderConfigType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'StepSliderConfig' could not be found.");
            }

            // Get int slider config type
            intSliderConfigType = assembly.GetType("RiskOfOptions.OptionConfigs.IntSliderConfig");

            // Check for int slider config
            if (intSliderConfigType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'IntSliderConfig' could not be found.");
            }

            // Get input field config type
            inputFieldConfigType = assembly.GetType("RiskOfOptions.OptionConfigs.InputFieldConfig");

            // Check for input field config
            if (inputFieldConfigType == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the type 'InputFieldConfig' could not be found.");
            }
        }

        static void GetMethods()
        {
            // Check for assembly and base option type
            if (assembly == null || baseOptionType == null) return;

            // Get the set mod icon method
            setModIconMethod = assembly.GetType("RiskOfOptions.ModSettingsManager")?.GetMethod("SetModIcon", [typeof(Sprite)]);

            // Check for set mod icon method
            if (setModIconMethod == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the method 'SetModIcon' could not be found.");
            }

            // Get the add option method
            addOptionMethod = assembly.GetType("RiskOfOptions.ModSettingsManager")?.GetMethod("AddOption", [baseOptionType, typeof(string), typeof(string)]);

            // Check for add option method
            if (addOptionMethod == null)
            {
                Log.Warning($"[CONFIG] | Risk Of Options was found but the method 'AddOption' could not be found.");
            }
        }
    }

    internal class Setting<T> : ISetting
    {
        // Store config file
        private ConfigFile configFile;

        // Store token
        private string m_token;

        // Store config entry
        public ConfigEntry<T> configEntry;

        // Store default value
        public T defaultValue;

        // Store if setting is stat
        private bool m_isStat;

        // Store if setting is client side
        private bool m_isClientSide;

        // Store if setting is synced with host
        private bool synced = true;

        // Store synced value from host
        private T syncedValue;

        // Store if setting has generated randomised value
        private bool m_isRandomised = false;

        // Store if setting can randomise
        private bool canRandomise = true;

        // Store randomised value
        private T randomisedValue;

        // Store optional min and max values
        private T minValue;
        private T maxValue;

        // Store optional min and max randomiser values
        private T randomiserMin;
        private T randomiserMax;

        // Whether this config requires a restart
        public bool restartRequired;

        // Formatting for Risk Of Options
        public string valueFormatting;

        public Setting(ConfigFile _configFile, string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Assign config file
            configFile = _configFile;

            // Assign token
            m_token = _token;

            // Set default value
            defaultValue = _defaultValue;

            // Assign min and max values
            minValue = _minValue;
            maxValue = _maxValue;

            // Assign min and max randomiser values
            randomiserMin = _randomiserMin;
            randomiserMax = _randomiserMax;

            // Check for min value
            if (!EqualityComparer<T>.Default.Equals(minValue, default))
            {
                // Add to description
                _description += $"\nMin value: {minValue}";
            }

            // Check for max value
            if (!EqualityComparer<T>.Default.Equals(maxValue, default))
            {
                // Add to description
                _description += $"\nMax value: {maxValue}";
            }

            // Create config entry
            configEntry = configFile.Bind(_section, _key, _defaultValue, _description);

            // Assign if setting is stat
            m_isStat = _isStat;

            // Assign if setting if client side
            m_isClientSide = _isClientSide;

            // Assign if setting can randomise
            canRandomise = _canRandomise;

            // If restart is required for this setting's changes to take effect
            restartRequired = _restartRequired;

            // Assign value formatting
            valueFormatting = _valueFormatting;
        }

        public void Sync()
        {
            // Set as not synced
            synced = false;

            // Check for net utils
            if (Utils.netUtils != null)
            {
                // Request a sync for this setting
                Utils.netUtils.SyncSetting(this);
            }
        }

        public void Delete()
        {
            // Ask config to delete
            Config.DeleteSetting(token);
        }

        public void DeleteFromConfigFile()
        {
            // Remove config entry definition from config file
            configFile.Remove(configEntry.Definition);
        }

        private void GenerateRandomisedValue()
        {
            // Skip if randomised already
            if (m_isRandomised) return;

            // Set as randomised
            m_isRandomised = true;

            // Set default randomised value
            randomisedValue = defaultValue;

            // Don't randomise if not stat
            if (!isStat)
            {
                return;
            }

            // Get randomiser value (leans towards 1 with a rare chance of reaching up to 10)
            float randomiserValue = UnityEngine.Random.Range(0, 2) == 0 ? UnityEngine.Random.Range(1.0f, 2.0f) : UnityEngine.Random.Range(0, 2) == 0 ? UnityEngine.Random.Range(1.0f, 5.0f) : UnityEngine.Random.Range(1.0f, 10.0f);

            // Check if type is int
            if (type == typeof(int))
            {
                // Define random float
                float floatRandom;

                // Check for min or max randomiser values
                if (!EqualityComparer<T>.Default.Equals(randomiserMin, default) || !EqualityComparer<T>.Default.Equals(randomiserMax, default))
                {
                    // Get float min and max
                    float min = Convert.ToSingle(randomiserMin);
                    float max = Convert.ToSingle(randomiserMax);

                    // Override randomised value
                    floatRandom = UnityEngine.Random.Range(min, max);
                }

                // No min or max randomiser values provided
                else
                {
                    // Get float random value
                    floatRandom = Convert.ToSingle(randomisedValue);

                    // Check if multiplied or divided
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        // Multiply random value
                        floatRandom *= randomiserValue;
                    }

                    // Divide
                    else
                    {
                        // Divide random value
                        floatRandom /= randomiserValue;
                    }
                }

                // Assign as new randomised value
                randomisedValue = (T)(object)UnityEngine.Mathf.RoundToInt(floatRandom);

                // Done
                return;
            }

            // Check if type is float
            else if (type == typeof(float))
            {
                // Define random float
                float floatRandom;

                // Check for min or max randomiser values
                if (!EqualityComparer<T>.Default.Equals(randomiserMin, default) || !EqualityComparer<T>.Default.Equals(randomiserMax, default))
                {
                    // Get float min and max
                    float min = (float)(object)randomiserMin;
                    float max = (float)(object)randomiserMax;

                    // Override randomised value
                    floatRandom = UnityEngine.Random.Range(min, max);
                }

                // No min or max randomiser values provided
                else
                {
                    // Get float random value
                    floatRandom = (float)(object)randomisedValue;

                    // Check if multiplied or divided
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        // Multiply random value
                        floatRandom *= randomiserValue;
                    }

                    // Divide
                    else
                    {
                        // Divide random value
                        floatRandom /= randomiserValue;
                    }
                }

                // Check if float is greater than 10
                if (floatRandom >= 100.0f)
                {
                    // Round float to nearest 25
                    floatRandom = UnityEngine.Mathf.Round(floatRandom / 25.0f) * 25.0f;
                }

                // Check if float is greater than 10
                else if (floatRandom >= 10.0f)
                {
                    // Round float to nearest 5
                    floatRandom = UnityEngine.Mathf.Round(floatRandom / 5.0f) * 5.0f;
                }

                // Check if float is greater than 5
                else if (floatRandom >= 5.0f)
                {
                    // Round float to nearest 0.5
                    floatRandom = UnityEngine.Mathf.Round(floatRandom * 2.0f) / 2.0f;
                }    

                // Check if float is greater than 1
                else if (floatRandom >= 1.0f)
                {
                    // Round float to nearest 0.25
                    floatRandom = UnityEngine.Mathf.Round(floatRandom * 4.0f) / 4.0f;
                }

                // Float value is below 1
                else
                {
                    // Round float to nearest 0.05
                    floatRandom = UnityEngine.Mathf.Round(floatRandom * 20.0f) / 20.0f;
                }

                // Assign rounded value as new randomised value
                randomisedValue = (T)(object)floatRandom;

                // Done
                return;
            }
        }

        public SettingData GetSettingData()
        {
            // Return setting data made using this setting
            return new SettingData(this);
        }

        public T GetClampedValue(T _originalValue)
        {
            // Check if setting is an int type
            if (type == typeof(int))
            {
                // Clamp value
                int clampedValue = UnityEngine.Mathf.Clamp((int)(object)_originalValue, EqualityComparer<T>.Default.Equals(minValue, default) ? int.MinValue : (int)(object)minValue, EqualityComparer<T>.Default.Equals(maxValue, default) ? int.MaxValue : (int)(object)maxValue);

                // Return clamped value
                return (T)(object)clampedValue;
            }

            // Check if setting is a float type
            else if (type == typeof(float))
            {
                // Clamp value
                float clampedValue = UnityEngine.Mathf.Clamp((float)(object)_originalValue, EqualityComparer<T>.Default.Equals(minValue, default) ? float.MinValue : (float)(object)minValue, EqualityComparer<T>.Default.Equals(maxValue, default) ? float.MaxValue : (float)(object)maxValue);

                // Return clamped value
                return (T)(object)clampedValue;
            }

            // Otherwise just return given value
            else return _originalValue;
        }

        public void SetSyncedValue(SettingData _settingData)
        {
            // Ignore if already synced
            if (synced) return;

            // Set as synced
            synced = true;

            // Check if setting is a bool type
            if (type == typeof(bool))
            {
                // Take bool from setting data
                syncedValue = (T)(object)_settingData.boolValue;
            }

            // Check if setting is an int type
            else if (type == typeof(int))
            {
                // Take int from setting data
                syncedValue = (T)(object)_settingData.intValue;
            }

            // Check if setting is a float type
            else if (type == typeof(float))
            {
                // Take float from setting data
                syncedValue = (T)(object)_settingData.floatValue;
            }

            // No valid setting type found
            else
            {
                // Warn
                Log.Warning($"[CONFIG] - Could not sync setting '{token}' with type '{type}'.");
            }
        }

        public string token
        {
            get
            {
                // Return token
                return m_token;
            }
        }

        public T Value
        {
            get
            {
                // Check if should retrieve synced value
                if (Utils.netUtils != null && !Utils.hosting && synced && !EqualityComparer<T>.Default.Equals(syncedValue, default) && !isClientSide)
                {
                    // Return synced value instead
                    return GetClampedValue(syncedValue);
                }

                // Check if randomiser mode is enabled and this is a stat (that can randomise)
                if (Utils.randomiserMode && isStat && canRandomise)
                {
                    // Return randomised value
                    return GetClampedValue(RandomisedValue);
                }

                // Return value of config entry
                return GetClampedValue(configEntry.Value);
            }
            set
            {
                // Set value of config entry
                configEntry.Value = value;
            }
        }

        private T RandomisedValue
        {
            get
            {
                // Check if randomised
                if (!m_isRandomised)
                {
                    // Generate randomised value
                    GenerateRandomisedValue();
                }

                // Return randomised value
                return randomisedValue;
            }
        }

        public float stepSliderIncrement
        {
            get
            {
                // Check if bool or int
                if (typeof(T) == typeof(bool) || typeof(T) == typeof(int))
                {
                    // Return default fallback
                    return 1.0f;
                }

                // Check for float and valid value formatting
                else if (typeof(T) == typeof(float) && !string.IsNullOrEmpty(valueFormatting))
                {
                    // Regex to count decimal places in formatting like "{0:0.00}" or "{0:0.0}s"
                    Match match = Regex.Match(valueFormatting, @"{0:0+(\.(0+))?}");
                    if (match.Success)
                    {
                        string decimalPart = match.Groups[1].Value; // Extract the decimal portion (e.g., ".00")
                        int decimalPlaces = decimalPart.Length > 1 ? decimalPart.Length - 1 : 0; // Subtract 1 for the leading '.'

                        // Return increment based on decimal places
                        return decimalPlaces switch
                        {
                            0 => 1.0f,
                            1 => 0.1f,
                            2 => 0.01f,
                            _ => 0.01f // Default fallback for more decimal places
                        };
                    }
                }

                // Return default fallback
                return 1.0f;
            }
        }

        public T sliderMin
        {
            get
            {
                // Check if bool or default min value
                if (typeof(T) == typeof(bool) || EqualityComparer<T>.Default.Equals(minValue, default)) return (T)Convert.ChangeType(0, typeof(T)); ;

                // Otherwise return min value
                return minValue;
            }
        }

        public T sliderMax
        {
            get
            {
                // Check if bool
                if (typeof(T) == typeof(bool)) return (T)Convert.ChangeType(0, typeof(T));

                // Check if default max value
                if (EqualityComparer<T>.Default.Equals(maxValue, default)) return (T)Convert.ChangeType(Convert.ToDouble(defaultValue) * 10, typeof(T));

                // Otherwise return max value
                return maxValue;
            }
        }

        public object ValueObject
        {
            get
            {
                // Return config entry value
                return Value;
            }
        }

        public bool isDefault
        {
            get
            {
                // Check if setting is default
                return EqualityComparer<T>.Default.Equals(defaultValue, Value);
            }
        }

        public bool isStat
        {
            get
            {
                // Return if setting is a stat
                return m_isStat;
            }
        }

        public bool isClientSide
        {
            get
            {
                // Return if setting is client side
                return m_isClientSide;
            }
        }

        public bool isSynced
        {
            get
            {
                // Return if item is synced with host
                return synced;
            }
        }

        public bool isRandomised
        {
            get
            {
                // Return if this setting had been randomised
                return m_isRandomised;
            }
            set
            {
                // Set if this setting has been randomised
                m_isRandomised = value;
            }
        }

        public Type type
        {
            get
            {
                // Return setting type
                return typeof(T);
            }
        }
    }

    public interface ISetting
    {
        public void Sync();

        public void DeleteFromConfigFile();

        public SettingData GetSettingData();

        public void SetSyncedValue(SettingData _settingData);

        public string token { get; }

        public object ValueObject { get; }

        public bool isDefault { get; }

        public bool isStat { get; }

        public bool isClientSide { get; }

        public bool isSynced { get; }

        public bool isRandomised { get; set; }

        public Type type { get; }
    }

    // Wrapper for sending setting data across the network
    public struct SettingData
    {
        // Store setting token
        public string token;

        // Store different serialisable settings values
        public bool boolValue; 
        public int intValue;
        public float floatValue;

        // Construct this struct using a setting
        public SettingData(ISetting _setting)
        {
            // Set token
            token = _setting.token;

            // Check if setting is of type bool
            if (_setting.type == typeof(bool))
            {
                // Set bool value
                boolValue = (bool)_setting.ValueObject;
            }

            // Check if setting is of type int
            else if (_setting.type == typeof(int))
            {
                // Set int value
                intValue = (int)_setting.ValueObject;
            }

            // Check if setting is of type float
            else if (_setting.type == typeof(float))
            {
                // Set float value
                floatValue = (float)_setting.ValueObject;
            }
        }
    }

    /*internal static class Config
    {
        // Store plugin info
        public static PluginInfo pluginInfo;

        // Config tag list
        static List<ConfigTag> tags = new List<ConfigTag>();

        // Store default item flags
        static ConfigTag defaultItemFlags;

        public static void Init(PluginInfo _pluginInfo)
        {
            // Set plugin info
            pluginInfo = _pluginInfo;

            // Check for config file
            if (File.Exists(ConfigFilePath))
            {
                Log.Debug("Reading config file");

                // Read config file
                ReadConfig();
            }
            else
            {
                Log.Debug("No config file found");
            }

            Log.Debug("Config initialised");
        }

        private static void ReadConfig()
        {
            // Create file reader
            using (StreamReader reader = File.OpenText(ConfigFilePath))
            {
                // Continue until end of file
                while (reader.Peek() > -1)
                {
                    // Read line
                    string line = reader.ReadLine();

                    // Ignore comments
                    if (!line.StartsWith("/") && !line.StartsWith("#"))
                    {
                        // Remove spaces
                        line = line.Replace(" ", "");

                        // Get arguments
                        string[] args = line.Split(':');

                        // Check for valid amount of arguments
                        if (args.Length >= 2)
                        {
                            // Get tag
                            string tag = args[0].ToUpper();

                            // Get state
                            bool enabled = args[1].ToLower().Contains("true");

                            // Store flags
                            string[] flags = [];

                            // Test for flags
                            if (args.Length >= 3)
                            {
                                // Get flags
                                flags = args[2].ToUpper().Split('|');
                            }

                            // Create new config tag
                            ConfigTag configTag = new ConfigTag(tag, enabled, flags);

                            // Check if default item flags
                            if (tag == "DEFAULT_ITEM_FLAGS")
                            {
                                // Set default item flags
                                defaultItemFlags = configTag;
                            }
                            else
                            {
                                // Otherwise append config tag
                                tags.Add(configTag);
                            }
                        }
                    }
                }
            }
        }

        // Get if tag is enabled in config
        public static bool CheckTag(string _tag)
        {
            // Force tag uppercase
            _tag = _tag.ToUpper();

            // Cycle through tags
            foreach (ConfigTag tag in tags)
            {
                // Check if correct tag
                if (tag.tag == _tag)
                {
                    // Return state of tag
                    return tag.enabled;
                }
            }

            // If tag not found for debug mode, assume disabled
            if (_tag == "DEBUG_MODE")
            {
                return false;
            }

            // Otherwise assume feature is enabled
            return true;
        }

        // Get if tag has a flag
        public static bool CheckTagFlag(string _tag, string _flag, bool _isItem = false)
        {
            // Force tag uppercase
            _tag = _tag.ToUpper();

            // Assume flag is false
            bool flag = false;

            // Cycle through tags
            foreach (ConfigTag tag in tags)
            {
                // Check if correct tag
                if (tag.tag == _tag)
                {
                    // Update flag and escape loop
                    flag = tag.GetFlag(_flag);

                    break;
                }
            }

            // Override false item flags with default item flags
            if (_isItem && !flag)
            {
                // Check for default item flags
                if (defaultItemFlags != null)
                {
                    // Override flag
                    flag = defaultItemFlags.GetFlag(_flag);
                }
            }

            // Return flag
            return flag;
        }

        // Get the path to the asset bundle
        public static string ConfigFilePath
        {
            get
            {
                // Returns the path to the asset bundle
                return Path.Combine(Path.GetDirectoryName(pluginInfo.Location), "config.cfg");
            }
        }
    }

    internal class ConfigTag
    {
        // Store tag and if tag is enabled as well as any flags
        public string tag;
        public bool enabled;
        public string[] flags;

        // Constructor
        public ConfigTag(string _tag, bool _enabled, string[] _flags)
        {
            // Set tag, state and flags
            tag = _tag;
            enabled = _enabled;
            flags = _flags;
        }

        // Get if config tag has a flag
        public bool GetFlag(string _flag)
        {
            // Cycle through flags
            foreach (string flag in flags)
            {
                // Flag found
                if (_flag == flag)
                {
                    return true;
                }
            }

            // Flag not found
            return false;
        }
    }*/
}
