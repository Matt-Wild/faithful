using UnityEngine;
using System.IO;
using BepInEx;
using IL.RoR2.ConVar;

namespace Faithful
{
    internal static class Assets
    {
        // Asset bundle name
        public const string bundleName = "faithfulbundle";

        // Asset bundle
        public static AssetBundle assetBundle;

        // Default assets
        private const string defaultModel = "temporalcubemesh";
        private const string defaultIcon = "textemporalcubeicon";
        private const string defaultConsumedIcon = "textemporalcubeconsumedicon";
        private const string defaultBuffIcon = "texbufftemporalcube";

        public static void Init()
        {
            // Loads the assetBundle from the path
            assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            // DEBUG display loading assets
            if (Utils.debugMode)
            {
                string[] loaded = assetBundle.GetAllAssetNames();
                foreach (string current in loaded)
                {
                    Log.Debug($"Loaded asset '{current}'");
                }
            }
        }

        // Get the path to the asset bundle
        public static string AssetBundlePath
        {
            get
            {
                // Returns the path to the asset bundle
                return Path.Combine(Path.GetDirectoryName(Utils.pluginInfo.Location), bundleName);
            }
        }

        public static string FindAsset(string _file)
        {
            // Cycle through asset bundle
            foreach (string name in assetBundle.GetAllAssetNames())
            {
                // Is correct asset?
                if (name.Contains(_file))
                {
                    return name;
                }
            }

            // Return null if not found
            return null;
        }

        public static bool HasAsset(string _name)
        {
            // Attempt to find asset
            string found = FindAsset(_name);

            // Return if found
            return found != null;
        }

        public static Sprite GetIcon(string _name)
        {
            // Add file extension
            string fullName = _name + ".png";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                if (Utils.debugMode)
                {
                    Log.Error($"Requested asset '{fullName}' could not be found.");
                }

                // Force name to lower case
                _name = _name.ToLower();

                // Return default asset
                return assetBundle.LoadAsset<Sprite>(_name.Contains("buff") ? FindAsset(defaultBuffIcon) : _name.Contains("consumed") ? FindAsset(defaultConsumedIcon) : FindAsset(defaultIcon));
            }

            // Return asset
            return assetBundle.LoadAsset<Sprite>(asset);
        }

        public static GameObject GetObject(string _name, string _default = null)
        {
            // Add file extension
            string fullName = _name + ".prefab";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                if (Utils.debugMode)
                {
                    Log.Error($"Requested asset '{fullName}' could not be found.");
                }

                // Check for default asset
                if (_default != null)
                {
                    // Return default asset
                    return assetBundle.LoadAsset<GameObject>(_default);
                }

                // Otherwise send error and return null
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<GameObject>(asset);
        }

        public static GameObject GetModel(string _name)
        {
            // Load object and return with default model as fallback
            return GetObject(_name, defaultModel);
        }

        public static Shader GetShader(string _name)
        {
            // Add file extension
            string fullName = _name + ".shader";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                Log.Error($"Requested asset '{fullName}' could not be found.");
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<Shader>(asset);
        }
    }
}