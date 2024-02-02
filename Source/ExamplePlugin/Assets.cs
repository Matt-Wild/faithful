using UnityEngine;
using System.IO;
using BepInEx;

namespace Faithful
{
    internal class Assets
    {
        // Toolbox
        protected Toolbox toolbox;

        // Asset bundle name
        public const string bundleName = "faithfulbundle";

        // Asset bundle
        public AssetBundle assetBundle;

        public Assets(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Loads the assetBundle from the path
            assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            // DEBUG display loading assets
            string[] loaded = assetBundle.GetAllAssetNames();
            foreach (string current in loaded)
            {
                Log.Debug($"Loaded asset '{current}'");
            }
        }

        // Get the path to the asset bundle
        public string AssetBundlePath
        {
            get
            {
                // Returns the path to the asset bundle
                return Path.Combine(Path.GetDirectoryName(toolbox.utils.pluginInfo.Location), bundleName);
            }
        }

        public string FindAsset(string _file)
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

        public bool HasAsset(string _name)
        {
            // Returns if asset bundle has asset
            return assetBundle.Contains(_name);
        }

        public Sprite GetIcon(string _name)
        {
            // Add file extension
            string fullName = _name + ".png";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if(asset == null)
            {
                Log.Error($"Requested asset '{fullName}' could not be found.");
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<Sprite>(asset);
        }

        public GameObject GetModel(string _name)
        {
            // Add file extension
            string fullName = _name + ".prefab";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                Log.Error($"Requested asset '{fullName}' could not be found.");
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<GameObject>(asset);
        }

        public Shader GetShader(string _name)
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