using BepInEx;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal static class Utils
    {
        // Plugin info
        public static PluginInfo pluginInfo;

        // Store debug mode
        static private bool _debugMode = false;

        // Store local player master and body
        static private CharacterMaster _localPlayer;
        static private CharacterBody _localPlayerBody;

        // Store character spawn cards
        static private List<CharacterSpawnCard> characterSpawnCards = new List<CharacterSpawnCard>();

        // Simulacrum banned items
        static List<ItemDef> simulacrumBanned = new List<ItemDef>();

        // Corruption item pairs
        static List<CorruptPair> corruptionPairs = new List<CorruptPair>();

        // Character model names
        static private Dictionary<string, string> characterModelNames = new Dictionary<string, string>()
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
            { "scavenger", "mdlScav" },
            { "seeker", "mdlSeeker" },
            { "false son", "mdlFalseSon" },
            { "chef", "mdlChef" }
        };

        // HG shader
        static private Shader HGShader;

        public static void Init(PluginInfo _pluginInfo)
        {
            // Provide plugin info
            pluginInfo = _pluginInfo;

            // Get HG shader
            HGShader = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

            // Config Simulacrum
            On.RoR2.InfiniteTowerRun.OverrideRuleChoices += InjectSimulacrumBannedItems;

            // Config item corruptions
            On.RoR2.Items.ContagiousItemManager.Init += SetupItemCorruptions;

            // Inject character spawn card awake behaviour
            On.RoR2.CharacterSpawnCard.Awake += OnCharacterSpawnCardAwake;

            // Update debug mode from config
            _debugMode = Config.CheckTag("DEBUG_MODE");

            Log.Debug("Utils initialised");
        }

        // Refresh chosen buff on chosen character
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration)
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

        public static void BanFromSimulacrum(ItemDef _item)
        {
            // Add item def to banned list for Simulacrum
            simulacrumBanned.Add(_item);
        }

        public static void AddCorruptionPair(ItemDef _corrupter, string _corruptedToken)
        {
            // Add item pair to corruption pairs
            corruptionPairs.Add(new CorruptPair(_corrupter, _corruptedToken));
        }

        public static void SpawnCharacterCard(Transform _target, string _name, int _amount = 1)
        {
            // Attempt to get character spawn card
            CharacterSpawnCard spawnCard = GetCharacterSpawnCard(_name);
            if (spawnCard == null)
            {
                // Send error
                Log.Error($"[UTILS] - Spawn character card failed! Could not find character card '{_name}'");
                return;
            }

            // Create director placement rule
            DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                spawnOnTarget = _target,
                preventOverhead = false
            };

            // Create director spawn request
            DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, directorPlacementRule, new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint));
            directorSpawnRequest.ignoreTeamMemberLimit = true;
            directorSpawnRequest.teamIndexOverride = TeamIndex.Monster;

            // COPIED SCRIPT //
            // ORIGINAL -> RoR2.CombatDirector.GenerateAmbush
            // COPIED SCRIPT //

            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(_target.position, spawnCard.hullSize, float.PositiveInfinity);
            NodeGraphSpider nodeGraphSpider = new NodeGraphSpider(groundNodes, HullMask.Human);
            nodeGraphSpider.AddNodeForNextStep(nodeIndex);
            List<NodeGraphSpider.StepInfo> list = new List<NodeGraphSpider.StepInfo>();
            int num = 0;
            List<NodeGraphSpider.StepInfo> collectedSteps = nodeGraphSpider.collectedSteps;
            while (nodeGraphSpider.PerformStep() && num < _amount && list.Count < _amount)  // ADJUSTED TO ENSURE CORRECT AMOUNT OF ENEMIES SPAWN
            {
                num++;
                for (int i = 0; i < collectedSteps.Count && list.Count < _amount; i++)
                {
                    list.Add(collectedSteps[i]);    // SKIP TEST FOR ACCEPTABLE AMBUSH
                }
                collectedSteps.Clear();
            }
            for (int j = 0; j < list.Count; j++)
            {
                Vector3 position;
                groundNodes.GetNodePosition(list[j].node, out position);
                spawnCard.DoSpawn(position, Quaternion.identity, directorSpawnRequest); // REPLACED LOAD RESOURCES WITH CACHED SPAWN CARD AND INJECT DIRECTOR SPAWN REQUEST
            }
        }

        static void InjectSimulacrumBannedItems(On.RoR2.InfiniteTowerRun.orig_OverrideRuleChoices orig, InfiniteTowerRun self, RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
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

        private static void SetupItemCorruptions(On.RoR2.Items.ContagiousItemManager.orig_Init orig)
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

        private static void OnCharacterSpawnCardAwake(On.RoR2.CharacterSpawnCard.orig_Awake orig, CharacterSpawnCard self)
        {
            orig(self); // Run normal processes

            // Check for card
            if (HasCharacterSpawnCard(self.prefab.name))
            {
                return;
            }

            // Add to character spawn cards
            characterSpawnCards.Add(self);

            // Debug only message
            if (debugMode)
            {
                Log.Debug($"[UTILS] - New character spawn card found for '{self.prefab.name}'");
            }
        }

        public static HoldoutZoneController ChargeHoldoutZone(HoldoutZoneController _zone)
        {
            // Instantly charge Holdout Zone
            _zone.baseChargeDuration = 0.0f;

            // return Holdout Zone
            return _zone;
        }

        public static HoldoutZoneController ChargeHoldoutZone(HoldoutZoneController _zone, float _amount, bool _percentage = true)
        {
            // Check if percentage based
            if (!_percentage)
            {
                _amount /= _zone.baseChargeDuration;
            }

            // Add charge to Holdout Zone
            _zone.charge += _amount;

            // Clamp zone charge
            _zone.charge = Mathf.Clamp01(_zone.charge);

            // return Holdout Zone
            return _zone;
        }

        public static void AddPulverizeBuildup(CharacterBody _target, CharacterBody _applier, float _duration)
        {
            // Check for target and applier
            if (_target == null || _applier == null)
            {
                return;
            }

            // Check for applier inventory
            Inventory applierInventory = _applier.inventory;
            if (applierInventory == null)
            {
                return;
            }

            // Get Shattering Justice item count
            int count = applierInventory.GetItemCount(RoR2Content.Items.ArmorReductionOnHit);

            // Test for item count and Pulverized buff
            if (count > 0 && !_target.HasBuff(RoR2Content.Buffs.Pulverized))
            {
                // Add Pulzerize Buildup
                _target.AddTimedBuff(RoR2Content.Buffs.PulverizeBuildup, _duration);

                // Check if enough Pulverize Buildup
                if (_target.GetBuffCount(RoR2Content.Buffs.PulverizeBuildup) >= 5)
                {
                    // Clear Pulverize Buildup
                    _target.ClearTimedBuffs(RoR2Content.Buffs.PulverizeBuildup);
                    _target.RemoveBuff(RoR2Content.Buffs.PulverizeBuildup);

                    // Add Pulverized
                    _target.AddTimedBuff(RoR2Content.Buffs.Pulverized, 8.0f * count);

                    // Spawn Pulzerized effect
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.pulverizedEffectPrefab, new EffectData
                    {
                        origin = _target.corePosition,
                        scale = _target.radius
                    }, true);
                }
            }
        }

        public static ItemDisplaySettings CreateItemDisplaySettings(string _modelFile, bool _useHopooShader = true, bool _ignoreOverlays = false, bool _dithering = true)
        {
            // In debug mode?
            if (debugMode)
            {
                Log.Debug($"Creating item display for '{_modelFile}'");
            }

            // Get model asset
            GameObject model = Assets.GetModel(_modelFile);

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
                // Get material
                Material material = renderers[i] is SkinnedMeshRenderer ? renderers[i].sharedMaterial : renderers[i].material;

                // Apply shader
                if (_useHopooShader && material)
                {
                    // Set shader
                    material.shader = HGShader;

                    // Should dither?
                    if (_dithering)
                    {
                        material.EnableKeyword("DITHER");
                    }
                    else
                    {
                        material.DisableKeyword("DITHER");
                    }
                }

                // Set render info
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = material,
                    renderer = renderers[i],
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = _ignoreOverlays    // Should this model ignore effect overlays
                };
            }

            // Set item display renderer info
            ItemDisplay itemDisplay = model.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = renderInfos;

            // Return item display settings
            return new ItemDisplaySettings(model, new ItemDisplayRuleDict());
        }

        // Attempt to fetch local player master
        private static void FetchLocalPlayer()
        {
            // Check for local user
            LocalUser local = LocalUserManager.GetFirstLocalUser();
            if (local != null && local.cachedMaster != null)
            {
                // Assign local player master
                _localPlayer = local.cachedMaster;
            }
        }

        public static bool HasCharacterSpawnCard(string _name)
        {
            // Check for character spawn card
            return GetCharacterSpawnCard(_name) != null;
        }

        public static CharacterSpawnCard GetCharacterSpawnCard(string _name)
        {
            // Cycle through character spawn cards
            foreach (CharacterSpawnCard current in characterSpawnCards)
            {
                // Check name
                if (current.prefab.name == _name)
                {
                    // Found
                    return current;
                }
            }

            // Not found
            return null;
        }

        // Get all Holdout Zones this character is in
        public static List<HoldoutZoneController> GetHoldoutZonesContainingCharacter(CharacterMaster _character)
        {
            // Check for character body
            if (!_character.hasBody)
            {
                return [];
            }

            // Get character position
            Vector3 charPos = _character.GetBody().transform.position;

            // Initialise list of zones containing character
            List<HoldoutZoneController> containing = new List<HoldoutZoneController>();

            // Get all Holdout Zones
            HoldoutZoneController[] zones = UnityEngine.Object.FindObjectsOfType<HoldoutZoneController>();

            // Cycle through zones
            foreach (HoldoutZoneController zone in zones)
            {
                // Calculate distance
                float distance = (zone.gameObject.transform.position - charPos).magnitude;

                // Check if character is within zone
                if (distance <= zone.currentRadius)
                {
                    // Add to containing list
                    containing.Add(zone);
                }
            }

            // Return list of zones containing character
            return containing;
        }

        // Return Hurt Boxes from RoR2 Sphere Search
        public static HurtBox[] GetHurtBoxesInSphere(Vector3 position, float radius)
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

        public static List<PlayerCharacterMasterController> GetPlayers()
        {
            // Return list of all characters
            return new List<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
        }

        public static List<CharacterMaster> GetCharacters()
        {
            // Return list of all characters
            return new List<CharacterMaster>(CharacterMaster.readOnlyInstancesList);
        }

        public static List<CharacterMaster> GetCharactersForTeam(TeamIndex _teamIndex, bool _requiresBody = true)
        {
            // Initialise list
            List<CharacterMaster> characters = new List<CharacterMaster>();

            // Cycle through characters
            foreach (CharacterMaster character in CharacterMaster.readOnlyInstancesList)
            {
                // Check if valid
                if (character.teamIndex == _teamIndex && (!_requiresBody || character.hasBody))
                {
                    // Add to list
                    characters.Add(character);
                }
            }

            // Return list
            return characters;
        }

        public static int GetItemCountForTeam(TeamIndex _teamIndex, ItemDef _itemDef, bool _requiresAlive = true, bool _requiresConnected = true)
        {
            // Found item count
            int count = 0;

            // Cycle through characters
            foreach (CharacterMaster character in CharacterMaster.readOnlyInstancesList)
            {
                // Check if valid
                if (character.teamIndex == _teamIndex && (!_requiresAlive || character.hasBody) && (!_requiresConnected || !character.playerCharacterMasterController || character.playerCharacterMasterController.isConnected))
                {
                    // Check for inventory
                    if (!character.inventory)
                    {
                        continue;
                    }

                    // Increment count
                    count += character.inventory.GetItemCount(_itemDef);
                }
            }

            // Return total items
            return count;
        }

        public static CharacterMaster GetLastAttacker(CharacterBody _victim)
        {
            // Check for victim
            if (_victim == null)
            {
                return null;
            }

            // Try to find Faithful Behaviour
            FaithfulHealthComponentBehaviour helper = _victim.gameObject.GetComponent<FaithfulHealthComponentBehaviour>();
            if (helper != null)
            {
                // Return last attacker
                return helper.lastAttacker;
            }

            // Otherwise return null
            return null;
        }

        public static CharacterBody GetInventoryBody(Inventory _inventory)
        {
            // Attempt to get Character Master
            CharacterMaster master = _inventory.gameObject.GetComponent<CharacterMaster>();
            if (master == null)
            {
                return null;
            }

            // Check for Character Body
            if (!master.hasBody)
            {
                return null;
            }

            // Return Character Body
            return master.GetBody();
        }

        public static string GetCharacterModelName(string _character)
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

        public static bool debugMode
        {
            get { return _debugMode; }
        }

        public static bool hosting
        {
            get { return NetworkServer.active; }
        }

        public static CharacterMaster localPlayer
        {
            get
            {
                // Check for local player master
                if (_localPlayer == null)
                {
                    // Attempt to fetch local player master
                    FetchLocalPlayer();
                }

                // Return local player master
                return _localPlayer;
            }
        }

        public static CharacterBody localPlayerBody
        {
            get
            {
                // Check for local player body
                if (_localPlayerBody == null)
                {
                    // Check for local player master
                    CharacterMaster localMaster = localPlayer;
                    if (localMaster == null)
                    {
                        // Cannot find local player master
                        return null;
                    }
                    else
                    {
                        // Update local player body
                        _localPlayerBody = localMaster.GetBody();
                    }
                }

                // Return local player body
                return _localPlayerBody;
            }
        }

        public static List<string> characterCardNames
        {
            get
            {
                // Create string list
                List<string> names = new List<string>();

                // Cycle through character spawn cards
                foreach (CharacterSpawnCard current in characterSpawnCards)
                {
                    // Add name to list
                    names.Add(current.prefab.name);
                }

                // Sort list
                names.Sort();

                // Return list
                return names;
            }
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
            ItemDef corrupted = ItemCatalog.allItemDefs.Where(x => x.nameToken == localCorruptedToken).FirstOrDefault();

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
        // Store model and display rules
        private GameObject model;
        private ItemDisplayRuleDict displayRules;

        // List of models with display rules already assigned
        List<string> assignedModels = new List<string>();

        public ItemDisplaySettings(GameObject _model, ItemDisplayRuleDict _displayRules)
        {
            // Assign model and display rules
            model = _model;
            displayRules = _displayRules;
        }

        public void AddCharacterDisplay(string _character, string _childName, Vector3 _position, Vector3 _angle, Vector3 _scale)
        {
            // Get character model name
            string modelName = Utils.GetCharacterModelName(_character.ToLower());

            // Check for model name
            if (modelName == null)
            {
                Log.Error($"Couldn't assign item on character display data for character '{_character}', character not found");
                return;
            }

            // Test if character already has display rules assigned
            if (assignedModels.Contains(modelName))
            {
                // Get current list of display rules
                List<ItemDisplayRule> current = new List<ItemDisplayRule>(displayRules[modelName])
                {
                    // Add to list
                    new ItemDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.ParentedPrefab,
                        followerPrefab = model,
                        childName = _childName,
                        localPos = _position,
                        localAngles = _angle,
                        localScale = _scale
                    }
                };

                // Update display rules
                displayRules[modelName] = current.ToArray();

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

            // Add to assigned models
            assignedModels.Add(modelName);
        }

        public ItemDisplayRuleDict GetRules()
        {
            // Return item display rules
            return displayRules;
        }
    }
}
