﻿using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;
using R2API;

namespace Faithful
{
    internal static class Assets
    {
        // Asset bundle name
        public const string bundleName = "faithfulbundle";

        // Asset bundle
        public static AssetBundle assetBundle;

        // Store needed RoR2 resources
        public static Material mageJetMaterial;
        public static Wave[] mageJetWaves;
        public static GameObject mageJetAkEventsPrefab;
        public static GameObject radiusIndicatorPrefab;
        public static GameObject pennonEffectPrefab;
        public static GameObject matrixEffectPrefab;

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
                    Log.Debug($"[ASSETS] - Loaded asset '{current}'");
                }
            }

            // Fetch all needed RoR2 resources in advance
            FetchNeededRoR2Resources();
        }

        private static void FetchNeededRoR2Resources()
        {
            // Fetch all needed resources
            mageJetMaterial = Object.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MageBody").GetComponent<Transform>().Find("ModelBase").Find("mdlMage").Find("MageArmature").Find("ROOT").Find("base").Find("stomach").Find("chest").Find("Jets, Right").GetComponent<MeshRenderer>().material);
            mageJetMaterial.SetTexture("_RemapTex", GetTexture("texRamp4T0NFire"));

            mageJetWaves = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MageBody").GetComponent<Transform>().Find("ModelBase").Find("mdlMage").Find("MageArmature").Find("ROOT").Find("base").Find("stomach").Find("chest").Find("JetsOn").Find("Point Light").GetComponent<RoR2.FlickerLight>().sinWaves;

            // Create mage jet ak events prefab
            GameObject mageJetOn = Object.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MageBody").transform.Find("ModelBase").Find("mdlMage").Find("MageArmature").Find("ROOT").Find("base").Find("stomach").Find("chest").Find("JetsOn").gameObject);
            mageJetAkEventsPrefab = mageJetOn.InstantiateClone("faithfulMageJetAkEvents");
            Object.DestroyImmediate(mageJetOn);
            Object.DestroyImmediate(mageJetAkEventsPrefab.transform.Find("Fire").gameObject);
            Object.DestroyImmediate(mageJetAkEventsPrefab.transform.Find("Point Light").gameObject);
            Object.DestroyImmediate(mageJetAkEventsPrefab.transform.Find("JetsL").gameObject);
            Object.DestroyImmediate(mageJetAkEventsPrefab.transform.Find("JetsR").gameObject);
            Object.DestroyImmediate(mageJetAkEventsPrefab.transform.Find("FireRing").gameObject);
            mageJetAkEventsPrefab.transform.SetParent(null);
            mageJetAkEventsPrefab.transform.position = Vector3.zero;
            mageJetAkEventsPrefab.transform.localEulerAngles = Vector3.zero;
            mageJetAkEventsPrefab.transform.localScale = Vector3.zero;
            Object.DestroyImmediate(mageJetAkEventsPrefab.GetComponent<Rigidbody>());

            // Create Leader's Pennon effect
            GameObject tempLPEffect = Object.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/WarbannerBuffEffect"));
            pennonEffectPrefab = tempLPEffect.InstantiateClone("faithfulLeadersPennonEffect");
            Object.DestroyImmediate(tempLPEffect);
            pennonEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(0.58039215f, 0.22745098f, 0.71764705f));
            pennonEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(0.58039215f, 0.22745098f, 0.71764705f));
            pennonEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", GetTexture("texRampPennonBuff"));
            pennonEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", GetTexture("texIndicatorPennonMask"));
            pennonEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.mainTexture = GetTexture("texIndicatorPennonMask");
            pennonEffectPrefab.transform.Find("Visual").Find("ColoredLightShafts (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(0.1f, 0.0f, 2.63840857f));
            pennonEffectPrefab.transform.Find("Visual").Find("ColoredLightShafts (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(0.1f, 0.0f, 2.63840857f));
            pennonEffectPrefab.transform.Find("Visual").Find("FlarePerst_Ps (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(0.1f, 0.0f, 1.0f));
            pennonEffectPrefab.transform.Find("Visual").Find("FlarePerst_Ps (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(0.1f, 0.0f, 1.0f));
            pennonEffectPrefab.transform.Find("Visual").Find("SoftGlow").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(0.58039215f, 0.22745098f, 1.0f));
            pennonEffectPrefab.transform.Find("Visual").Find("SoftGlow").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(0.58039215f, 0.22745098f, 1.0f));

            // Create Targeting Matrix effect
            GameObject tempTMEffect = Object.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/TemporaryVisualEffects/WarbannerBuffEffect"));
            matrixEffectPrefab = tempTMEffect.InstantiateClone("faithfulTargetingMatrixEffect");
            Object.DestroyImmediate(tempTMEffect);
            matrixEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", GetTexture("texRampPennonBuff"));
            matrixEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.SetTexture("_MainTex", GetTexture("texIndicatorMatrixMask"));
            matrixEffectPrefab.transform.Find("Visual").Find("PulseEffect, Ring").GetComponent<ParticleSystemRenderer>().material.mainTexture = GetTexture("texIndicatorMatrixMask");
            matrixEffectPrefab.transform.Find("Visual").Find("ColoredLightShafts (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("ColoredLightShafts (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("FlarePerst_Ps (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("FlarePerst_Ps (1)").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("SoftGlow").GetComponent<ParticleSystemRenderer>().material.SetColor("_Color", new Color(1.0f, 0.0f, 0.0f));
            matrixEffectPrefab.transform.Find("Visual").Find("SoftGlow").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f));

            // Create radius indicator prefab
            GameObject radiusIndicator = Object.Instantiate(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator"));
            radiusIndicatorPrefab = radiusIndicator.InstantiateClone("faithfulRadiusIndicator");
            Object.DestroyImmediate(radiusIndicator);
            Object.DestroyImmediate(radiusIndicatorPrefab.GetComponent<RoR2.NetworkedBodyAttachment>());
            Object.DestroyImmediate(radiusIndicatorPrefab.GetComponent<NetworkIdentity>());
            radiusIndicatorPrefab.transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
            radiusIndicatorPrefab.transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_TintColor", Color.white);
            radiusIndicatorPrefab.transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
            radiusIndicatorPrefab.transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_TintColor", Color.white);
            radiusIndicatorPrefab.transform.position = Vector3.zero;
            radiusIndicatorPrefab.transform.eulerAngles = Vector3.zero;
            radiusIndicatorPrefab.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            radiusIndicatorPrefab.AddComponent<FaithfulRadiusIndicatorBehaviour>();

            // Check if debug mode
            if (Utils.debugMode)
            {
                // Log confirmation
                Log.Debug("[ASSETS] - Fetched all needed resources from RoR2 assets.");
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
            // Ensure lower case
            _file = _file.ToLower();

            // Cycle through asset bundle
            foreach (string name in assetBundle.GetAllAssetNames())
            {
                // Is correct asset?
                if (name.ToLower().Contains(_file))
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

        public static void LogRoR2Resources()
        {
            // Define list of paths to resources and their GUIDs
            List<KeyValuePair<string, string>> resources = new List<KeyValuePair<string, string>>();
            RoR2.LegacyResourcesAPI.GetAllPathGuidPairs(resources);

            // Cycle through resources
            foreach (KeyValuePair<string, string> resource in resources)
            {
                // Log details about resource
                Log.Debug($"[ASSETS] - Resource found: '{resource.Key}' | GUID: {resource.Value}");
            }
        }

        public static void FindRoR2Resources(string _searchTerm)
        {
            // Define list of paths to resources and their GUIDs
            List<KeyValuePair<string, string>> resources = new List<KeyValuePair<string, string>>();
            RoR2.LegacyResourcesAPI.GetAllPathGuidPairs(resources);

            // Create message string
            string message = $"\n[ASSETS]\n====================\nAttempting to find resources with search term: '{_searchTerm}'\n--------------------";

            // Store if found a matching resources
            bool found = false;

            // Cycle through resources
            foreach (KeyValuePair<string, string> resource in resources)
            {
                // Check if resource contains search term
                if (resource.Key.ToUpper().Contains(_searchTerm.ToUpper()))
                {
                    // Found matching resource
                    found = true;

                    // Add to message string
                    message += $"\nResource found: '{resource.Key}' | GUID: {resource.Value}";
                }
            }

            // End message string
            message += found ? "\n====================" : "\nNo resources found...\n====================";

            // Log message string
            Log.Info(message);
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

        public static Texture GetTexture(string _name)
        {
            // Add file extension
            string fullName = _name + ".png";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                Log.Error($"Requested asset '{fullName}' could not be found.");
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<Texture>(asset);
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