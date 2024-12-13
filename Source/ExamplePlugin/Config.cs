using System.IO;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Collections;
using Newtonsoft.Json.Linq;
using System;

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
        }

        public static Setting<T> CreateSetting<T>(string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true, bool _isClientSide = false)
        {
            // Check for token in settings dictionary
            if (settings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[CONFIG] - Could not create setting for token '{_token}' as token already exists.");
                return null;
            }

            // Create setting
            Setting<T> setting = new Setting<T>(configFile, _token, _section, _key, _defaultValue, _description, _isStat, _isClientSide);

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

        public static string FormatLanguageToken(string _token, string _tokenPrefix = "")
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

            // Return formatted language string
            return languageString;
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

        public static Dictionary<string, ISetting> GetSettings()
        {
            // Return settings
            return settings;
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

        public Setting(ConfigFile _configFile, string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true, bool _isClientSide = false)
        {
            // Assign config file
            configFile = _configFile;

            // Assign token
            m_token = _token;

            // Set default value
            defaultValue = _defaultValue;

            // Create config entry
            configEntry = configFile.Bind(_section, _key, _defaultValue, _description);

            // Assign if setting is stat
            m_isStat = _isStat;

            // Assign if setting if client side
            m_isClientSide = _isClientSide;
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

        public SettingData GetSettingData()
        {
            // Return setting data made using this setting
            return new SettingData(this);
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
                    return syncedValue;
                }

                // Return value of config entry
                return configEntry.Value;
            }
            set
            {
                // Set value of config entry
                configEntry.Value = value;
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
                return EqualityComparer<T>.Default.Equals(defaultValue, configEntry.Value);
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
