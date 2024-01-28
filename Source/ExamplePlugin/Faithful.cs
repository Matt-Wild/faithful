using BepInEx;
using IL.RoR2.Artifacts;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;

namespace Faithful
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class Faithful : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SpilledSoup";
        public const string PluginName = "Faithful";
        public const string PluginVersion = "1.0.0";

        // Plugin info
        public static PluginInfo PInfo { get; private set; }

        // Toolbox
        Toolbox toolbox;

        // If in debug mode
        public const bool debugMode = true;

        // Game info
        public bool escapeZoneActive = false;
        HoldoutZoneController escapeZone;

        // Asset manager
        Assets assets;

        // Dev buff
        private static BuffDef devBuffDef;

        // Item def
        private static ItemDef itemDef;
        private static BuffDef buffDef;

        // Simulacrum banned items
        List<ItemDef> simulacrumBanned = [];

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Update plugin info
            PInfo = Info;

            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);

            // Initialise toolbox
            toolbox = new Toolbox();

            // Initialise asset manager
            assets = new Assets();
            assets.Init(PInfo);

            // Define item
            itemDef = ScriptableObject.CreateInstance<ItemDef>();

            // Set item data
            itemDef.name = "FAITHFUL_COPPER_GEAR_NAME";
            itemDef.nameToken = "FAITHFUL_COPPER_GEAR_NAME";
            itemDef.pickupToken = "FAITHFUL_COPPER_GEAR_PICKUP";
            itemDef.descriptionToken = "FAITHFUL_COPPER_GEAR_DESC";
            itemDef.loreToken = "FAITHFUL_COPPER_GEAR_LORE";
            itemDef.tier = ItemTier.Tier1;
            itemDef.tags = [ItemTag.Damage, ItemTag.HoldoutZoneRelated];
            simulacrumBanned.Add(itemDef);

            // Define dev buff
            devBuffDef = ScriptableObject.CreateInstance<BuffDef>();
            devBuffDef.name = "FAITHFUL_DEV_BUFF";
            devBuffDef.buffColor = Color.white;
            devBuffDef.canStack = false;
            devBuffDef.isDebuff = false;
            devBuffDef.iconSprite = assets.GetIcon("assets/testing/texbuffdevmode.png");

            // Define buff
            buffDef = ScriptableObject.CreateInstance<BuffDef>();
            buffDef.name = "FAITHFUL_COPPER_GEAR_BUFF";
            buffDef.buffColor = Color.yellow;
            buffDef.canStack = true;
            buffDef.isDebuff = false;
            buffDef.iconSprite = assets.GetIcon("assets/testing/texbuffteleportergear.png");

            // Hook buff behaviour
            R2API.RecalculateStatsAPI.GetStatCoefficients += CopperGearStatMod;
            R2API.RecalculateStatsAPI.GetStatCoefficients += GodModeStatMod;

            // You can create your own icons and prefabs through assetbundles, but to keep this boilerplate brief, we'll be using question marks.
            itemDef.pickupIconSprite = assets.GetIcon("assets/testing/texcoppergearicon.png");
            itemDef.pickupModelPrefab = assets.GetModel("assets/testing/coppergearmesh.prefab");

            // Can remove determines
            // if a shrine of order,
            // or a printer can take this item,
            // generally true, except for NoTier items.
            itemDef.canRemove = true;

            // Hidden means that there will be no pickup notification,
            // and it won't appear in the inventory at the top of the screen.
            // This is useful for certain noTier helper items, such as the DrizzlePlayerHelper.
            itemDef.hidden = false;

            // Hook holdout zone behaviour
            /*On.RoR2.TeleporterInteraction.FixedUpdate += TeleporterFixedUpdate;
            On.RoR2.EscapeSequenceController.BeginEscapeSequence += EscapeSequenceBegin;
            On.EntityStates.Missions.Moon.MoonBatteryActive.FixedUpdate += MoonBatteryFixedUpdate;
            On.EntityStates.Missions.Arena.NullWard.Active.FixedUpdate += NullWardFixedUpdate;
            On.EntityStates.DeepVoidPortalBattery.Charging.FixedUpdate += PortalBatteryFixedUpdate;*/

            // Behaviour for characters in Holdout Zone
            toolbox.behaviour.CallInHoldoutZone(InHoldoutZone);

            // You can add your own display rules here,
            // where the first argument passed are the default display rules:
            // the ones used when no specific display rules for a character are found.
            // For this example, we are omitting them,
            // as they are quite a pain to set up without tools like https://thunderstore.io/package/KingEnderBrine/ItemDisplayPlacementHelper/
            var displayRules = new ItemDisplayRuleDict(null);

            // Add item to R2API
            ItemAPI.Add(new CustomItem(itemDef, displayRules));

            // Add dev buff
            ContentAddition.AddBuffDef(devBuffDef);

            // Add buff
            ContentAddition.AddBuffDef(buffDef);

            // But now we have defined an item, but it doesn't do anything yet. So we'll need to define that ourselves.
            //GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;

            // On damage dealt behaviour
            GlobalEventManager.onServerDamageDealt += OnDamageDealt;

            // Config Simulacrum
            On.RoR2.InfiniteTowerRun.OverrideRuleChoices += InjectSimulacrumBannedItems;

            // Log successful load information
            Log.Info("Faithful loaded successfully, thank you for playing!");
        }

        public void PortalBatteryFixedUpdate(On.EntityStates.DeepVoidPortalBattery.Charging.orig_FixedUpdate orig, EntityStates.DeepVoidPortalBattery.Charging self)
        {
            InHoldoutZoneBehaviour(toolbox.utils.GetHurtBoxesInSphere(self.holdoutZoneController.transform.position, self.holdoutZoneController.currentRadius));   // Do holdout zone behaviour

            orig(self); // Run normal processes
        }

        public void NullWardFixedUpdate(On.EntityStates.Missions.Arena.NullWard.Active.orig_FixedUpdate orig, EntityStates.Missions.Arena.NullWard.Active self)
        {
            InHoldoutZoneBehaviour(toolbox.utils.GetHurtBoxesInSphere(self.holdoutZoneController.transform.position, self.holdoutZoneController.currentRadius));   // Do holdout zone behaviour

            orig(self); // Run normal processes
        }

        public void MoonBatteryFixedUpdate(On.EntityStates.Missions.Moon.MoonBatteryActive.orig_FixedUpdate orig, EntityStates.Missions.Moon.MoonBatteryActive self)
        {
            InHoldoutZoneBehaviour(toolbox.utils.GetHurtBoxesInSphere(self.holdoutZoneController.transform.position, self.holdoutZoneController.currentRadius));   // Do holdout zone behaviour

            orig(self); // Run normal processes
        }

        public void TeleporterFixedUpdate(On.RoR2.TeleporterInteraction.orig_FixedUpdate orig, RoR2.TeleporterInteraction self)
        {
            HurtBox[] inside = toolbox.utils.GetHurtBoxesInSphere(self.holdoutZoneController.transform.position, self.holdoutZoneController.currentRadius);   // Get inside hurtboxes

            InHoldoutZoneBehaviour(inside);   // Do holdout zone behaviour

            if (debugMode)  // Debug mode behaviour
            {
                // God Mode teleporter charging effect
                foreach (HurtBox current in inside)
                {
                    CharacterBody body = current.healthComponent.body;  // Get character body

                    if (body.GetBuffCount(devBuffDef) > 0)  // Is in God Mode?
                    {
                        self.holdoutZoneController.baseChargeDuration = 0.0f;
                    }
                }

                orig(self); // Run normal processes
            }
        }

        public void EscapeSequenceBegin(On.RoR2.EscapeSequenceController.orig_BeginEscapeSequence orig, RoR2.EscapeSequenceController self)
        {
            Log.Debug("ESCAPE SEQUENCE BEGIN");

            HoldoutZoneController[] zones = FindObjectsOfType<HoldoutZoneController>();
            foreach (HoldoutZoneController zone in zones)
            {
                if (zone.gameObject.name == "HoldoutZone")
                {
                    escapeZoneActive = true;
                    escapeZone = zone;
                    Log.Debug("Found escape ship holdout zone");
                    break;
                }
            }

            orig(self); // Run normal processes
        }

        void InHoldoutZoneBehaviour(RoR2.HurtBox[] containedHurtBoxes)
        {
            foreach (RoR2.HurtBox hurtBox in containedHurtBoxes) // Cycle through entities in teleporter radius
            {
                CharacterBody body = hurtBox.healthComponent.body;  // Get character body

                Inventory inventory = body.inventory;
                if (!inventory)
                {
                    continue;   // Continue, entity doesn't have inventory
                }

                int copperGearCount = inventory.GetItemCount(itemDef.itemIndex);    // Get copper gear amount
                if (copperGearCount > 0)
                {
                    toolbox.utils.RefreshTimedBuffs(body, buffDef, 1);    // Refresh buffs

                    int needed = copperGearCount - body.GetBuffCount(buffDef);  // Get needed amount of buffs
                    for (int i = 0; i < needed; i++)    // Catch up buff count
                    {
                        body.AddTimedBuff(buffDef, 1);  // Add copper gear buff
                    }

                }
            }
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory)
            {
                // Get Copper Gear amount
                int copperGearCount = inventory.GetItemCount(itemDef.itemIndex);

                // Has Copper Gears?
                if (copperGearCount > 0)
                {
                    // Refresh Copper Gear buffs
                    toolbox.utils.RefreshTimedBuffs(_body, buffDef, 1);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(buffDef);

                    // Catch up buff count
                    for (int i = 0; i < needed; i++)
                    {
                        // Add Copper Gear buff
                        _body.AddTimedBuff(buffDef, 1);
                    }
                }
            }

            // Debug mode behaviour
            if (debugMode)  
            {
                // Is character in God Mode
                if (_body.GetBuffCount(devBuffDef) > 0)
                {
                    // Instantly charge Holdout Zone
                    _zone.baseChargeDuration = 0.0f;
                }
            }
        }

        void InjectSimulacrumBannedItems(On.RoR2.InfiniteTowerRun.orig_OverrideRuleChoices orig, RoR2.InfiniteTowerRun self, RoR2.RuleChoiceMask mustInclude, RoR2.RuleChoiceMask mustExclude, ulong runSeed)
        {
            List<ItemDef> newBanned = [];
            foreach (ItemDef target in simulacrumBanned)    // Cycle through current banned items
            {
                if (!self.blacklistedItems.Contains(target))    // Contains item?
                {
                    Log.Debug($"Banning {target.nameToken} from Simulacrum");
                    newBanned.Add(target); // Add to needed list for banning
                }
            }

            if (newBanned.Count > 0)   // Needs to ban extra items
            {
                foreach (ItemDef current in self.blacklistedItems)  // Add existing banned items to 
                {
                    newBanned.Add(current); // Update new banned list
                }

                self.blacklistedItems = newBanned.ToArray();    // Update blacklisted items
            }

            orig(self, mustInclude, mustExclude, runSeed);  // Run normal processes
        }

        private void OnDamageDealt(DamageReport report)
        {
            // Does victim have God Mode
            if (report.victimBody.GetBuffCount(devBuffDef) > 0)
            {
                // Set health to max
                report.victim.health = report.victim.fullHealth;
            }
        }

        // Old example behaviour
        /*private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            // If a character was killed by the world, we shouldn't do anything.
            if (!report.attacker || !report.attackerBody)
            {
                return;
            }

            var attackerCharacterBody = report.attackerBody;

            // We need an inventory to do check for our item
            if (attackerCharacterBody.inventory)
            {
                // Store the amount of our item we have
                var garbCount = attackerCharacterBody.inventory.GetItemCount(itemDef.itemIndex);
                if (garbCount > 0 &&
                    // Roll for our 50% chance.
                    Util.CheckRoll(50, attackerCharacterBody.master))
                {
                    // Since we passed all checks, we now give our attacker the cloaked buff.
                    // Note how we are scaling the buff duration depending on the number of the custom item in our inventory.
                    attackerCharacterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, 3 + garbCount);
                }
            }
        }*/

        private void FixedUpdate()
        {
            // Escape holdout zone behaviour
            if (escapeZoneActive)
            {
                if (escapeZone)
                {
                    InHoldoutZoneBehaviour(toolbox.utils.GetHurtBoxesInSphere(escapeZone.transform.position, escapeZone.currentRadius));   // Do holdout zone behaviour
                }
                else
                {
                    // Escape zone is null reference, assume escape zone is deactivated
                    escapeZoneActive = false;
                }
            }
        }

        // The Update() method is run on every frame of the game.
        private void Update()
        {
            if (!debugMode)
            {
                return; // Update contains only debug functionality
            }

            UpdateGodMode();    // Update God Mode stuff

            // Is F1 key down - Item spawning
            if (Input.GetKey(KeyCode.F1))
            {
                bool spawn = false;
                PickupIndex index = PickupCatalog.FindPickupIndex(ItemTier.Tier1);

                // Item tier picking behaviour
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    spawn = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier3);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Boss);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Lunar);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier3);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidBoss);
                }

                if (spawn)  // Spawn command essense of selected tier
                {
                    Transform transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                    Log.Debug($"Spawning item at coordinates {transform.position}");
                    GameObject essence = UnityEngine.Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CommandCube"), transform.position, transform.rotation);
                    essence.GetComponent<PickupIndexNetworker>().NetworkpickupIndex = index;
                    essence.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(index);
                    NetworkServer.Spawn(essence);
                }
            }

            // Is F2 pressed - God mode
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Get character body
                CharacterBody body = PlayerCharacterMasterController.instances[0].master.GetBody();

                // Has god mode already?
                if (body.GetBuffCount(devBuffDef) > 0)
                {
                    // Disable godmode
                    body.RemoveBuff(devBuffDef);

                    // REMOVED UNTIL TESTING
                    //body.name = body.name.Replace("[GODMODE] ", "");   // Remove God Mode name

                    Log.Debug("God Mode disabled");
                }
                else
                {
                    // Enable godmode
                    body.AddBuff(devBuffDef);

                    // REMOVED UNTIL TESTING
                    //body.name = "[GODMODE] " + body.name;   // Set God Mode name

                    Log.Debug("God Mode enabled!");
                }
            }
        }

        private void UpdateGodMode()
        {
            if (!debugMode)
            {
                return; // Return if not in debug mode
            }

            if (PlayerCharacterMasterController.instances.Count <= 0)
            {
                return; // No player character
            }

            // Get character body
            CharacterBody body = PlayerCharacterMasterController.instances[0].master.GetBody();

            if (!body)
            {
                return; // Player body not present
            }

            // Has God Mode?
            if (body.GetBuffCount(devBuffDef) > 0)
            {
                // Clear timed debuffs
                List<BuffIndex> buffClearList = [];
                foreach (CharacterBody.TimedBuff timed in body.timedBuffs)
                {
                    // Is debuff?
                    if (BuffCatalog.GetBuffDef(timed.buffIndex).isDebuff)
                    {
                        buffClearList.Add(timed.buffIndex); // Add to clear list
                    }
                }
                foreach (BuffIndex buffIndex in buffClearList)
                {
                    body.ClearTimedBuffs(buffIndex);    // Clear debuffs
                }
            }
        }

        // Copper Gear stats modification
        private void CopperGearStatMod(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender) // Ensure valid sender
            {
                if (sender.HasBuff(buffDef))    // Check for Copper Gear buff
                {
                    args.attackSpeedMultAdd += 0.25f * sender.GetBuffCount(buffDef);    // Modify attack speed
                }
            }
        }

        // God Mode stats modification
        private void GodModeStatMod(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && debugMode) // Ensure valid sender and in debug mode
            {
                if (sender.HasBuff(devBuffDef))    // Check for God Mode buff
                {
                    args.baseDamageAdd += float.PositiveInfinity;    // God Mode damage
                    args.moveSpeedMultAdd += 2.0f;  // 3x move speed
                    args.jumpPowerMultAdd += 0.5f;  // 1.5x jump power
                    args.cooldownReductionAdd += float.PositiveInfinity;    // God Mode cooldowns
                }
            }
        }
    }
}
