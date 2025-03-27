using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    // This is mostly yoinked from Henry Mod
    internal static class ItemDisplays
    {
        private static Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();
        public static Dictionary<Object, ItemDisplayRule[]> keyAssetDisplayPrefabs = new Dictionary<Object, ItemDisplayRule[]>();
        public static Dictionary<string, Object> keyAssets = new Dictionary<string, Object>();

        public static int queuedDisplays;

        // Has this class been set up
        public static bool initialised = false;

        // Every character with item displays will call this as a precaution
        public static void LazyInit()
        {
            // Ignore if already initialised
            if (initialised) return;

            // Update initialised
            initialised = true;

            // Fetch the default item displays
            FetchDisplays();
        }

        internal static void DisposeWhenDone()
        {
            // Reduce display queue and check is queue is empty
            queuedDisplays--;
            if (queuedDisplays > 0) return;

            // Queue is empty

            // Ignore is not initialised
            if (!initialised) return;

            // Set as no longer initialised
            initialised = false;

            // Remove all item display references
            itemDisplayPrefabs = null;
            keyAssetDisplayPrefabs = null;
            keyAssets = null;
        }

        internal static void FetchDisplays()
        {
            // Get default item display assets from existing character body
            FetchDisplaysFromBody("LoaderBody");

            // Add support for custom bones having capictor 
            SetupCustomLightningArm();

            // [custom item display stuff here]
        }

        private static void FetchDisplaysFromBody(string _bodyName)
        {
            // Get item display information
            ItemDisplayRuleSet itemDisplayRuleSet = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/" + _bodyName).GetComponent<ModelLocator>().modelTransform.GetComponent<CharacterModel>().itemDisplayRuleSet;

            // Cycle through item rule groups
            ItemDisplayRuleSet.KeyAssetRuleGroup[] itemRuleGroups = itemDisplayRuleSet.keyAssetRuleGroups;
            for (int i = 0; i < itemRuleGroups.Length; i++)
            {
                // Get rules
                ItemDisplayRule[] rules = itemRuleGroups[i].displayRuleGroup.rules;

                // Store rules and key assets
                keyAssetDisplayPrefabs[itemRuleGroups[i].keyAsset] = rules;
                keyAssets[itemRuleGroups[i].keyAsset.name] = itemRuleGroups[i].keyAsset;

                // Cycle through rules
                for (int j = 0; j < rules.Length; j++)
                {
                    // Check for follower prefab
                    GameObject followerPrefab = rules[j].followerPrefab;
                    if (followerPrefab)
                    {
                        // Store follower prefab in item displays prefabs if not already stored
                        string key = followerPrefab.name?.ToLowerInvariant();
                        if (!itemDisplayPrefabs.ContainsKey(key))
                        {
                            itemDisplayPrefabs[key] = followerPrefab;
                        }
                    }
                }
            }
        }

        private static void SetupCustomLightningArm()
        {
            // This function allows the capacitor item display effect to cover custom bones (e.g. "LightningJoint1" etc)

            // Create cloned item display effect for the capicitor lightning
            GameObject display = R2API.PrefabAPI.InstantiateClone(itemDisplayPrefabs["displaylightningarmright"], "DisplayLightningCustom", false);

            // Setup limb matcher to work with custom bones
            LimbMatcher limbMatcher = display.GetComponent<LimbMatcher>();
            limbMatcher.limbPairs[0].targetChildLimb = "LightningJoint1";
            limbMatcher.limbPairs[1].targetChildLimb = "LightningJoint2";
            limbMatcher.limbPairs[2].targetChildLimb = "LightningJointEnd";

            // Add to display prefabs list
            itemDisplayPrefabs["displaylightningarmcustom"] = display;
        }

        public static GameObject LoadDisplay(string _name)
        {
            // Check for item display prefab
            if (itemDisplayPrefabs.ContainsKey(_name.ToLowerInvariant()))
            {
                // Get display object
                GameObject display = itemDisplayPrefabs[_name.ToLowerInvariant()];

                // Check if display object exists
                if (display != null) return display;

                // Error and return null - display prefab is null
                Log.Error($"[CHARACTER ITEM DISPLAYS] | Item display '{_name}' is null!");
                return null;
            }

            // Unable to find display prefab
            Log.Error($"[CHARACTER ITEM DISPLAYS] | Item display '{_name}' could not be found!");
            return null;
        }

        public static ItemDisplayRuleSet.KeyAssetRuleGroup CreateDisplayRuleGroupWithRules(string _itemName, params ItemDisplayRule[] _rules) => CreateDisplayRuleGroupWithRules(GetKeyAssetFromString(_itemName), _rules);
        public static ItemDisplayRuleSet.KeyAssetRuleGroup CreateDisplayRuleGroupWithRules(Object _keyAsset, params ItemDisplayRule[] _rules)
        {
            // Check for key asset
            if (_keyAsset == null) Log.Error($"[CHARACTER ITEM DISPLAYS] | Null key asset provided when creating a display rule group!");

            // Construct and return key asset rule group
            return new ItemDisplayRuleSet.KeyAssetRuleGroup
            {
                keyAsset = _keyAsset,
                displayRuleGroup = new DisplayRuleGroup
                {
                    rules = _rules
                }
            };
        }

        public static ItemDisplayRule CreateDisplayRule(string _prefabName, string _childName, Vector3 _position, Vector3 _rotation, Vector3 _scale) => CreateDisplayRule(LoadDisplay(_prefabName), _childName, _position, _rotation, _scale);
        public static ItemDisplayRule CreateDisplayRule(GameObject _itemPrefab, string _childName, Vector3 _position, Vector3 _rotation, Vector3 _scale)
        {
            // Construct and return item display rule
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.ParentedPrefab,
                childName = _childName,
                followerPrefab = _itemPrefab,
                limbMask = LimbFlags.None,
                localPos = _position,
                localAngles = _rotation,
                localScale = _scale
            };
        }

        public static ItemDisplayRule CreateLimbMaskDisplayRule(LimbFlags _limb)
        {
            // Construct and return item display rule
            return new ItemDisplayRule
            {
                ruleType = ItemDisplayRuleType.LimbMask,
                limbMask = _limb,
                childName = "",
                followerPrefab = null
            };
        }

        private static Object GetKeyAssetFromString(string _itemName)
        {
            // Attempt to fetch as item def
            Object keyAsset = LegacyResourcesAPI.Load<ItemDef>("ItemDefs/" + _itemName);

            // Check if not found
            if (keyAsset == null)
            {
                // Attempt to fetch as equipment def
                keyAsset = LegacyResourcesAPI.Load<EquipmentDef>("EquipmentDefs/" + _itemName);
            }

            // Check if found
            if (keyAsset == null)
            {
                // Error
                Log.Error($"[CHARACTER ITEM DISPLAYS] | Could not find keyasset for '{_itemName}'!");
            }

            // Return key asset
            return keyAsset;
        }
    }
}
