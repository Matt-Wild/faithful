﻿using System.IO;
using System.Collections.Generic;
using BepInEx.Configuration;

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

        public static Setting<T> CreateSetting<T>(string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true)
        {
            // Check for token in settings dictionary
            if (settings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[CONFIG] - Could not create setting for token '{_token}' as token already exists.");
                return null;
            }

            // Create setting
            Setting<T> setting = new Setting<T>(configFile, _token, _section, _key, _defaultValue, _description, _isStat);

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
    }

    internal class Setting<T> : ISetting
    {
        // Store config file
        private ConfigFile configFile;

        // Store token
        public string token;

        // Store config entry
        public ConfigEntry<T> configEntry;

        // Store default value
        public T defaultValue;

        // Store if setting is stat
        private bool m_isStat;

        public Setting(ConfigFile _configFile, string _token, string _section, string _key, T _defaultValue, string _description, bool _isStat = true)
        {
            // Assign config file
            configFile = _configFile;

            // Assign token
            token = _token;

            // Set default value
            defaultValue = _defaultValue;

            // Create config entry
            configEntry = configFile.Bind(_section, _key, _defaultValue, _description);

            // Assign if setting is stat
            m_isStat = _isStat;
        }

        public void Delete()
        {
            // Delete this setting
            configFile.Remove(configEntry.Definition);
        }

        public T Value
        {
            get
            {
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
                return configEntry.Value;
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
    }

    public interface ISetting
    {
        public object ValueObject { get; }

        public bool isDefault { get; }

        public bool isStat { get; }
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
