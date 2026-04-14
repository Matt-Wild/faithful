using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal enum Quality
    {
        UNCOMMON,
        RARE,
        EPIC,
        LEGENDARY
    }

    internal struct QualityCounts
    {
        public int UNCOMMON = 0;
        public int RARE = 0;
        public int EPIC = 0;
        public int LEGENDARY = 0;

        public QualityCounts()
        {
        }
    }

    internal class QualityValues<T>
    {
        public T UNCOMMON { get; private set; }
        public T RARE { get; private set; }
        public T EPIC { get; private set; }
        public T LEGENDARY { get; private set; }

        public void UpdateValues(QualitySetting<T> _setting, float _multiplier = 1.0f)
        {
            // Check for setting
            if (_setting == null) return;

            // Update values from setting
            UNCOMMON = ApplyMultiplier(_setting.GetValue(Quality.UNCOMMON), _multiplier);
            RARE = ApplyMultiplier(_setting.GetValue(Quality.RARE), _multiplier);
            EPIC = ApplyMultiplier(_setting.GetValue(Quality.EPIC), _multiplier);
            LEGENDARY = ApplyMultiplier(_setting.GetValue(Quality.LEGENDARY), _multiplier);
        }

        private static T ApplyMultiplier(T value, float multiplier)
        {
            if (typeof(T) == typeof(float))
            {
                float floatValue = (float)(object)value;
                return (T)(object)(floatValue * multiplier);
            }

            if (typeof(T) == typeof(int))
            {
                int intValue = (int)(object)value;
                return (T)(object)Mathf.RoundToInt(intValue * multiplier);
            }

            return value;
        }

        public T GetValue(Quality _quality)
        {
            return _quality switch
            {
                Quality.UNCOMMON => UNCOMMON,
                Quality.RARE => RARE,
                Quality.EPIC => EPIC,
                Quality.LEGENDARY => LEGENDARY,
                _ => default
            };
        }
    }

    internal static class QualityConfig
    {
        // Dictionary of all quality settings
        static Dictionary<string, IQualitySetting> qualitySettings = [];

        public static QualitySetting<T> CreateSetting<T>(string _token, string _section, string _key, T _uncommonDefaultValue, T _rareDefaultValue, T _epicDefaultValue, T _legendaryDefaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Check for token in quality settings dictionary
            if (qualitySettings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[QUALITY CONFIG] - Could not create quality setting for token '{_token}' as token already exists.");
                return null;
            }

            // Create new quality setting and add to dictionary
            QualitySetting<T> qualitySetting = new QualitySetting<T>(_token, _section, _key, _uncommonDefaultValue, _rareDefaultValue, _epicDefaultValue, _legendaryDefaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);
            qualitySettings.Add(_token, qualitySetting);

            // Return quality setting
            return qualitySetting;
        }

        public static QualitySetting<T> FetchSetting<T>(string _token)
        {
            // Check for token in quality settings dictionary
            if (!qualitySettings.ContainsKey(_token))
            {
                // Log warning
                Log.Warning($"[QUALITY CONFIG] - Attempted to fetch quality setting with token '{_token}' but token was not found.");
                return null;
            }

            // Attempt to return casted quality setting
            try
            {
                // Return casted quality setting
                return qualitySettings[_token] as QualitySetting<T>;
            }
            catch
            {
                // Log warning
                Log.Warning($"[QUALITY CONFIG] - Could not fetch quality setting with token '{_token}' as type '{typeof(T).Name}'.");
                return null;
            }
        }
    }

    internal class QualitySetting<T> : IQualitySetting
    {
        // Settings for each quality
        private Dictionary<Quality, Setting<T>> qualitySettings = [];

        // Original token for setting
        private string token;

        public QualitySetting(string _token, string _section, string _key, T _uncommonDefaultValue, T _rareDefaultValue, T _epicDefaultValue, T _legendaryDefaultValue, string _description, bool _isStat = true, bool _isClientSide = false, T _minValue = default, T _maxValue = default, T _randomiserMin = default, T _randomiserMax = default, bool _canRandomise = true, bool _restartRequired = false, string _valueFormatting = "{0:0}")
        {
            // Assign main token
            token = _token;

            // Cycle through qualities and add setting for each
            foreach (Quality quality in System.Enum.GetValues(typeof(Quality)))
            {
                // Get quality name
                string qualityName = quality.ToString();

                // Get default value for quality
                T defaultValue = quality switch
                {
                    Quality.UNCOMMON => _uncommonDefaultValue,
                    Quality.RARE => _rareDefaultValue,
                    Quality.EPIC => _epicDefaultValue,
                    Quality.LEGENDARY => _legendaryDefaultValue,
                    _ => default
                };

                // Create setting for quality
                Setting<T> qualitySetting = Config.CreateSetting($"{_token}_QUALITY_{qualityName}", $"Quality {_section}", $"({qualityName}) {_key}", defaultValue, _description, _isStat, _isClientSide, _minValue, _maxValue, _randomiserMin, _randomiserMax, _canRandomise, _restartRequired, _valueFormatting);

                // Add setting to dictionary
                qualitySettings.Add(quality, qualitySetting);
            }
        }

        public T GetValue(Quality _quality)
        {
            // Check for quality in quality settings dictionary
            if (!qualitySettings.ContainsKey(_quality))
            {
                // Log warning
                Log.Warning($"[QUALITY SETTING] - Attempted to get value for quality '{_quality}' in quality setting with token '{token}' but quality was not found.");
                return default;
            }

            // Return value for quality setting
            return qualitySettings[_quality].Value;
        }

        public string Token
        {
            get
            {
                // Return token
                return token;
            }
        }
    }

    public interface IQualitySetting
    {
        public string Token { get; }
    }
}
