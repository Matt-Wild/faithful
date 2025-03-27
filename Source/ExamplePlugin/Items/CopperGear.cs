using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class CopperGear : ItemBase
    {
        // Store item and buff
        Item copperGearItem;
        Buff copperGearBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> attackSpeedSetting;
        Setting<float> attackSpeedStackingSetting;
        Setting<float> buffDurationSetting;

        // Store item stats
        float attackSpeed;
        float attackSpeedStacking;
        float buffDuration;

        // Constructor
        public CopperGear(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("coppergeardisplaymesh");

            // Create Copper Gear item and buff
            copperGearItem = Items.AddItem("COPPER_GEAR", [ItemTag.Damage, ItemTag.HoldoutZoneRelated], "texcoppergearicon", "coppergearmesh", _simulacrumBanned: true, _displaySettings: displaySettings);
            copperGearBuff = Buffs.AddBuff("COPPER_GEAR", "texbuffteleportergear", Color.white);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(copperGearBuff, CopperGearStatsMod);

            // Link Holdout Zone behaviour
            Behaviour.AddInHoldoutZoneCallback(InHoldoutZone);
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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898F, 0.26776F, 0.00182F), new Vector3(7.69423F, 1.2381F, 2.28152F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Huntress", "Muzzle", new Vector3(0F, -0.02925F, -0.02537F), new Vector3(75F, 270F, 90F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(0F, 0.1F, 0F), new Vector3(355F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.39384F, 2.95909F, -1.01878F), new Vector3(0F, 270F, 35F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.141F, 0.382F, 0.1435F), new Vector3(45F, 135F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.00525F, 0.08556F, 0.03226F), new Vector3(10F, 0F, 355F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(0.0085F, 0.152F, -0.0075F), new Vector3(0F, 18.25F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0F, -0.034F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(-0.0025F, 0.64F, -0.0055F), new Vector3(356F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(0F, 3.725F, 0F), new Vector3(355F, 180F, 0F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0F, 3.725F, 0F), new Vector3(359.765F, 267.319F, 355.005F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(-0.00353F, -0.00525F, -0.06F), new Vector3(0F, 90F, 90F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(-0.075F, -0.1475F, 0.2855F), new Vector3(0F, 90F, 270F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(-0.0025F, 0.385F, 0.0025F), new Vector3(4.5F, 0F, 350F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(5.13896F, 3.45994F, 0.08489F), new Vector3(338.495F, 358.5428F, 334.8337F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.3275F, -0.075F), new Vector3(0F, 315F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.01725F, 0.2145F, 0.0045F), new Vector3(358.75F, 0.0375F, 358.95F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.02F, 0.36575F, 0.0011F), new Vector3(1.61F, 149.31F, 0.265F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, 0.12975F, 0F), new Vector3(0F, 2.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, -0.12975F, 0F), new Vector3(0F, 122.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, -0.12975F, 0F), new Vector3(0F, 122.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            attackSpeedSetting = copperGearItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 25.0f, "How much should this item increase attack speed while within the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            attackSpeedStackingSetting = copperGearItem.CreateSetting("ATTACK_SPEED_STACKING", "Attack Speed Stacking", 25.0f, "How much should further stacks of this item increase attack speed while within the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            buffDurationSetting = copperGearItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the teleporter radius? (1.0 = 1 second)", _minValue: 0.1f, _canRandomise: false, _valueFormatting: "{0:0.00}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            attackSpeedStacking = attackSpeedStackingSetting.Value / 100.0f;
            buffDuration = buffDurationSetting.Value;

            // Update item texts with new settings
            copperGearItem.UpdateItemTexts();
        }

        void CopperGearStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for buff
            if (_count == 0) return;

            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeed + attackSpeedStacking * (_count - 1);
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory)
            {
                // Get Copper Gear amount
                int copperGearCount = inventory.GetItemCount(copperGearItem.itemDef);

                // Has Copper Gears?
                if (copperGearCount > 0)
                {
                    // Refresh Copper Gear buffs
                    Utils.RefreshTimedBuffs(_body, copperGearBuff.buffDef, buffDuration);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(copperGearBuff.buffDef);

                    // Catch up buff count
                    for (int i = 0; i < needed; i++)
                    {
                        // Add Copper Gear buff
                        _body.AddTimedBuff(copperGearBuff.buffDef, buffDuration);
                    }
                }
            }
        }
    }
}
