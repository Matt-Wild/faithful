using UnityEngine;
using System.IO;
using BepInEx;

namespace Faithful
{
    public class Assets
    {
        // Store plugin info
        public PluginInfo PInfo;

        // Asset bundle name
        public const string bundleName = "faithfulbundle";

        // Asset bundle
        public AssetBundle assetBundle;

        // Get the path to the asset bundle
        public string AssetBundlePath
        {
            get
            {
                // Returns the path to the asset bundle
                return Path.Combine(Path.GetDirectoryName(PInfo.Location), bundleName);
            }
        }

        public void Init(PluginInfo _pluginInfo)
        {
            // Get plugin info
            PInfo = _pluginInfo;

            // Loads the assetBundle from the path
            assetBundle = AssetBundle.LoadFromFile(AssetBundlePath);

            // DEBUG loading assets
            string[] loaded = assetBundle.GetAllAssetNames();
            foreach (string current in loaded)
            {
                Log.Debug($"Loaded asset '{current}'");
            }
        }

        public bool HasAsset(string _name)
        {
            // Returns if asset bundle has asset
            return assetBundle.Contains(_name);
        }

        public Sprite GetIcon(string _name)
        {
            // Check for asset
            if(!HasAsset(_name))
            {
                Log.Error($"Requested asset '{_name}' could not be found.");
            }

            // Return asset
            return assetBundle.LoadAsset<Sprite>(_name);
        }

        public GameObject GetModel(string _name)
        {
            // Check for asset
            if (!HasAsset(_name))
            {
                Log.Error($"Requested asset '{_name}' could not be found.");
            }

            // Return asset
            return assetBundle.LoadAsset<GameObject>(_name);
        }
    }
}