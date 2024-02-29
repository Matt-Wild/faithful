using System.IO;
using System.Collections.Generic;
using BepInEx;
using JetBrains.Annotations;

namespace Faithful
{
    internal class Config
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store plugin info
        public PluginInfo pluginInfo;

        // Config tag list
        List<ConfigTag> tags = new List<ConfigTag>();

        // Store default item flags
        ConfigTag defaultItemFlags;

        // Constructor
        public Config(Toolbox _toolbox, PluginInfo _pluginInfo)
        {
            toolbox = _toolbox;

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

        private void ReadConfig()
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
        public bool CheckTag(string _tag)
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
        public bool CheckTagFlag(string _tag, string _flag, bool _isItem = false)
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
        public string ConfigFilePath
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
    }
}
