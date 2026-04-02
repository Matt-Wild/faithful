using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace Faithful
{
    internal class CollectorsVision : ItemBase
    {
        // Store item and buff
        Buff inspirationBuff;
        Item collectorsVisionItem;

        // Store reference to inspiration buff behaviour
        Inspiration inspirationBehaviour;

        // Appraiser's Eye item granted after reaching 100% inspiration
        Item appraisersEye;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> countTemporarySetting;
        Setting<bool> capInspirationSetting;
        Setting<int> inspirationGainSetting;
        Setting<int> inspirationGainStackingSetting;
        Setting<float> critDamageSetting;
        Setting<float> inspirationReturnSetting;

        // Store item stats
        bool countTemporary;
        bool capInspiration;
        int inspirationGain;
        int inspirationGainStacking;
        float critDamageMult;
        float inspirationReturnPerc;

        // Used for timing item given notifications for after the original item has already pushed it's notification
        struct PendingEyeNotification
        {
            public CharacterMaster master;
            public PickupIndex grantingPickupIndex;
            public int remainingChecks;
        }
        static readonly List<PendingEyeNotification> pendingEyeNotifications = [];

        // Constructor
        public CollectorsVision(Toolbox _toolbox, Inspiration _inspiration) : base(_toolbox)
        {
            // Assign inspiration buff behaviour
            inspirationBehaviour = _inspiration;

            // Get Vengeance buff
            inspirationBuff = Buffs.GetBuff("INSPIRATION");

            // Get Appraiser's Eye item
            appraisersEye = Items.GetItem("APPRAISERS_EYE");

            // Create display settings
            CreateDisplaySettings("collectorsvisiondisplaymesh");

            // Create Collector's Vision item
            collectorsVisionItem = Items.AddItem("COLLECTORS_VISION", "Collectors Vision", [ItemTag.Damage, ItemTag.Technology, ItemTag.AIBlacklist, ItemTag.BrotherBlacklist, ItemTag.ExtractorUnitBlacklist], "texcollectorsvisionicon", "collectorsvisionmesh", ItemTier.VoidTier3, _corruptToken: "ITEM_CRITDAMAGE_NAME", _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Give Item behaviours
            Behaviour.AddServerOnGiveItemPermanentLateCallback(OnGiveItemPermanent);
            Behaviour.AddServerOnGiveItemTemporaryLateCallback(OnGiveItemTemporary);

            // Add on scene exit behaviour
            Behaviour.AddOnPreSceneExitCallback(OnSceneExit);

            // Hook other behaviour
            On.RoR2.CharacterMasterNotificationQueue.PushPickupNotification_CharacterMaster_PickupIndex_bool_int += OnPushPickupNotification;
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "GunL", new Vector3(0.205F, 0F, 0F), new Vector3(0F, 90F, 180F), new Vector3(0.0525F, 0.0525F, 0.0525F));
            displaySettings.AddCharacterDisplay("Commando", "GunR", new Vector3(-0.205F, 0F, 0F), new Vector3(0F, 270F, 180F), new Vector3(0.0525F, 0.0525F, 0.0525F));
            displaySettings.AddCharacterDisplay("Huntress", "BowBase", new Vector3(0F, -0.0145F, -0.0775F), new Vector3(90F, 180F, 0F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Bandit", "SideWeapon", new Vector3(0F, -0.1165F, 0.1925F), new Vector3(90F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(2.275F, 2.475F, -0.35F), new Vector3(300F, 180F, 90F), new Vector3(0.725F, 0.725F, 0.725F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.11475F, 0.395F, 0.22F), new Vector3(270F, 225F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(-0.11475F, 0.395F, 0.22F), new Vector3(270F, 135F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.805F, 0.54618F, 1.8275F), new Vector3(0F, 0F, 270F), new Vector3(0.1125F, 0.1125F, 0.1125F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0.615F, 0.7805F, 0.71F), new Vector3(0F, 0F, 270F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0F, 0.22075F, -0.20375F), new Vector3(270F, 2.5F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmR", new Vector3(0.0225F, 0.2225F, 0.2045F), new Vector3(270F, 182.5F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0.1865F, 0.0715F, 0.0675F), new Vector3(2.5F, 355F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "AimOriginSyringe", new Vector3(0.2825F, 0.09425F, -0.1575F), new Vector3(0F, 0F, 270F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0.181F, 0.1635F, 0.02625F), new Vector3(0F, 0F, 282.5F), new Vector3(0.065F, 0.065F, 0.065F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(-1.0431F, 4.035F, -1.631F), new Vector3(275F, 35F, 358F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0.1525F, 0.0625F, -0.0075F), new Vector3(271F, 314.5F, 318.5F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(0F, 0.32F, 0.08F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Hand", new Vector3(0.0965F, 0.1135F, 0.01375F), new Vector3(275F, 270F, 2F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Seeker", "HandL", new Vector3(-0.071F, -0.016F, -0.1245F), new Vector3(295.5F, 357.5F, 40F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "HandR", new Vector3(-0.01875F, -0.045F, 0.1425F), new Vector3(286.25F, 207.25F, 317.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(-0.399F, 0.2925F, 0.1575F), new Vector3(0F, 0F, 165F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Cleaver", new Vector3(0.08025F, 0.5105F, 0F), new Vector3(270F, 270F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "PizzaCutter", new Vector3(-0.18175F, 0.4075F, -0.00025F), new Vector3(270F, 90F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "HandL", new Vector3(-0.094F, 0.11F, -0.0165F), new Vector3(270F, 90F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "HandR", new Vector3(0.094F, 0.11F, -0.0165F), new Vector3(270F, 270F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Operator", "MuzzleGun", new Vector3(0F, 0.105F, 0.0348F), new Vector3(2.5F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(-0.275F, -0.023F, -0.295F), new Vector3(275F, 315F, 120F), new Vector3(0.125F, 0.125F, 0.125F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            countTemporarySetting = collectorsVisionItem.CreateSetting("COUNT_TEMPORARY", "Count Temporary Items?", true, "Should this item count temporary items when providing inspiration?", false);
            capInspirationSetting = collectorsVisionItem.CreateSetting("CAP_INSPIRATION", "Cap Inspiration?", true, "Should inspiration granted by this item be capped at 100%?", false);
            inspirationGainSetting = collectorsVisionItem.CreateSetting("INSPIRATION_GAIN", "Inspiration Gain", 1, "How much inspiration should this item give the player when they pick up a new unique item for the stage? (1 = 1% inspiration)", _valueFormatting: "{0:0}%", _minValue: 1);
            inspirationGainStackingSetting = collectorsVisionItem.CreateSetting("INSPIRATION_GAIN_STACKING", "Inspiration Gain Stacking", 1, "How much inspiration should further stacks of this item give the player when they pick up a new unique item for the stage? (1 = 1% inspiration)", _valueFormatting: "{0:0}%", _minValue: 1);
            critDamageSetting = collectorsVisionItem.CreateSetting("CRIT_DAMAGE_MULT", "Crit Damage Multiplier", 20.0f, "How much should 1% of inspiration increase the crit damage multiplier? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            inspirationReturnSetting = collectorsVisionItem.CreateSetting("INSPIRATION_RETURN", "Inspiration Return", 50.0f, "How much inspiration acquired by the end of a stage should the shrine of recollection return? (50.0 = 50% return)", _valueFormatting: "{0:0.0}%", _maxValue: 100.0f);
        }

        public override void FetchSettings()
        {
            // Get item settings
            countTemporary = countTemporarySetting.Value;
            capInspiration = capInspirationSetting.Value;
            inspirationGain = inspirationGainSetting.Value;
            inspirationGainStacking = inspirationGainStackingSetting.Value;
            critDamageMult = critDamageSetting.Value / 100.0f;
            inspirationReturnPerc = inspirationReturnSetting.Value / 100.0f;

            // Send values to inspiration buff
            inspirationBehaviour.critDamageMult = critDamageMult;

            // Update item texts with new settings
            collectorsVisionItem.UpdateItemTexts();
        }

        void OnGiveItemPermanent(Inventory _inventory, ItemIndex _index, int _count)
        {
            // Call OnGiveItem
            OnGiveItem(_inventory, _index, _count);
        }

        void OnGiveItemTemporary(Inventory _inventory, ItemIndex _index, float _count)
        {
            // Ignore if not counting temporary items
            if (!countTemporary) return;

            // Call OnGiveItem
            OnGiveItem(_inventory, _index, _count);
        }

        void OnGiveItem(Inventory _inventory, ItemIndex _index, float _count)
        {
            // Check for valid call
            if (_inventory == null || _index == ItemIndex.None || _count <= 0.0f)
            {
                return;
            }

            // Ignore Appraiser's Eye
            if (appraisersEye != null && _index == appraisersEye.itemDef.itemIndex)
            {
                return;
            }

            // Attempt to get Character Body
            CharacterBody body = Utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Attempt to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(body);
            if (helper == null)
            {
                return;
            }

            // Get item count
            int count = _inventory.GetItemCountEffective(collectorsVisionItem.itemDef);
            if (count > 0.0f)
            {
                // Get Collector's Vision index
                ItemIndex collectorsIndex = ItemCatalog.FindItemIndex("FAITHFUL_COLLECTORS_VISION_NAME");

                // Ensure it's not Collector's Vision
                if (_index == collectorsIndex)
                {
                    return;
                }

                // Check flag to ensure item is a first pickup for the stage
                if (helper.stageFlags.Get($"CS_{_index}_FFS"))
                {
                    return;
                }

                // Set flag
                helper.stageFlags.Set($"CS_{_index}_FFS");

                // Get current amount of buffs
                int currentBuffs = body.GetBuffCount(inspirationBuff.buffDef);

                // Check if should cap inspiration
                if (capInspiration)
                {
                    // Check if already at 100% inspiration
                    if (currentBuffs >= 100)
                    {
                        // Check if has given appraiser's eye this stage
                        if (!helper.stageFlags.Get("CS_APPRAISERS_EYE_GIVEN"))
                        {
                            // Grant appraiser's eye
                            GrantEye(_inventory, _index);

                            // Set flag to prevent giving multiple appraiser's eyes in one stage
                            helper.stageFlags.Set("CS_APPRAISERS_EYE_GIVEN");
                        }
                    }

                    // Calculate buffs to add
                    int buffsToAdd = Mathf.Min(inspirationGain + inspirationGainStacking * (count - 1), 100 - currentBuffs);

                    // Grant buffs
                    for (int i = 0; i < buffsToAdd; i++)
                    {
                        body.AddBuff(inspirationBuff.buffDef);
                    }
                }

                // Inspiration is not capped
                else
                {
                    // Grant buffs
                    for (int i = 0; i < inspirationGain + inspirationGainStacking * (count - 1); i++)
                    {
                        body.AddBuff(inspirationBuff.buffDef);
                    }

                    // Check if is over 100% inspiration
                    if (body.GetBuffCount(inspirationBuff.buffDef) > 100)
                    {
                        // Check if has given appraiser's eye this stage
                        if (!helper.stageFlags.Get("CS_APPRAISERS_EYE_GIVEN"))
                        {
                            // Grant appraiser's eye
                            GrantEye(_inventory, _index);

                            // Set flag to prevent giving multiple appraiser's eyes in one stage
                            helper.stageFlags.Set("CS_APPRAISERS_EYE_GIVEN");
                        }
                    }
                }
            }
        }

        private void OnPushPickupNotification(On.RoR2.CharacterMasterNotificationQueue.orig_PushPickupNotification_CharacterMaster_PickupIndex_bool_int orig, CharacterMaster characterMaster, PickupIndex pickupIndex, bool isTemporary, int upgradeCount)
        {
            // Perform original method functionality
            orig(characterMaster, pickupIndex, isTemporary, upgradeCount);

            // Process pending eye notifications
            ProcessPendingEyeNotifications(characterMaster, pickupIndex);
        }

        private void GrantEye(Inventory _inventory, ItemIndex _grantingItemIndex)
        {
            // Grant appraiser's eye
            _inventory.GiveItemPermanent(appraisersEye.itemDef);

            // Try get character master
            CharacterMaster master = _inventory.gameObject.GetComponent<CharacterMaster>();
            if (master == null) return;

            // Add to pending notifications to give notification after the original item has already given its notification
            pendingEyeNotifications.Add(new PendingEyeNotification
            {
                master = master,
                grantingPickupIndex = PickupCatalog.FindPickupIndex(_grantingItemIndex),
                remainingChecks = 64    // For avoiding keeping pending notifications around for too long if something goes wrong with the notification processing
            });
        }

        private void ProcessPendingEyeNotifications(CharacterMaster _master, PickupIndex _pushedPickupIndex)
        {
            // Cycle through pending notifications
            for (int i = pendingEyeNotifications.Count - 1; i >= 0; i--)
            {
                PendingEyeNotification notification = pendingEyeNotifications[i];

                // Check if master is still valid
                if (notification.master == null)
                {
                    pendingEyeNotifications.RemoveAt(i);
                    continue;
                }

                // Check if this is the pickup notification that granted the eye for the pending notification
                if (_master == notification.master && _pushedPickupIndex == notification.grantingPickupIndex)
                {
                    pendingEyeNotifications.RemoveAt(i);

                    // Send eye notification
                    CharacterMasterNotificationQueue.PushItemNotification(_master, appraisersEye.itemDef.itemIndex);
                    continue;
                }

                // Decrement remaining checks
                notification.remainingChecks--;

                // Check if should remove pending notification due to too many checks
                if (notification.remainingChecks <= 0)
                {
                    pendingEyeNotifications.RemoveAt(i);
                }
            }
        }

        private void OnSceneExit(SceneExitController _exitController)
        {
            // Cycle through players
            foreach (PlayerCharacterMasterController player in Utils.GetPlayers())
            {
                // Try get character body
                CharacterBody body = player.master?.GetBody();
                if (body == null) continue;

                // Get buff count
                int buffCount = body.GetBuffCount(inspirationBuff.buffDef);

                // Check if has buff
                if (buffCount > 0)
                {
                    // Create lookup string
                    string lookupString = $"{player.GetComponent<NetworkIdentity>().netId} IC";

                    // Calculate carryover for stage
                    int newCarryover = Mathf.CeilToInt(buffCount * inspirationReturnPerc);

                    // Get already existing carryover for this player
                    int currentCarryover = LookupTable.GetInt(lookupString);

                    // Check if new carryover is greater than the current carryover
                    if (newCarryover > currentCarryover)
                    {
                        // Use lookup table to cache new carryover stacks of inspiration (sync with all clients)
                        Utils.netUtils.CmdSetLookupInt(lookupString, newCarryover);
                    }
                }
            }
        }
    }
}
