using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Collections.Generic;
using R2API;
using UnityEngine.AddressableAssets;
using RoR2.ExpansionManagement;
using System.Reflection;

namespace Faithful
{
    internal static class Assets
    {
        // Asset bundle name
        public const string bundleName = "faithfulbundle";

        // Asset bundle
        public static AssetBundle assetBundle;
        
        // Store needed RoR2 resources / modified resources
        public static Material mageJetMaterial;
        public static Wave[] mageJetWaves;
        public static GameObject mageJetAkEventsPrefab;
        public static GameObject radiusIndicatorPrefab;
        public static GameObject pennonEffectPrefab;
        public static GameObject matrixEffectPrefab;
        public static GameObject shrineUseEffectPrefab;
        public static GameObject technicianArcPrefab;
        public static PhysicMaterial ragdollMaterial;

        // Store useful lookup assets
        public static DynamicBone scarfDynamicBone;

        // Ping icons
        public static Sprite dronePingIcon;
        public static Sprite inventoryPingIcon;
        public static Sprite lootPingIcon;
        public static Sprite shrinePingIcon;
        public static Sprite teleporterPingIcon;
        public static Sprite mysteryPingIcon;

        // Inspect icons
        public static Sprite chestInspectIcon;
        public static Sprite droneInspectIcon;
        public static Sprite lunarInspectIcon;
        public static Sprite pillarInspectIcon;
        public static Sprite printerInspectIcon;
        public static Sprite radioScannerInspectIcon;
        public static Sprite scapperInspectIcon;
        public static Sprite shrineInspectIcon;
        public static Sprite meridianInspectIcon;
        public static Sprite voidInspectIcon;
        public static Sprite mysteryInspectIcon;

        // Interactable symbol materials
        public static Material chanceShrineSymbolMaterial;

        // Expansion definitions
        public static ExpansionDef sotvDef;
        public static ExpansionDef sotsDef;

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

            // Update mod icon for Risk of Options
            RiskOfOptionsWrapper.UpdateModIcon();
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

            // Get shrine use effect prefab
            shrineUseEffectPrefab = FetchAsset<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab");

            // Get and modify engi turret laser prefab to become Technician's Arc
            GameObject turretLaser = Object.Instantiate(FetchAsset<GameObject>("RoR2/Base/Engi/LaserEngiTurret.prefab"));
            technicianArcPrefab = turretLaser.InstantiateClone("faithfulTechnicianArc");
            Object.DestroyImmediate(turretLaser);
            technicianArcPrefab.transform.Find("LaserStart").GetComponent<LineRenderer>().widthMultiplier = 4.0f;
            technicianArcPrefab.transform.Find("LaserStart").GetComponent<LineRenderer>().material.SetColor("_TintColor", new Color(0.9725f, 1.0f, 0.5373f));
            technicianArcPrefab.transform.Find("LaserStart").GetComponent<LineRenderer>().material.SetTexture("_RemapTex", GetTexture("texRampTechnician"));
            technicianArcPrefab.transform.Find("LaserStart").GetComponent<LineRenderer>().material.SetTexture("_Cloud2Tex", GetTexture("texLightningTechnician"));

            // Get physics material for ragdoll bodies
            ragdollMaterial = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RoR2.RagdollController>().bones[1].GetComponent<Collider>().material;

            // Fetch ego scarf asset
            scarfDynamicBone = Utils.FindChildByName(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/LunarSun/DisplaySunHeadNeck.prefab").WaitForCompletion().transform, "Bandage1").GetComponent<DynamicBone>();

            // Fetch ping icons
            dronePingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texDroneIconOutlined.png");
            inventoryPingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texInventoryIconOutlined.png");
            lootPingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texLootIconOutlined.png");
            shrinePingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texShrineIconOutlined.png");
            teleporterPingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texTeleporterIconOutlined.png");
            mysteryPingIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png");

            // Fetch inspect icons
            chestInspectIcon = FetchAsset<Sprite>("RoR2/Base/ChestIcon_1.png");
            droneInspectIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texDroneIconOutlined.png");
            lunarInspectIcon = FetchAsset<Sprite>("RoR2/Base/LunarIcon_1.png");
            pillarInspectIcon = FetchAsset<Sprite>("RoR2/Base/PillarIcon.png");
            printerInspectIcon = FetchAsset<Sprite>("RoR2/Base/PrinterIcon_1.png");
            radioScannerInspectIcon = FetchAsset<Sprite>("RoR2/Base/RadioScannerIcon_2.png");
            scapperInspectIcon = FetchAsset<Sprite>("RoR2/Base/ScrapperIcon.png");
            shrineInspectIcon = FetchAsset<Sprite>("RoR2/Base/ShrineIcon.png");
            meridianInspectIcon = FetchAsset<Sprite>("RoR2/DLC2/texColossusExpansionIcon.png");
            voidInspectIcon = FetchAsset<Sprite>("RoR2/Base/VoidIcon_2.png");
            mysteryInspectIcon = FetchAsset<Sprite>("RoR2/Base/Common/MiscIcons/texMysteryIcon.png");

            // Fetch interactable symbol materials
            chanceShrineSymbolMaterial = FetchAsset<Material>("RoR2/Base/ShrineChance/matShrineChanceSymbol.mat");

            // Get expansion definitions
            sotvDef = FetchAsset<ExpansionDef>("RoR2/DLC1/Common/DLC1.asset");
            sotsDef = FetchAsset<ExpansionDef>("RoR2/DLC2/Common/DLC2.asset");

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

        public static T FetchAsset<T>(string _path)
        {
            // Fetch asset from addressables
            return Addressables.LoadAssetAsync<T>(_path).WaitForCompletion();
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

        public static GameObject GetCrosshair(string _crosshairName)
        {
            // Attempt to fetch crosshair
            GameObject loadedCrosshair = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/" + _crosshairName + "Crosshair");

            // Check for crosshair
            if (loadedCrosshair == null)
            {
                // Warn and return default crosshair
                Log.Error($"Crosshair '{_crosshairName}' could not be found - defaulting to standard crosshair.");
                return RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            }

            // Return crosshair
            return loadedCrosshair;
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

        public static Mesh GetMesh(string _name, string _default = null)
        {
            // Add file extension
            string fullName = _name + ".prefab";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Initialise prefab
            GameObject prefab = null;

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
                    // Get default asset
                    prefab = assetBundle.LoadAsset<GameObject>(_default);
                }
            }
            else
            {
                // Load asset
                prefab = assetBundle.LoadAsset<GameObject>(asset);
            }
            

            // Check for asset
            if (prefab == null) return null;

            // Get mesh filter from asset
            MeshFilter filter = prefab.GetComponent<MeshFilter>();

            // Check for mesh filter
            if (filter == null)
            {
                // Error and return - Unsuccessful
                Log.Error($"[ASSETS] | Unable to get mesh for asset '{fullName}' - Mesh filter component not found!");
                return null;
            }

            // Return mesh from mesh filter
            return filter.sharedMesh;
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

        public static Sprite GetSprite(string _name)
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
            return assetBundle.LoadAsset<Sprite>(asset);
        }

        public static Material GetMaterial(string _name)
        {
            // Add file extension
            string fullName = _name + ".mat";

            // Attempt to find asset
            string asset = FindAsset(fullName);

            // Check for asset
            if (asset == null)
            {
                Log.Error($"Requested asset '{fullName}' could not be found.");
                return null;
            }

            // Return asset
            return assetBundle.LoadAsset<Material>(asset);
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

        public static Material GetShrineSymbolMaterial(Texture _texture, Color _colour)
        {
            // Clone material and modify main texture
            Material clonedMaterial = new Material(chanceShrineSymbolMaterial);
            clonedMaterial.SetTexture("_MainTex", _texture);
            clonedMaterial.SetColor("_TintColor", _colour);

            // Return cloned material
            return clonedMaterial;
        }

        public static GameObject GetClonedDopplegangerMaster(GameObject _bodyPrefab, string _masterName, string _masterToCopy)
        {
            // Create new cloned doppleganger master and assign body prefab
            GameObject newMaster = PrefabAPI.InstantiateClone(RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/" + _masterToCopy + "MonsterMaster"), _masterName, true);
            newMaster.GetComponent<RoR2.CharacterMaster>().bodyPrefab = _bodyPrefab;

            // Add to content pack
            ContentAddition.AddMaster(newMaster);

            // Return cloned master
            return newMaster;
        }
    }
}