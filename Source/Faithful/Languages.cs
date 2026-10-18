using Newtonsoft.Json;
using R2API;
using RoR2;
using System.Collections.Generic;

namespace Faithful
{
    public static class Languages
    {
        // Languages supported by Faithful
        private static string[] SUPPORTED_LANGUAGES =
        [
            "en",
            "zh-CN",
            "RU"
        ];

        // Dictionaries for language tokens
        private static Dictionary<string, Dictionary<string, string>> dictionaries = [];

        public static void Init()
        {
            // Get base path for languages
            string basePath = System.IO.Path.GetDirectoryName(Utils.pluginInfo.Location) + "/plugins/Languages/";

            // Get the backup base paths for languages
            string backupBasePath1 = System.IO.Path.GetDirectoryName(Utils.pluginInfo.Location) + "/Languages/";
            string backupBasePath2 = System.IO.Path.GetDirectoryName(Utils.pluginInfo.Location) + "/";

            // Cycle through all supported languages and load them
            foreach (string lang in SUPPORTED_LANGUAGES)
            {
                // Get full path to language file
                string langPath = basePath + $"{lang}/Faithful_{lang}.language";

                // Check if file exists
                if (!System.IO.File.Exists(langPath))
                {
                    // Record failed path
                    string failedPath1 = langPath;

                    // Attempt to fetch the language file from the first backup path
                    langPath = backupBasePath1 + $"{lang}/Faithful_{lang}.language";

                    // Check if backup file exists
                    if (!System.IO.File.Exists(langPath))
                    {
                        // Record failed path
                        string failedPath2 = langPath;

                        // Attempt to fetch the language file from the second backup path
                        langPath = backupBasePath2 + $"Faithful_{lang}.language";

                        // Check if backup file exists
                        if (!System.IO.File.Exists(langPath))
                        {
                            Log.Error($"[LANGUAGES] - Could not find language file for language '{lang}' at path: '{failedPath1}' or '{failedPath2}' or '{langPath}'");
                            continue;
                        }
                    }
                }

                // Add to R2API
                LanguageAPI.AddPath(langPath);

                try
                {
                    // Read file
                    string json = System.IO.File.ReadAllText(langPath);

                    // Deserialize the JSON
                    Dictionary<string, Dictionary<string, string>> sections = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);

                    if (sections == null)
                    {
                        Log.Error($"[LANGUAGES] - Failed to deserialize '{langPath}'");
                        continue;
                    }

                    string targetSection = lang == "en" ? "strings" : lang;

                    if (!sections.TryGetValue(targetSection, out Dictionary<string, string> dict))
                    {
                        Log.Error($"[LANGUAGES] - Section '{targetSection}' not found in '{langPath}'");
                        continue;
                    }

                    // Add to dictionaries
                    dictionaries.Add(lang, dict);
                }
                catch (JsonReaderException ex)
                {
                    Log.Error($"[LANGUAGES] - Failed to parse language file for language '{lang}' at path: {langPath}");
                    Log.Error($"Path: {ex.Path} | Line: {ex.LineNumber} | Position: {ex.LinePosition}");
                    Log.Error(ex.ToString());
                }
            }

            // Hook on language changed behaviour
            Language.onCurrentLanguageChanged += OnCurrentLanguageChanged;

            if (Utils.verboseConsole) Log.Debug("Languages initialised");
        }

        private static void OnCurrentLanguageChanged()
        {
            // Refresh item settings (to update item texts etc)
            Utils.RefreshItemSettings();
        }

        public static bool HasLanguageString(string _token)
        {
            // Return if language dictionary has string
            return GetLanguageDictionary().ContainsKey(_token);
        }

        public static string GetLanguageString(string _token, bool _prioritiseRoR2Language = false, bool _warn = true)
        {
            // Get current language dictionary
            Dictionary<string, string> languageDictionary = GetLanguageDictionary();

            // Check if should prioritise using RoR2.Language
            if (_prioritiseRoR2Language)
            {
                // First attempt to get from RoR2.Language
                string result = Language.GetString(_token);
                if (result != _token)
                {
                    // Found string
                    return result;
                }

                // Check for token
                if (languageDictionary.ContainsKey(_token))
                {
                    // Return corresponding value
                    return languageDictionary[_token];
                }

                // Token not found
                else
                {
                    if (_warn) Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");
                    return _token; // Return original token
                }
            }

            else
            {
                // Check for token
                if (languageDictionary.ContainsKey(_token))
                {
                    // Return corresponding value
                    return languageDictionary[_token];
                }

                // Token not found
                else
                {
                    if (_warn) Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");
                    return Language.GetString(_token); // Revert to RoR2.Language
                }
            }
        }

        public static string GetXMLLanguageString(string _token)
        {
            // Get current language dictionary
            Dictionary<string, string> languageDictionary = GetLanguageDictionary();

            // Check for token
            if (languageDictionary.ContainsKey(_token))
            {
                // Get string
                string languageString = languageDictionary[_token];

                // Return XML safe language string
                return Utils.GetXMLSafeString(languageString);
            }

            // Token not found
            else
            {
                Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");
                return _token; // Return original token
            }
        }

        public static Dictionary<string, string> GetLanguageDictionary()
        {
            // Check if the current language has a dictionary
            if (dictionaries.ContainsKey(Language.currentLanguageName))
            {
                return dictionaries[Language.currentLanguageName];
            }

            // Otherwise, revert to English
            if (dictionaries.ContainsKey("en"))
            {
                return dictionaries["en"];
            }

            // If English isn't even there, return an empty dictionary and log an error
            Log.Error($"[LANGUAGES] - Could not find a language dictionary for the current language '{Language.currentLanguageName}', and English was not found as a fallback. Returning an empty dictionary.");
            return [];
        }
    }
}
