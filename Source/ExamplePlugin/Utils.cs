using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
using RoR2.Navigation;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using Faithful.Shared;

namespace Faithful
{
    internal static class Utils
    {
        // Plugin info
        public static PluginInfo pluginInfo;

        // Store expansion definition
        public static ExpansionDef expansionDef;

        // Store expansion run behaviour prefab
        public static GameObject runBehaviourPrefab;

        // Store if expansion is enabled
        public static bool expansionEnabled = true;

        /*// Detected assemblies
        static private Assembly lookingGlassAssembly = null;

        // Optional tracked components from other assemblies
        static private ConfigEntry<bool> lookingGlassFullItemDescs = null;*/

        // Store debug mode
        static private bool _debugMode = false;

        // Store verbose console
        static private bool _verboseConsole = false;

        // Store randomiser mode
        static private bool _randomiserMode = false;

        // Store local player master and body
        static private CharacterMaster _localPlayer;
        static private CharacterBody _localPlayerBody;

        // Store net utils
        static private NetUtils _netUtils;

        // Store character spawn cards
        static private List<CharacterSpawnCard> characterSpawnCards = new List<CharacterSpawnCard>();

        // Simulacrum banned items
        static List<ItemDef> simulacrumBanned = new List<ItemDef>();

        // Corruption item pairs
        static List<CorruptPair> corruptionPairs = new List<CorruptPair>();

        // Create debug mode config
        static private Setting<bool> debugModeSetting;

        // Create verbose console config
        static private Setting<bool> verboseConsoleSetting;

        // Create randomiser mode config
        static private Setting<bool> randomiserModeSetting;

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
            { "chef", "mdlChef" },
            { "operator", "mdlDroneTech" },
            { "drifter", "mdlDrifter" },
            { "best buddy", "mdlDefectiveUnit (1)" },
            { "technician", "mdlTechnician" }
        };

        // Common irregular words dictionary - used for pluralization
        static private Dictionary<string, string> irregularPlurals = new Dictionary<string, string>()
        {
            { "Child", "Children" },
            { "Person", "People" },
            { "Mouse", "Mice" },
            { "Goose", "Geese" },
            { "Tooth", "Teeth" },
            { "Foot", "Feet" },
            { "Ox", "Oxen" },
            { "Fish", "Fish" },
            { "Sheep", "Sheep" },
            { "Deer", "Deer" },
            { "Potato", "Potatoes" },
            { "Tomato", "Tomatoes" },
            { "Hero", "Heroes" },
            { "Echo", "Echoes" },
            { "Torpedo", "Torpedoes" },
            { "Embargo", "Embargoes" },
            { "Mosquito", "Mosquitoes" },
            { "Buffalo", "Buffaloes" },
            { "Volcano", "Volcanoes" }
        };

        // Store list of character bodies and corresponding faithful character behaviours
        static private Dictionary<CharacterBody, FaithfulCharacterBodyBehaviour> characterBodyLookup = new Dictionary<CharacterBody, FaithfulCharacterBodyBehaviour>();

        // HG shader
        static private Shader HGShader;

        // Cached list of shaders converted to Hopoo Games shader
        private static List<Material> HGCachedMaterials = new List<Material>();

        // Store language dictionary for early lookups
        static private Dictionary<string, string> languageDictionary;

        // Store list of item behaviours
        static private List<ItemBase> itemBehaviours = new List<ItemBase>();

        // Store list of character behaviours
        static private List<System.WeakReference<ICharacterBehaviour>> characterBehaviours = new List<System.WeakReference<ICharacterBehaviour>>();

        // Cache for TMP fonts to avoid redundant searches
        static private Dictionary<string, TMP_FontAsset> fontCache = new Dictionary<string, TMP_FontAsset>();

        public static void Init(PluginInfo _pluginInfo)
        {
            // Detect assemblies
            DetectAssemblies();

            // Create debug mode setting
            debugModeSetting = Config.CreateSetting("DEBUG_MODE", "Debug Tools", "Debug Mode", false, "Do you want to enable this mod's debug mode?", false, true, _restartRequired: true);

            // Create debug mode setting
            verboseConsoleSetting = Config.CreateSetting("VERBOSE_CONSOLE", "Debug Tools", "Verbose Console", false, "Do you want more detailed console logging for this mod?", false, true, _restartRequired: true);

            // Create randomiser mode setting
            randomiserModeSetting = Config.CreateSetting("RANDOMISER_MODE", "Extras", "Randomizer Mode", false, "Do you want to randomize the stats of items introduced by the Faithful mod?\n[WARNING] - This setting is likely to dramatically alter the balance of items introduced by the Faithful mod.", false, true);

            // Update debug mode from config
            _debugMode = debugModeSetting.Value;

            // Update verbose console from config
            _verboseConsole = verboseConsoleSetting.Value;

            // Update randomiser mode from config
            _randomiserMode = randomiserModeSetting.Value;

            // Provide plugin info
            pluginInfo = _pluginInfo;

            // Get HG shader
            HGShader = LegacyResourcesAPI.Load<Shader>("Shaders/Deferred/HGStandard");

            // Config Simulacrum
            On.RoR2.InfiniteTowerRun.OverrideRuleChoices += InjectSimulacrumBannedItems;

            // Config item corruptions
            On.RoR2.Items.ContagiousItemManager.Init += SetupItemCorruptions;

            // Check if debug mode is enabled
            if (debugMode)
            {
                // Inject character spawn card awake and spawn behaviour
                On.RoR2.CharacterSpawnCard.Awake += OnCharacterSpawnCardAwake;
                On.RoR2.CharacterSpawnCard.Spawn += OnCharacterSpawnCardSpawn;
            }

            // Add pre-game controller set rule book behaviour
            PreGameController.onPreGameControllerSetRuleBookGlobal += OnPreGameControllerSetRuleBookGlobal;

            // Add behaviour to main menu on enter
            On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += OnMainMenuEnter;

            // Add behaviour to loadout panel
            On.RoR2.UI.LoadoutPanelController.OnEnable += OnLoadoutPanelEnable;

            // Load language file
            LoadLanguageFile();

            if (_verboseConsole) Log.Debug("Utils initialised");
        }

        public static void Start()
        {
            // Load fonts
            LoadFonts();
        }

        private static void DetectAssemblies()
        {
            /*// Cycle through loaded assemblies
            foreach (Assembly currentAssembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                // Check if LookingGlass is installed
                if (currentAssembly.GetName().Name == "LookingGlass")
                {
                    // Assign LookingGlass assembly
                    lookingGlassAssembly = currentAssembly;
                }
            }*/
        }

        private static void LoadFonts()
        {
            // Cycle through fonts
            foreach (TMP_FontAsset tmpFont in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                // Check if font is already cached
                if (fontCache.ContainsKey(tmpFont.name)) continue;

                // Store fonts in dictionary for quick lookup
                fontCache[tmpFont.name] = tmpFont;

                // Log font if in verbose mode
                if (verboseConsole) Log.Debug($"[UTILS] | Cached font '{tmpFont.name}'.");
            }
        }

        private static void LoadLanguageFile()
        {
            // Get language file path
            string languageFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pluginInfo.Location), "Faithful.language");

            // Check for the language file
            if (System.IO.File.Exists(languageFilePath))
            {
                // Read the language file
                string json = System.IO.File.ReadAllText(languageFilePath);

                // Deserialize the JSON using the wrapper into the language dictionary
                languageDictionary = JsonConvert.DeserializeObject<LanguageWrapper>(json).strings;
            }

            // Language file doesn't exist
            else
            {
                Log.Error("[UTILS]- Language file not found at: " + languageFilePath);
            }
        }

        public static void CreateExpansionDef()
        {
            // Get DLC 1 to steal it's stuff
            ExpansionDef dlc1 = LegacyResourcesAPI.Load<ExpansionDef>("ExpansionDefs/DLC1");

            // Get template for expansion run behaviour prefab
            GameObject runBehaviourTemplate = Addressables.LoadAssetAsync<GameObject>("12bf89dabb4bb914382a0e31546446cc").WaitForCompletion();

            // Create expansion run behaviour prefab
            runBehaviourPrefab = PrefabAPI.InstantiateClone(runBehaviourTemplate, "FaithfulExpansionRunBehaviour", true);

            // Destroy global death rewards
            Object.DestroyImmediate((Object)(object)runBehaviourPrefab.GetComponent<GlobalDeathRewards>());

            // Get expansion requirement component
            ExpansionRequirementComponent expansionRequirementComponent = runBehaviourPrefab.GetComponent<ExpansionRequirementComponent>();

            // Create expansion def
            expansionDef = ScriptableObject.CreateInstance<ExpansionDef>();

            // Assign required expansion to expansion run behaviour
            expansionRequirementComponent.requiredExpansion = expansionDef;

            // Register expansion run behaviour
            PrefabAPI.RegisterNetworkPrefab(runBehaviourPrefab);

            // Add language tokens
            expansionDef.name = "FAITHFUL_EXPANSION_NAME";
            expansionDef.nameToken = "FAITHFUL_EXPANSION_NAME";
            expansionDef.descriptionToken = "FAITHFUL_EXPANSION_DESC";

            // Set expansion def icon
            expansionDef.iconSprite = Assets.GetIcon("texCustomExpansionIcon");

            // Assign disabled icon and required entitlement
            expansionDef.disabledIconSprite = dlc1.disabledIconSprite;
            expansionDef.requiredEntitlement = null;

            // Assign run behaviour prefab
            expansionDef.runBehaviorPrefab = runBehaviourPrefab;

            // Register new expansion def with R2API
            ContentAddition.AddExpansionDef(expansionDef);
        }

        // Refresh chosen buff on chosen character
        public static void RefreshTimedBuffs(CharacterBody body, BuffDef buffDef, float duration, int amount = int.MaxValue)
        {
            if (!body || body.GetBuffCount(buffDef) <= 0)
            {
                return; // Body not valid
            }

            // Count how many buffs have been refreshed
            int counter = 0;

            // Cycle through buffs
            foreach (CharacterBody.TimedBuff buff in body.timedBuffs)
            {
                // Check if reached limit to refresh
                if (counter >= amount) return;

                // Check if correct buff
                if (buffDef.buffIndex == buff.buffIndex)
                {
                    // Refresh buff
                    buff.timer = duration;

                    // Increment counter
                    counter++;
                }
            }
        }

        public static void RefreshItemSettings()
        {
            // Fetch settings for items
            Items.FetchSettings();

            // Cycle through item behaviours
            foreach (ItemBase itemBehaviour in itemBehaviours)
            {
                // Tell item behaviour to fetch it's settings again
                itemBehaviour.FetchSettings();
            }

            // Cycle through character behaviours
            foreach (System.WeakReference<ICharacterBehaviour> weakCharacterBehaviour in characterBehaviours)
            {
                // Check if the behaviour is still alive and access it
                if (weakCharacterBehaviour.TryGetTarget(out ICharacterBehaviour characterBehaviour))
                {
                    // Check for character behaviour
                    if (characterBehaviour == null) continue;

                    // Fetch settings for character behaviour
                    characterBehaviour.FetchSettings();
                }
            }
        }

        public static void BanFromSimulacrum(ItemDef _item)
        {
            // Add item def to banned list for Simulacrum
            simulacrumBanned.Add(_item);
        }

        public static void RegisterItemBehaviour(ItemBase _itemBehaviour)
        {
            // Add to list
            itemBehaviours.Add(_itemBehaviour);
        }

        public static void RegisterCharacterBehaviour(ICharacterBehaviour _characterBehaviour)
        {
            // Add to list
            characterBehaviours.Add(new System.WeakReference<ICharacterBehaviour>(_characterBehaviour));
        }

        public static void UnregisterCharacterBehaviour(ICharacterBehaviour _characterBehaviour)
        {
            // Remove all matching references
            characterBehaviours.RemoveAll(weakReference =>
            {
                // Try get reference to behaviour
                if (weakReference.TryGetTarget(out ICharacterBehaviour behaviour))
                {
                    // Remove if behaviour is null or the reference matches
                    return behaviour == null || ReferenceEquals(behaviour, _characterBehaviour);
                }

                // Don't remove
                return false;
            });
        }

        public static void RegisterFaithfulCharacterBodyBehaviour(CharacterBody _characterBody, FaithfulCharacterBodyBehaviour _faithfulBehaviour)
        {
            // Add to dictionary
            characterBodyLookup[_characterBody] = _faithfulBehaviour;
        }

        public static void AddCorruptionPair(ItemDef _corrupter, string _corruptedToken, string _corruptedOverride = "")
        {
            // Add item pair to corruption pairs
            corruptionPairs.Add(new CorruptPair(_corrupter, _corruptedToken, _corruptedOverride));
        }

        public static void TeleportToNextStage()
        {
            // Teleport out of current scene
            Stage.instance.BeginAdvanceStage(Run.instance.nextStageScene);
        }

        public static void KillAllEnemies()
        {
            // Get all character bodies
            CharacterBody[] characterBodies = Object.FindObjectsOfType<CharacterBody>();

            // Cycle through character bodies
            foreach (CharacterBody characterBody in characterBodies)
            {
                // Check if not player team
                if (characterBody.teamComponent.teamIndex != TeamIndex.Player)
                {
                    // Attempt to kill
                    try
                    {
                        // Kill character
                        characterBody.healthComponent.gameObject.GetComponent<CharacterDeathBehavior>().OnDeath();
                    }
                    catch
                    {
                        // Ignore and continue
                        continue;
                    }
                }
            }
        }

        public static void SpawnCharacterCard(Transform _target, string _name, int _amount = 1, TeamIndex _team = TeamIndex.Monster, System.Action<SpawnCard.SpawnResult> _onSpawn = null)
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
            directorSpawnRequest.teamIndexOverride = _team;

            // Check if on spawn callback is provided
            if (_onSpawn != null)
            {
                directorSpawnRequest.onSpawnedServer = _onSpawn;
            }
            

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

        public static void SpawnPickup(PickupDef _pickup, Vector3 _position, Vector3? _velocity = null, int _amount = 1)
        {
            // Check for velocity
            Vector3 velocity = _velocity ?? Vector3.zero;

            // Get pickup index
            PickupIndex index = _pickup.pickupIndex;

            // Cycle for amount
            for (int i = 0; i < _amount; i++)
            {
                // Spawn pickup
                PickupDropletController.CreatePickupDroplet(index, _position, velocity);
            }
        }

        public static void MakeElite(CharacterBody _character, Inventory _inventory, EliteDef _eliteDef)
        {
            // Make elite
            _inventory.SetEquipmentIndex(_eliteDef.eliteEquipmentDef.equipmentIndex);

            // Modify stats
            _character.baseDamage *= _eliteDef.damageBoostCoefficient;
            _character.baseMaxHealth *= _eliteDef.healthBoostCoefficient;
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

        static void RegisterCharacterSpawnCard(CharacterSpawnCard _spawnCard)
        {
            // Encounters issues with modded spawn cards
            try
            {
                // Check for card
                if (HasCharacterSpawnCard(_spawnCard.prefab.name))
                {
                    return;
                }

                // Add to character spawn cards
                characterSpawnCards.Add(_spawnCard);

                // Verbose only message
                if (verboseConsole)
                {
                    Log.Debug($"[UTILS] - New character spawn card found for '{_spawnCard.prefab.name}'");
                }
            }
            catch
            {
                // Verbose only message
                if (verboseConsole)
                {
                    Log.Warning($"[UTILS] - Could not add character spawn card to spawn list.");
                }
            }
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

            if (_verboseConsole) Log.Debug("Added item corruptions");

            orig(); // Run normal processes
        }

        private static void OnCharacterSpawnCardAwake(On.RoR2.CharacterSpawnCard.orig_Awake orig, CharacterSpawnCard self)
        {
            orig(self); // Run normal processes

            // Register spawn card
            RegisterCharacterSpawnCard(self);
        }

        private static void OnCharacterSpawnCardSpawn(On.RoR2.CharacterSpawnCard.orig_Spawn orig, CharacterSpawnCard self, Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnCard.SpawnResult result)
        {
            orig(self, position, rotation, directorSpawnRequest, ref result); // Run normal processes

            // Register spawn card
            RegisterCharacterSpawnCard(self);
        }

        private static void OnPreGameControllerSetRuleBookGlobal(PreGameController preGameController, RuleBook ruleBook)
        {
            // Check if expansion is enabled
            if (ruleBook.IsChoiceActive(expansionDef.enabledChoice))
            {
                // Update if expansion is enabled
                expansionEnabled = true;

                // Enable behaviour
                Behaviour.Enable();
            }

            // Expansion disabled
            else
            {
                // Update if expansion is enabled
                expansionEnabled = false;

                // Disable behaviour
                Behaviour.Disable();
            }
        }

        private static void OnMainMenuEnter(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_OnEnter orig, RoR2.UI.MainMenu.BaseMainMenuScreen self, RoR2.UI.MainMenu.MainMenuController mainMenuController)
        {
            // Run original processes
            orig(self, mainMenuController);

            // Update randomiser mode from config
            _randomiserMode = randomiserModeSetting.Value;

            // Check if randomiser mode is enabled
            if (randomiserMode)
            {
                // Randomise item stats and refresh items
                RandomiseAndRefresh();
            }

            // Not randomiser mode
            else
            {
                // Refresh item settings
                RefreshItemSettings();
            }
        }

        private static void OnLoadoutPanelEnable(On.RoR2.UI.LoadoutPanelController.orig_OnEnable orig, RoR2.UI.LoadoutPanelController self)
        {
            // Run original processes
            orig(self);

            // Randomise item stats and refresh items
            RandomiseAndRefresh();

            // Reset lookups
            LookupTable.Reset();
        }

        public static void ApplyFonts(Transform parent)
        {
            // Cycle through children
            foreach (Transform child in parent)
            {
                // Get font name
                string fontName = ExtractFontTag(child.gameObject.name);

                // Get font
                TMP_FontAsset font = GetFont(fontName);

                // Check for valid font name
                if (!string.IsNullOrEmpty(fontName) && font != null)
                {
                    // Apply font to TextMeshProUGUI
                    TextMeshProUGUI tmpUGUI = child.GetComponent<TextMeshProUGUI>();
                    if (tmpUGUI != null)
                    {
                        // Apply font
                        tmpUGUI.font = font;
                    }

                    // Apply font to TextMeshPro
                    TextMeshPro tmp = child.GetComponent<TextMeshPro>();
                    if (tmp != null)
                    {
                        // Apply font
                        tmp.font = font;
                    }
                }

                // Recursively check children
                ApplyFonts(child);
            }
        }

        private static string ExtractFontTag(string objectName)
        {
            // Match anything inside square brackets at the end
            Match match = Regex.Match(objectName, @"\[(.*?)\]$");

            // Return match
            return match.Success ? match.Groups[1].Value : null;
        }

        public static void RandomiseAndRefresh()
        {
            // Check if randomiser mode is enabled
            if (randomiserMode)
            {
                // Re-randomise settings
                Config.ResetSettingRandomisers();

                // Refresh item settings
                RefreshItemSettings();

                // Check if verbose mode is enabled
                if (verboseConsole)
                {
                    Log.Debug($"[UTILS] - Randomised item stats.");
                }
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

        public static ItemDisplaySettings CreateItemDisplaySettings(string _modelFile, bool _ignoreOverlays = false)
        {
            // In verbose mode?
            if (verboseConsole)
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

            // Process renderer rules for model
            ProcessRendererRules(model);

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

                // Set render info
                renderInfos[i] = new CharacterModel.RendererInfo
                {
                    defaultMaterial = material,
                    renderer = renderers[i],
                    defaultShadowCastingMode = ShadowCastingMode.On,
                    ignoreOverlays = _ignoreOverlays    // Should this model ignore effect overlays
                };
            }

            // Set item display renderer info
            ItemDisplay itemDisplay = model.AddComponent<ItemDisplay>();
            itemDisplay.rendererInfos = renderInfos;

            // Return item display settings
            return new ItemDisplaySettings(model, new ItemDisplayRuleDict());
        }

        public static void ProcessRendererRules(GameObject _model)
        {
            // Check if model is valid
            if (_model == null)
            {
                Log.Error($"Failed to process pickup prefab, prefab is null!");
                return;
            }

            // In verbose mode?
            if (verboseConsole)
            {
                Log.Debug($"Processing pickup prefab for '{_model.name}'");
            }

            // Grab only mesh/skinned renderers
            Renderer[] renderers = _model.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (r is not MeshRenderer && r is not SkinnedMeshRenderer) continue;

                // Rules are attached to the same GO as the renderer
                RendererRules rules = r.GetComponent<RendererRules>();
                if (!rules) continue;

                // Using r.materials gives per-renderer instances
                Material[] mats = r.materials;
                bool changedAnyMaterial = false;

                for (int mi = 0; mi < mats.Length; mi++)
                {
                    Material mat = mats[mi];
                    if (!mat) continue;

                    // Try and fetch standard shader properties and keywords
                    bool normalMap = mat.IsKeywordEnabled("_NORMALMAP");
                    bool emission = mat.IsKeywordEnabled("_EMISSION");
                    bool noCull = mat.IsKeywordEnabled("NOCULL");
                    bool limbRemoval = mat.IsKeywordEnabled("LIMBREMOVAL");
                    float? bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : null;
                    Color? emissionColour = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : null;
                    Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                    Texture emissionMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;

                    // Apply modifier
                    switch (rules.modifier)
                    {
                        case RendererModifier.None:
                            break;

                        case RendererModifier.HopooShader:
                            mat.shader = HGShader;

                            // Check if should salvage existing shader properties
                            if (rules.salvageExistingProperties)
                            {
                                // Map salvaged properties to new shader
                                if (normalMap && bumpScale != null)
                                {
                                    mat.SetFloat("_NormalStrength", (float)bumpScale);
                                    mat.SetTexture("_NormalTex", bumpMap);
                                }
                                if (emission && emissionColour != null)
                                {
                                    mat.SetColor("_EmColor", (Color)emissionColour);
                                    mat.SetFloat("_EmPower", 1);
                                    mat.SetTexture("_EmTex", emissionMap);
                                }
                                if (noCull)
                                {
                                    mat.SetInt("_Cull", 0);
                                }
                                if (limbRemoval)
                                {
                                    mat.SetInt("_LimbRemovalOn", 1);
                                }
                            }

                            break;

                        case RendererModifier.InfusionGlass:
                            mat = Object.Instantiate(Assets.infusionGlassMaterial);
                            mats[mi] = mat;
                            changedAnyMaterial = true;
                            break;

                        default:
                            break;
                    }

                    // Apply texture rules
                    if (rules.textureRules != null)
                    {
                        for (int ti = 0; ti < rules.textureRules.Count; ti++)
                        {
                            ShaderTextureRule tr = rules.textureRules[ti];
                            if (string.IsNullOrWhiteSpace(tr.name) || tr.texture == null) continue;

                            mat.SetTexture(tr.name, tr.texture);
                        }
                    }

                    // Apply colour rules
                    if (rules.colourRules != null)
                    {
                        for (int ci = 0; ci < rules.colourRules.Count; ci++)
                        {
                            ShaderColourRule cr = rules.colourRules[ci];
                            if (string.IsNullOrWhiteSpace(cr.name)) continue;

                            mat.SetColor(cr.name, cr.colour);
                        }
                    }

                    // Apply vector rules
                    if (rules.vectorRules != null)
                    {
                        for (int vi = 0; vi < rules.vectorRules.Count; vi++)
                        {
                            ShaderVectorRule vr = rules.vectorRules[vi];
                            if (string.IsNullOrWhiteSpace(vr.name)) continue;

                            mat.SetVector(vr.name, vr.value);
                        }
                    }

                    // Apply keyword rules
                    if (rules.keywordRules != null)
                    {
                        for (int ki = 0; ki < rules.keywordRules.Count; ki++)
                        {
                            ShaderKeywordRule kr = rules.keywordRules[ki];
                            if (string.IsNullOrWhiteSpace(kr.keyword)) continue;

                            if (kr.enabled) mat.EnableKeyword(kr.keyword);
                            else mat.DisableKeyword(kr.keyword);
                        }
                    }

                    // Apply float rules
                    if (rules.floatRules != null)
                    {
                        for (int fi = 0; fi < rules.floatRules.Count; fi++)
                        {
                            ShaderFloatRule fr = rules.floatRules[fi];
                            if (string.IsNullOrWhiteSpace(fr.name)) continue;

                            mat.SetFloat(fr.name, fr.value);
                        }
                    }

                    // Apply int rules
                    if (rules.intRules != null)
                    {
                        for (int ii = 0; ii < rules.intRules.Count; ii++)
                        {
                            ShaderIntRule ir = rules.intRules[ii];
                            if (string.IsNullOrWhiteSpace(ir.name)) continue;

                            mat.SetInt(ir.name, ir.value);
                        }
                    }
                }

                // Assign back materials list if any were changed
                if (changedAnyMaterial) r.materials = mats;

                // Remove the rules component after applying (keeps runtime hierarchy clean)
                Object.Destroy(rules);
            }
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

        public static GameObject FindChildByName(Transform _parent, string _childName)
        {
            // Cycle through children
            foreach (Transform child in _parent)
            {
                // Check child name
                if (child.name == _childName)
                {
                    // Correct child found
                    return child.gameObject;
                }

                // Recursively search in the child's children
                GameObject foundChild = FindChildByName(child, _childName);

                // Check that is not null
                if (foundChild != null)
                {
                    // Return correct child
                    return foundChild;
                }
            }

            return null; // Return null if not found
        }

        public static FaithfulCharacterBodyBehaviour FindCharacterBodyHelper(CharacterBody _characterBody)
        {
            // Check for character body in character body lookup
            if (characterBodyLookup.ContainsKey(_characterBody))
            {
                // Return corresponding faithful behaviour
                return characterBodyLookup[_characterBody];
            }

            // Check if verbose mode
            if (verboseConsole)
            {
                // Warn and return null
                Log.Warning($"[UTILS] - Could not find faithful character body behaviour for character '{_characterBody.name}' with net ID {_characterBody.GetComponent<NetworkIdentity>().netId}.");
            }

            // Return null
            return null;
        }

        public static bool HasCharacterSpawnCard(string _name)
        {
            // Check for character spawn card
            return GetCharacterSpawnCard(_name) != null;
        }

        public static void ConfigureScarfDynamicBone(DynamicBone _dynamicBone)
        {
            // Set up dynamic bone
            _dynamicBone.m_Root = _dynamicBone.transform;
            _dynamicBone.m_UpdateRate = 60.0f;
            _dynamicBone.m_UpdateMode = DynamicBone.UpdateMode.Normal;
            _dynamicBone.m_Damping = 0.576f;
            _dynamicBone.m_Elasticity = 0.016f;
            _dynamicBone.m_Stiffness = 0.0f;
            _dynamicBone.m_Inert = 0.0f;
            _dynamicBone.m_Radius = 0.0f;
            _dynamicBone.m_EndLength = 0.69f;
            _dynamicBone.m_EndOffset = Vector3.zero;
            _dynamicBone.m_Gravity = new Vector3(0.0f, -0.01f, 0.0f);
            _dynamicBone.m_LocalGravity = new Vector3(0.0f, 0.01f, -0.01f);
            _dynamicBone.m_Force = Vector3.zero;
            _dynamicBone.m_Colliders = new List<DynamicBoneCollider>();
            _dynamicBone.m_Exclusions = new List<Transform>();
            _dynamicBone.m_FreezeAxis = DynamicBone.FreezeAxis.None;
            _dynamicBone.m_DistantDisable = false;
            _dynamicBone.m_ReferenceObject = null;
            _dynamicBone.m_DistanceToObject = 20.0f;
            _dynamicBone.neverOptimize = false;

            // Check for scarf dynamic bone
            DynamicBone scarfDynamicBone = Assets.scarfDynamicBone;
            if (scarfDynamicBone == null)
            {
                // Warn and return
                Log.Warning($"[UTILS] | Could not find scarf dynamic bone in assets. Unable to set up animation curves for scarf '{_dynamicBone.gameObject.name}'!");
                return;
            }

            // Set up animation curves
            _dynamicBone.m_DampingDistrib = scarfDynamicBone.m_DampingDistrib;
            _dynamicBone.m_ElasticityDistrib = scarfDynamicBone.m_ElasticityDistrib;
            _dynamicBone.m_StiffnessDistrib = scarfDynamicBone.m_StiffnessDistrib;
            _dynamicBone.m_InertDistrib = scarfDynamicBone.m_InertDistrib;
            _dynamicBone.m_RadiusDistrib = scarfDynamicBone.m_RadiusDistrib;
        }

        public static void LogComponents(GameObject _gameObject)
        {
            // Check if null
            if (_gameObject == null) return;

            // Create message string
            string message = $"The GameObject '{_gameObject.name}' has the following components:";

            // Cycle through each component
            foreach (Component component in _gameObject.GetComponents<Component>())
            {
                // Add to message string
                message += $"\n - {component.GetType()}";
            }

            // Log
            Debug.Log($"[UTILS] - {message}");
        }

        // Mostly yoinked this function from the Henry mod
        public static Material ConvertDefaultShaderToHopoo(this Material tempMat)
        {
            // Check if material has already been converted
            if (HGCachedMaterials.Contains(tempMat))
            {
                // Done
                return tempMat;
            }

            // Check if possible to convert
            string name = tempMat.shader.name.ToLowerInvariant();
            if (!name.StartsWith("standard") && !name.StartsWith("autodesk"))
            {
                // Log warning if in verbose mode
                if (verboseConsole)
                {
                    Log.Warning($"[UTILS] | '{tempMat.name}' is not unity standard shader - Cannot convert to a HG shader.");
                }
                
                // Abort conversion
                return tempMat;
            }

            float? bumpScale = null;
            Color? emissionColor = null;

            // Grab values before the shader changes
            if (tempMat.IsKeywordEnabled("_NORMALMAP"))
            {
                bumpScale = tempMat.GetFloat("_BumpScale");
            }
            if (tempMat.IsKeywordEnabled("_EMISSION"))
            {
                emissionColor = tempMat.GetColor("_EmissionColor");
            }

            // Set shader
            tempMat.shader = HGShader;

            // Apply values after shader is set
            tempMat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            tempMat.EnableKeyword("DITHER");

            if (bumpScale != null)
            {
                tempMat.SetFloat("_NormalStrength", (float)bumpScale);
                tempMat.SetTexture("_NormalTex", tempMat.GetTexture("_BumpMap"));
            }
            if (emissionColor != null)
            {
                tempMat.SetColor("_EmColor", (Color)emissionColor);
                tempMat.SetFloat("_EmPower", 1);
            }

            // Set this keyword in unity if you want your model to show backfaces
            // In unity, right click the inspector tab and choose Debug
            if (tempMat.IsKeywordEnabled("NOCULL"))
            {
                tempMat.SetInt("_Cull", 0);
            }
            // Set this keyword in unity if you've set up your model for limb removal item displays (eg. goat hoof) by setting your model's vertex colors
            if (tempMat.IsKeywordEnabled("LIMBREMOVAL"))
            {
                tempMat.SetInt("_LimbRemovalOn", 1);
            }

            // Cache this material to show that it's already been converted
            HGCachedMaterials.Add(tempMat);

            // Return newly converted material
            return tempMat;
        }

        public static void ConvertAllRenderersToHopooShader(GameObject _objectToConvert)
        {
            // Check for object
            if (!_objectToConvert) return;

            // Cycle through renderers
            foreach (Renderer renderer in _objectToConvert.GetComponentsInChildren<Renderer>())
            {
                // Check for renderer
                if (renderer)
                {
                    // Check for material
                    if (renderer.sharedMaterial)
                    {
                        // Convert material to Hopoo Games shader
                        renderer.sharedMaterial.ConvertDefaultShaderToHopoo();
                    }
                }
            }
        }

        public static string FindChildNameInsensitive(this ChildLocator _childLocator, string _child)
        {
            // Return child matching name ignoring case
            return _childLocator.transformPairs.Where((pair) => pair.name.ToLowerInvariant() == _child.ToLowerInvariant()).FirstOrDefault().name;
        }

        public static GameObject FindChildGameObjectInsensitive(this ChildLocator _childLocator, string _child)
        {
            // Return game object of child found using case insensitive search
            return _childLocator.FindChildGameObject(_childLocator.FindChildNameInsensitive(_child));
        }

        public static string Pluralize(string _phrase)
        {
            // Check for phrase
            if (string.IsNullOrWhiteSpace(_phrase)) return _phrase;

            // Split into words
            string[] words = _phrase.Split(' ');
            string lastWord = words[words.Length - 1];

            // Check if the last word is an irregular plural
            if (irregularPlurals.TryGetValue(lastWord, out string pluralForm))
            {
                // Replace last word with plural form
                words[words.Length - 1] = pluralForm;
            }
            else
            {
                // Apply standard pluralization rules to the last word
                words[words.Length - 1] = PluralizeWord(lastWord);
            }

            // Return joined words
            return string.Join(" ", words);
        }

        private static string PluralizeWord(string _word)
        {
            // When ending with 's' assume already plural
            if (Regex.IsMatch(_word, "(s)$", RegexOptions.IgnoreCase)) return _word;

            // Common last letters for 'es' ending
            if (Regex.IsMatch(_word, "(x|z|ch|sh)$", RegexOptions.IgnoreCase)) return _word + "es";

            // Common last letters for 'ies' ending
            if (Regex.IsMatch(_word, "[^aeiou]y$", RegexOptions.IgnoreCase)) return _word.Substring(0, _word.Length - 1) + "ies";

            // Common last letters for 'ves' ending
            if (Regex.IsMatch(_word, "(f|fe)$", RegexOptions.IgnoreCase)) return Regex.Replace(_word, "(f|fe)$", "ves");

            // Just add 's' by default
            return _word + "s";
        }

        public static void AnalyseGameObject(GameObject _gameObject)
        {
            // Create message stirng
            string message = "\n====================\n";

            // Add analysis to message
            message += AnalyseGameObject(_gameObject.transform);

            // Log message
            Log.Info(message);
        }

        private static string AnalyseGameObject(Transform _current, string _parents = "")
        {
            // Add to parents string
            string parents = _parents == "" ? _current.name : _parents + $" > {_current.name}";

            // Create message string
            string message = "";

            // Check if original game object
            if (_parents == "")
            {
                // Add name of origin object
                message += $"Analysing '{_current.name}'\n--------------------\n";

                // Keep track of parents of origin object
                string originParents = "";

                // Keep track of current parent
                Transform parent = _current;

                // Cycle until no parent found
                while (parent.parent != null)
                {
                    // Update current parent
                    parent = parent.parent;

                    // Add parent to parents string
                    originParents = originParents == "" ? parent.name : $"{parent.name} > {originParents}";
                }

                // Check for origin parents
                if (originParents != "")
                {
                    // Add details on origin parents
                    message += $"Origin Parents:\n{originParents}\n--------------------\n";
                }
            }

            // Not original game object
            else
            {
                // Add name and parents of current object
                message += $"Analysing '{_current.name}':\n{parents}\n--------------------\n";
            }

            // Add information on tag and layer and if enabled
            message += $"Enabled: {_current.gameObject.activeInHierarchy}\nTag: '{_current.tag}'\nLayer: '{LayerMask.LayerToName(_current.gameObject.layer)}' ({_current.gameObject.layer})\n--------------------\n";

            // Add information on position
            message += _current.parent == null ? $"Position: ({_current.position.x}, {_current.position.y}, {_current.position.z})" : $"World Position: ({_current.position.x}, {_current.position.y}, {_current.position.z})\nLocal Position: ({_current.localPosition.x}, {_current.localPosition.y}, {_current.localPosition.z})";

            // Add breaker
            message += "\n--------------------\n";

            // Add information on rotation
            message += _current.parent == null ? $"Rotation: ({_current.eulerAngles.x}, {_current.eulerAngles.y}, {_current.eulerAngles.z})" : $"World Rotation: ({_current.eulerAngles.x}, {_current.eulerAngles.y}, {_current.eulerAngles.z})\nLocal Rotation: ({_current.localEulerAngles.x}, {_current.localEulerAngles.y}, {_current.localEulerAngles.z})";

            // Add breaker
            message += "\n--------------------\n";

            // Add information on scale
            message += _current.parent == null ? $"Scale: ({_current.lossyScale.x}, {_current.lossyScale.y}, {_current.lossyScale.z})" : $"World Scale: ({_current.lossyScale.x}, {_current.lossyScale.y}, {_current.lossyScale.z})\nLocal Scale: ({_current.localScale.x}, {_current.localScale.y}, {_current.localScale.z})";

            // Add breaker
            message += "\n--------------------\n";

            // Add information on components
            message += "Components:";

            // Keep track of materials in renderers
            string materials = "";

            // Cycle through each component
            foreach (Component component in _current.GetComponents<Component>())
            {
                // Add to message string
                message += $"\n - {component.GetType()}";

                // Check if component is a renderer
                if (component is Renderer)
                {
                    // Cycle through materials
                    foreach (Material material in ((Renderer)component).materials)
                    {
                        // Check if new line needed
                        if (materials != "")
                        {
                            materials += "\n";
                        }

                        // Add material details
                        materials += $" - {material.name}";

                        // Check for texture
                        if (material.mainTexture != null)
                        {
                            // Add texture details
                            materials += $"\n     Texture: '{material.mainTexture.name}'";
                        }

                        // Add shader details
                        materials += $"\n     Shader: '{material.shader.name}'";

                        // Store shader properties
                        string shaderProperties = "";

                        // Get shader property count
                        int propertyCount = material.shader.GetPropertyCount();

                        // Enabled local keywords on this material
                        string[] kws = material.shaderKeywords;

                        if (kws != null && kws.Length > 0)
                        {
                            shaderProperties += "         Enabled keywords:";
                            foreach (string kw in kws) shaderProperties += $"\n             {kw}";
                        }
                        else
                        {
                            shaderProperties += "\n         Enabled keywords:\n             (none)";
                        }

                        // Cycle through properties
                        for (int i = 0; i < propertyCount; i++)
                        {
                            // Check if new line needed
                            if (shaderProperties != "")
                            {
                                shaderProperties += "\n         ";
                            }

                            // Get the property name
                            string propertyName = material.shader.GetPropertyName(i);

                            // Get the property type
                            ShaderPropertyType propertyType = material.shader.GetPropertyType(i);

                            // Add shader property information based on type
                            switch (propertyType)
                            {
                                case ShaderPropertyType.Color:
                                    Color colorValue = material.GetColor(propertyName);
                                    shaderProperties += $"Colour '{propertyName}' - ({colorValue.r}, {colorValue.g}, {colorValue.b}, {colorValue.a})";
                                    break;
                                case ShaderPropertyType.Vector:
                                    Vector4 vectorValue = material.GetVector(propertyName);
                                    shaderProperties += $"Vector '{propertyName}' - ({vectorValue.x}, {vectorValue.y}, {vectorValue.z}, {vectorValue.w})";
                                    break;
                                case ShaderPropertyType.Float:
                                    shaderProperties += $"Float '{propertyName}' - {material.GetFloat(propertyName)}";
                                    break;
                                case ShaderPropertyType.Int:
                                    shaderProperties += $"Int '{propertyName}' - {material.GetInt(propertyName)}";
                                    break;
                                case ShaderPropertyType.Range:
                                    shaderProperties += $"Range '{propertyName}' - {material.GetFloat(propertyName)}";
                                    break;
                                case ShaderPropertyType.Texture:
                                    Texture texture = material.GetTexture(propertyName);
                                    string textureValue = texture != null ? $"'{texture.name}'" : "None";
                                    shaderProperties += $"Texture '{propertyName}' - {textureValue}";
                                    break;
                            }
                        }

                        // Add shader property details
                        materials += $"\n{shaderProperties}";
                    }
                }
            }

            // Check if materials were found
            if (materials != "")
            {
                // Add breaker
                message += "\n--------------------\n";

                // Add information for materials
                message += $"Renderer Materials:\n{materials}";
            }

            // Add breaker
            message += "\n====================";

            // Cycle through children
            foreach (Transform child in _current)
            {
                // Add analysis of child to message string
                message += $"\n{AnalyseGameObject(child, parents)}";
            }

            // Return message string
            return message;
        }

        public static FaithfulRadiusIndicatorBehaviour CreateRadiusIndicator(Transform _parent, float _startingSize, Color _colour, float _adjustmentSpeed = 1.0f)
        {
            // Assume starting size is starting target size
            return CreateRadiusIndicator(_parent, _startingSize, _startingSize, _colour, _adjustmentSpeed);
        }

        public static FaithfulRadiusIndicatorBehaviour CreateRadiusIndicator(Transform _parent, float _startingSize, float _startingTargetSize, Color _colour, float _adjustmentSpeed = 1.0f)
        {
            // Instantiate new radius indicator
            GameObject newObj = Object.Instantiate(Assets.radiusIndicatorPrefab, _parent);

            // Get radius behaviour
            FaithfulRadiusIndicatorBehaviour behaviour = newObj.GetComponent<FaithfulRadiusIndicatorBehaviour>();

            // Initialise radius behaviour
            behaviour.Init(_parent, _startingSize, _colour, _adjustmentSpeed);

            // Set radius target size
            behaviour.SetTargetSize(_startingTargetSize);

            // Return behaviour
            return behaviour;
        }

        public static FaithfulRadiusIndicatorBehaviour CreateRadiusIndicator(CharacterBody _parent, float _startingSize, Color _colour, float _adjustmentSpeed = 1.0f)
        {
            // Assume starting size is starting target size
            return CreateRadiusIndicator(_parent, _startingSize, _startingSize, _colour, _adjustmentSpeed);
        }

        public static FaithfulRadiusIndicatorBehaviour CreateRadiusIndicator(CharacterBody _parent, float _startingSize, float _startingTargetSize, Color _colour, float _adjustmentSpeed = 1.0f)
        {
            // Instantiate new radius indicator
            GameObject newObj = Object.Instantiate(Assets.radiusIndicatorPrefab, _parent.corePosition, Quaternion.identity);

            // Get radius behaviour
            FaithfulRadiusIndicatorBehaviour behaviour = newObj.GetComponent<FaithfulRadiusIndicatorBehaviour>();

            // Initialise radius behaviour
            behaviour.Init(_parent.modelLocator.modelTransform, _startingSize, _colour, _adjustmentSpeed);

            // Set radius target size
            behaviour.SetTargetSize(_startingTargetSize);

            // Return behaviour
            return behaviour;
        }

        public static GameObject FindChildWithTerm(Transform _parent, string _searchTerm)
        {
            // Cycle through children
            foreach (Transform child in _parent)
            {
                // Check child name
                if (child.name.Contains(_searchTerm))
                {
                    // Return child
                    return child.gameObject;
                }
            }

            // Did not find child
            return null;
        }

        public static GameObject[] FindChildrenWithTerm(Transform _parent, string _searchTerm)
        {
            // List of children with term in name
            List<GameObject> matching = new List<GameObject>();

            // Cycle through children
            foreach (Transform child in _parent)
            {
                // Check child name
                if (child.name.Contains(_searchTerm))
                {
                    // Add child to list
                    matching.Add(child.gameObject);
                }
            }

            // Return found children
            return matching.ToArray();
        }

        public static bool HasLanguageString(string _token)
        {
            // Return if language file has string
            return languageDictionary.ContainsKey(_token);
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
            RoR2.HurtBox[] hurtBoxes = new SphereSearch // SPHERE SEARCH IS LIMITED TO ONLY 100 COLLIDERS (BEFORE FILTERS) SO IT CAN BE VERY UNRELIABLE
            {
                radius = radius,
                mask = LayerIndex.entityPrecise.mask,
                origin = position
            }.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            return hurtBoxes;   // Return found hurt boxes
        }

        // Returns all character bodies in holdout zone
        public static CharacterBody[] GetCharacterBodiesInHoldoutZone(HoldoutZoneController _holdoutZone)
        {
            // Create list of character bodies
            List<CharacterBody> inHoldoutZone = new List<CharacterBody>();
            
            // Cycle through character bodies
            foreach (CharacterBody current in CharacterBody.readOnlyInstancesList)
            {
                // Check if character body is in holdout zone
                if (_holdoutZone.IsBodyInChargingRadius(current))
                {
                    // Add to in holdout zone list
                    inHoldoutZone.Add(current);
                }
            }

            // Return list converted to array
            return inHoldoutZone.ToArray();
        }

        public static TMP_FontAsset GetFont(string _fontName)
        {
            // Check for font name
            if (_fontName == null) return null;

            // Check for font
            if (!fontCache.ContainsKey(_fontName))
            {
                // Attempt to load more fonts
                LoadFonts();

                // Check again for font
                if (!fontCache.ContainsKey(_fontName)) return null;
            }

            // Return font
            return fontCache[_fontName];
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

        public static ItemDef GetItem(string _identifier)
        {
            // Attempt to get item def normally
            ItemDef item = ItemCatalog.GetItemDef(ItemCatalog.FindItemIndex(_identifier));

            // Check for item def
            if (item == null)
            {
                // This can error if Item Catalog isn't ready
                try
                {
                    // Attempt to fetch item with proper name
                    item = ItemCatalog.allItemDefs.Where(x => Language.GetString(x.nameToken) == _identifier).FirstOrDefault();
                }
                catch
                {
                    // Couldn't find item yet
                    return null;
                }
            }

            // Return result
            return item;
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

        public static string GetChildTree(Transform target, int indentation = 0)
        {
            // Initialise child tree string
            string childTree = "";

            // Get next indentation
            int nextIndentation = indentation + 1;

            // Cycle for indentation
            for (int i = 0; i < indentation; i++)
            {
                // Add indentation
                childTree += "    ";
            }

            // Get for null transform
            if (target == null)
            {
                // Return null tree string
                return childTree + "- NULL";
            }

            // Add target to child tree string
            childTree += $"- {target.name}";

            // Cycle through children
            foreach (Transform child in target)
            {
                // Add child's child tree to this child tree string
                childTree += $"\n{GetChildTree(child, nextIndentation)}";
            }

            // Return child tree string
            return childTree;
        }

        public static string GetLanguageString(string _token, bool _prioritiseRoR2Language = false)
        {
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
                    Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");
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
                    Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");

                    return Language.GetString(_token); // Revert to RoR2.Language
                }
            }
        }

        public static string GetXMLLanguageString(string _token)
        {
            // Check for token
            if (languageDictionary.ContainsKey(_token))
            {
                // Get string
                string languageString = languageDictionary[_token];

                // Return XML safe language string
                return GetXMLSafeString(languageString);
            }

            // Token not found
            else
            {
                Log.Warning($"[UTILS] - Language token '{_token}' requested but not found.");
                return _token; // Return original token
            }
        }

        public static string GetXMLSafeString(string _unsafe)
        {
            // Remove invalid characters for XML
            string safe = Regex.Replace(_unsafe, @"[^a-zA-Z0-9_]", "_");

            // Ensure the name starts with a letter or underscore
            if (char.IsDigit(safe[0]))
            {
                safe = $"_{safe}";
            }

            return safe;
        }

        public static List<T> GetComponentsInParentsWithInterface<T>(Transform _transform)
        {
            // Initialise list of components with interface
            List<T> components = new List<T>();

            // Start at given transform
            Transform current = _transform;

            // Cycle until no more parents are found
            while (current != null)
            {
                // Cycle through components
                foreach (Component component in current.GetComponents<Component>())
                {
                    // Check if component uses interface
                    if (component is T validComponent)
                    {
                        // Add to components with interface
                        components.Add(validComponent);
                    }
                }

                // Move to parent transform
                current = current.parent;
            }

            // Return list of components with interface
            return components;
        }

        public static void SetLayer(GameObject _object, string _layer)
        {
            // Fetch layer index and call alternate method
            SetLayer(_object, LayerMask.NameToLayer(_layer));
        }

        public static void SetLayer(GameObject _object, int _layer)
        {
            // Check for object
            if (_object == null) return;

            // Set layer
            _object.layer = _layer;

            // Recursively assign the layer to all children
            foreach (Transform child in _object.transform)
            {
                // Check for child
                if (child != null)
                {
                    // Perform recursion
                    SetLayer(child.gameObject, _layer);
                }
            }
        }

        public static bool debugMode
        {
            get { return _debugMode; }
        }

        public static bool verboseConsole
        {
            get { return _verboseConsole; }
        }

        public static bool randomiserMode
        {
            get { return _randomiserMode; }
        }

        /*public static bool lookingGlassInstalled
        {
            get { return lookingGlassAssembly != null; }
        }

        public static bool lookingGlassFullItemDescsEnabled
        {
            get
            {
                // False if LookingGlass not installed
                if (!lookingGlassInstalled) return false;

                // Check for config entry
                if (lookingGlassFullItemDescs == null)
                {
                    // Get the ItemStats type
                    System.Type itemStatsType = lookingGlassAssembly.GetType("LookingGlass.ItemStatsNameSpace.ItemStats");
                    if (itemStatsType != null)
                    {
                        // Get the fullDescOnPickup field
                        FieldInfo field = itemStatsType.GetField("fullDescOnPickup", BindingFlags.Public | BindingFlags.Static);
                        if (field != null)
                        {
                            // Get the value of the field
                            object value = field.GetValue(null);
                            if (value != null)
                            {
                                // Assign LookingGlass full item descriptions setting reference
                                lookingGlassFullItemDescs = value as ConfigEntry<bool>;
                            }
                        }
                    }

                    // Check for config entry again
                    if (lookingGlassFullItemDescs == null) return false;
                }

                // Return value of config entry
                return lookingGlassFullItemDescs.Value;
            }
        }*/

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

        public static NetUtils netUtils
        {
            get
            {
                // Return net utils
                return _netUtils;
            }
            set
            {
                // Set net utils
                _netUtils = value;
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
        private string corruptedOverride = "";

        public CorruptPair(ItemDef _corrupter, string _corruptedToken, string _corruptedOverride)
        {
            // Assign corrupter and corrupted
            corrupter = _corrupter;
            corruptedToken = _corruptedToken;
            corruptedOverride = _corruptedOverride;
        }

        public ItemDef.Pair GetPair()
        {
            // Copy corrupted token to local
            string localCorruptedToken = corruptedToken;

            // Check for corrupt override
            if (corruptedOverride != "")
            {
                // Get item to corrupt
                ItemDef overrideCorrupted = Utils.GetItem(corruptedOverride);

                // Check for new corrupted item
                if (overrideCorrupted != null)
                {
                    // Return setup item pair
                    return new ItemDef.Pair { itemDef1 = overrideCorrupted, itemDef2 = corrupter };
                }

                // Override not found
                else
                {
                    // Log warning
                    Log.Warning($"[UTILS] | Failed to assign override for corrupted item '{corruptedOverride}' for void item '{corrupter.name}' reverting to default item.");
                }
            }
            
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

        // List of item display specifies
        public List<ItemDisplaySpecifier> displaySpecifiers = new List<ItemDisplaySpecifier>();

        public ItemDisplaySettings(GameObject _model, ItemDisplayRuleDict _displayRules)
        {
            // Assign model and display rules
            model = _model;
            displayRules = _displayRules;
        }

        public void AddCharacterDisplay(string _character, string _childName, Vector3 _position, Vector3 _angle, Vector3 _scale, string _characterBodySpecifier = null)
        {
            // Get character model name
            string modelName = Utils.GetCharacterModelName(_character.ToLower());

            // Check for model name
            if (modelName == null)
            {
                Log.Error($"Couldn't assign item on character display data for character '{_character}', character not found");
                return;
            }

            // Check for character body specifier
            if (_characterBodySpecifier != null)
            {
                // Add to display specifiers
                displaySpecifiers.Add(new ItemDisplaySpecifier(_childName, _position, _angle, _scale, _characterBodySpecifier));
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
                        followerPrefabAddress = new AssetReferenceGameObject(""),
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
                    followerPrefabAddress = new AssetReferenceGameObject(""),
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

        public GameObject GetModel()
        {
            // Return display model
            return model;
        }
    }

    [System.Serializable]
    internal struct ItemDisplaySpecifier
    {
        // Store identifiers for display
        [SerializeField] string parentName;
        [SerializeField] Vector3 position;
        [SerializeField] Vector3 angle;
        [SerializeField] Vector3 scale;

        // Character specifier string
        [SerializeField] string characterSpecifier;

        public ItemDisplaySpecifier(string _parentName, Vector3 _position, Vector3 _angle, Vector3 _scale, string _characterSpecifier)
        {
            // Assign identifiers
            parentName = _parentName.ToLower();
            position = _position;
            angle = _angle;
            scale = _scale;

            // Assign character specifier
            characterSpecifier = _characterSpecifier;
        }

        public bool MatchesSpecifier(GameObject _gameObject)
        {
            // Get game object transform
            Transform transform = _gameObject.transform;
            
            // Compare identifiers
            if (parentName == transform.parent.name.ToLower() && position == transform.localPosition && angle == transform.localEulerAngles && scale == transform.localScale) return true;

            // Doesn't match identifiers
            return false;
        }

        public BodyIndex bodyIndex => BodyCatalog.FindBodyIndex(characterSpecifier);
    }

    // Wrapper class for deserialization of the language file
    public class LanguageWrapper
    {
        public Dictionary<string, string> strings; // The tokens and their corresponding strings
    }
}
