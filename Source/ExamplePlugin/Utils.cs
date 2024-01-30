using BepInEx;
using HarmonyLib;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Faithful
{
    internal class Utils
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store plugin info
        public PluginInfo pluginInfo;

        // Store debug mode
        private const bool _debugMode = true;

        // Simulacrum banned items
        List<ItemDef> simulacrumBanned = new List<ItemDef>();

        // Corruption item pairs
        List<CorruptPair> corruptionPairs = new List<CorruptPair>();

        // Character model names
        private Dictionary<string, string> characterModelNames = new Dictionary<string, string>()
        {
            { "commando", "mdlCommandoDualies" },
            { "huntress", "mdlHuntress" },
            { "bandit", "mdlBandit2" },
            { "mul-t", "mdlToolbot" },
            { "engineer", "mdlEngi" },
            { "turret", "mdlEngiTurret" },
            { "artificer", "mdlMage" },
            { "mercenary", "mdlMerc" },
            { "rex", "mdlTreebot" },
            { "loader", "mdlLoader" },
            { "acrid", "mdlCroco" },
            { "captain", "mdlCaptain" },
            { "railgunner", "mdlRailGunner" },
            { "void fiend", "mdlVoidSurvivor" },
            { "scavenger", "mdlScav" }
        };

        // Constructor
        public Utils(Toolbox _toolbox, PluginInfo _pluginInfo)
        {
            toolbox = _toolbox;

            // Set plugin info
            pluginInfo = _pluginInfo;

            // Config Simulacrum
            On.RoR2.InfiniteTowerRun.OverrideRuleChoices += InjectSimulacrumBannedItems;

            // Config item corruptions
            On.RoR2.Items.ContagiousItemManager.Init += SetupItemCorruptions;

            Log.Debug("Utils initialised");
        }

        // Refresh chosen buff on chosen character
        public void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0)
            {
                return; // Body not valid
            }

            // Cycle through buffs
            foreach (CharacterBody.TimedBuff buff in body.timedBuffs)
            {
                // Check if correct buff
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    // Refresh buff
                    buff.timer = duration;
                }
            }
        }

        public void BanFromSimulacrum(ItemDef _item)
        {
            // Add item def to banned list for Simulacrum
            simulacrumBanned.Add(_item);
        }

        public void AddCorruptionPair(ItemDef _corrupter, string _corruptedToken)
        {
            // Add item pair to corruption pairs
            corruptionPairs.Add(new CorruptPair(_corrupter, _corruptedToken));
        }

        void InjectSimulacrumBannedItems(On.RoR2.InfiniteTowerRun.orig_OverrideRuleChoices orig, InfiniteTowerRun self, RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
        {
            // List of items needing to be banned
            List<ItemDef> newBanned = [];

            // Cycle through current banned items
            foreach (ItemDef target in simulacrumBanned)
            {
                // Contains item?
                if (!self.blacklistedItems.Contains(target))
                {
                    Log.Debug($"Banning {target.nameToken} from Simulacrum");
                    // Add to needed list for banning
                    newBanned.Add(target);
                }
            }

            // Needs to ban extra items
            if (newBanned.Count > 0)
            {
                // Add existing banned items to 
                foreach (ItemDef current in self.blacklistedItems)
                {
                    // Update new banned list
                    newBanned.Add(current);
                }

                // Update blacklisted items
                self.blacklistedItems = newBanned.ToArray();
            }

            orig(self, mustInclude, mustExclude, runSeed);  // Run normal processes
        }

        private void SetupItemCorruptions(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
        {
            // Create item pair list
            List<ItemDef.Pair> itemPairs = new List<ItemDef.Pair>();

            // Cycle through corruption pairs
            foreach (CorruptPair pair in corruptionPairs)
            {
                // Add to item pairs
                itemPairs.Add(pair.GetPair());
            }

            // Append corruption pairs to contagious items pair array
            ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem] = ItemCatalog.itemRelationships[DLC1Content.ItemRelationshipTypes.ContagiousItem].AddRangeToArray(itemPairs.ToArray());

            Log.Debug("Added item corruptions");

            orig(); // Run normal processes
        }

        public ItemDisplaySettings CreateItemDisplaySettings(string _modelFile, bool _ignoreOverlays = false)
        {
            // Get model asset
            GameObject model = toolbox.assets.GetModel(_modelFile);

            // Check for model
            if (model == null)
            {
                Log.Error($"Failed to setup item display for '{_modelFile}', could not find model");
                return null;
            }

            // List of renderers
            List<Renderer> renderers =
            [
                // Add all mesh and skinned mesh renderers to list
                .. model.GetComponentsInChildren<MeshRenderer>(),
                .. model.GetComponentsInChildren<SkinnedMeshRenderer>()
            ];

            // Check for renderers
            if (renderers.Count <= 0)
            {
                Log.Error($"Failed to setup item display for '{_modelFile}', could not find any renderers");
                return null;
            }

            // Create render info array
            CharacterModel.RendererInfo[] renderInfos = new CharacterModel.RendererInfo[renderers.Count];

            // Cycle through renderers
            for (int i = 0; i < renderers.Count; i++)
            {
                // Set render info
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = renderers[i] is SkinnedMeshRenderer ? renderers[i].sharedMaterial : renderers[i].material,
                    renderer = renderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = _ignoreOverlays    // Should this model ignore effect overlays
                };
            }

            // Set item display renderer info
            ItemDisplay itemDisplay = model.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = renderInfos;

            // Return item display settings
            return new ItemDisplaySettings(this, model, new ItemDisplayRuleDict());
        }

        // Return Hurt Boxes from RoR2 Sphere Search
        public HurtBox[] GetHurtBoxesInSphere(Vector3 position, float radius)
        {
            if (radius <= 0)
            {
                return []; // Can't check 0 or negative radius
            }

            // Get hurt boxes in sphere search
            RoR2.HurtBox[] hurtBoxes = new SphereSearch
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = position
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            return hurtBoxes;   // Return found hurt boxes
        }

        public string GetCharacterModelName(string _character)
        {
            // Check for name
            if (characterModelNames.ContainsKey(_character))
            {
                // Return model name
                return characterModelNames[_character];
            }

            // Return null if not found
            return null;
        }

        public bool debugMode
        {
            get { return _debugMode; }
        }
    }

    internal struct CorruptPair
    {
        // Store corrupter and corrupted
        private ItemDef corrupter;
        private string corruptedToken;

        public CorruptPair(ItemDef _corrupter, string _corruptedToken)
        {
            // Assign corrupter and corrupted
            corrupter = _corrupter;
            corruptedToken = _corruptedToken;
        }

        public ItemDef.Pair GetPair()
        {
            // Copy corrupted token to local
            string localCorruptedToken = corruptedToken;

            // Get item to corrupt
            ItemDef corrupted = ItemCatalog.itemDefs.Where(x => x.nameToken == localCorruptedToken).FirstOrDefault();

            // Found item?
            if (!corrupted)
            {
                Log.Error($"Failed to add '{localCorruptedToken}' as corrupted by '{corrupter.nameToken}', unable to find '{localCorruptedToken}'");

                // Return empty item pair
                return new ItemDef.Pair();
            }

            // Return setup item pair
            return new ItemDef.Pair { itemDef1 = corrupted, itemDef2 = corrupter };
        }
    }

    internal class ItemDisplaySettings
    {
        // Utils reference
        private Utils utils;

        // Store model and display rules
        private GameObject model;
        private ItemDisplayRuleDict displayRules;

        public ItemDisplaySettings(Utils _utils, GameObject _model, ItemDisplayRuleDict _displayRules)
        {
            // Assign utils
            utils = _utils;

            // Assign model and display rules
            model = _model;
            displayRules = _displayRules;
        }

        public void AddCharacterDisplay(string _character, string _childName, Vector3 _position, Vector3 _angle, Vector3 _scale)
        {
            // Get character model name
            string modelName = utils.GetCharacterModelName(_character.ToLower());

            // Check for model name
            if (modelName == null)
            {
                Log.Error($"Couldn't assign item on character display data for character '{_character}', character not found");
                return;
            }

            // Add item display rule
            displayRules.Add(modelName,
            [
                new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    followerPrefab = model,
                    childName = _childName,
                    localPos = _position,
                    localAngles = _angle,
                    localScale = _scale
                }
            ]);
        }

        public ItemDisplayRuleDict GetRules()
        {
            // Return item display rules
            return displayRules;
        }
    }
}
