using EntityStates;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class CopperGear : ItemBase
    {
        // Store buffs
        Buff copperGearBuff;
        Buff copperGearEffectBuff;

        // Quality buffs
        Buff copperGearQualityBuff;
        Buff copperGearQualityUncommonBuff;
        Buff copperGearQualityRareBuff;
        Buff copperGearQualityEpicBuff;
        Buff copperGearQualityLegendaryBuff;

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

        // Store additional quality settings
        QualitySetting<float> attackSpeedQualitySetting;
        QualitySetting<float> durationQualitySetting;
        QualitySetting<float> durationStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> attackSpeedQualityValues = new();
        QualityValues<float> durationQualityValues = new();
        QualityValues<float> durationStackingQualityValues = new();

        // Constructor
        public CopperGear(Toolbox _toolbox) : base(_toolbox, "COPPER_GEAR")
        {
            // Create display settings
            CreateDisplaySettings("coppergeardisplaymesh");

            // Create Copper Gear item and buff
            MainItem = Items.AddItem(token, "Copper Gear", [ItemTag.Damage, ItemTag.Technology, ItemTag.HoldoutZoneRelated], "texcoppergearicon", "coppergearmesh", _simulacrumBanned: true, _supportsQuality: true, _displaySettings: displaySettings);
            copperGearBuff = Buffs.AddBuff("COPPER_GEAR", "Copper Gear", "texbuffteleportergear", Color.white, false);
            copperGearEffectBuff = Buffs.AddBuff("COPPER_GEAR_EFFECT", "Copper Gear", "texbuffteleportergear", Color.white, _isHidden: true, _hasConfig: false, _langTokenOverride: "COPPER_GEAR");

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(copperGearEffectBuff, CopperGearStatsMod);

            // Link Holdout Zone behaviour
            Behaviour.AddInHoldoutZoneCallback(InHoldoutZone);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            copperGearQualityBuff = Buffs.AddBuff("COPPER_GEAR_QUALITY", "Copper Kill", "texBuffDeathGear", Color.white, _qualityBuff: true);
            copperGearQualityUncommonBuff = Buffs.AddBuff("COPPER_GEAR_QUALITY_UNCOMMON", "Copper Kill", "texBuffDeathGear", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "COPPER_GEAR_QUALITY");
            copperGearQualityRareBuff = Buffs.AddBuff("COPPER_GEAR_QUALITY_RARE", "Copper Kill", "texBuffDeathGear", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "COPPER_GEAR_QUALITY");
            copperGearQualityEpicBuff = Buffs.AddBuff("COPPER_GEAR_QUALITY_EPIC", "Copper Kill", "texBuffDeathGear", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "COPPER_GEAR_QUALITY");
            copperGearQualityLegendaryBuff = Buffs.AddBuff("COPPER_GEAR_QUALITY_LEGENDARY", "Copper Kill", "texBuffDeathGear", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "COPPER_GEAR_QUALITY");

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath_Quality);

            // Add stats mods for buffs
            Behaviour.AddStatsMod(copperGearQualityUncommonBuff, UncommonStatsMod_Quality);
            Behaviour.AddStatsMod(copperGearQualityRareBuff, RareStatsMod_Quality);
            Behaviour.AddStatsMod(copperGearQualityEpicBuff, EpicStatsMod_Quality);
            Behaviour.AddStatsMod(copperGearQualityLegendaryBuff, LegendaryStatsMod_Quality);
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
            displaySettings.AddCharacterDisplay("Walker Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
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
            displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(0F, 0.505F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Operator", "BackpackKnobUp", new Vector3(0F, 0F, 0.005F), new Vector3(90F, 0F, 0F), new Vector3(0.03F, 0.03F, 0.03F));
            displaySettings.AddCharacterDisplay("Operator", "BackpackKnobDown", new Vector3(0F, 0F, 0.005F), new Vector3(15F, 90F, 270F), new Vector3(0.03F, 0.03F, 0.03F));
            displaySettings.AddCharacterDisplay("Operator", "LowerArmR", new Vector3(0F, 0F, 0F), new Vector3(0F, 230F, 0F), new Vector3(0.15F, 0.075F, 0.15F));
            displaySettings.AddCharacterDisplay("Drifter", "BagFrontPocket", new Vector3(-0.1F, 0.5125F, 0F), new Vector3(0F, 0F, 352.5F), new Vector3(0.075F, 0.075F, 0.075F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            attackSpeedSetting = MainItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 25.0f, "How much should this item increase attack speed while within the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            attackSpeedStackingSetting = MainItem.CreateSetting("ATTACK_SPEED_STACKING", "Attack Speed Stacking", 25.0f, "How much should further stacks of this item increase attack speed while within the teleporter radius? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            buffDurationSetting = MainItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the teleporter radius? (1.0 = 1 second)", _minValue: 0.1f, _canRandomise: false, _valueFormatting: "{0:0.00}s");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            attackSpeedQualitySetting = MainItem.CreateQualitySetting("ATTACK_SPEED", "Attack Speed", 5.0f, 10.0f, 20.0f, 30.0f, "How much should each kill increase attack speed while within the teleporter radius? (5.0 = 5% increase)", _valueFormatting: "{0:0.0}%");
            durationQualitySetting = MainItem.CreateQualitySetting("DURATION", "Buff Duration", 2.5f, 5.0f, 10.0f, 15.0f, "How long should the attack speed buff last after each kill while within the teleporter radius? (2.5 = 2.5 seconds)", _valueFormatting: "{0:0.0}s");
            durationStackingQualitySetting = MainItem.CreateQualitySetting("DURATION_STACKING", "Buff Duration Stacking", 2.5f, 5.0f, 10.0f, 15.0f, "How much longer should further stacks of this item make the attack speed buff last after each kill while within the teleporter radius? (2.5 = 2.5 seconds)", _valueFormatting: "{0:0.0}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            attackSpeedStacking = attackSpeedStackingSetting.Value / 100.0f;
            buffDuration = buffDurationSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            attackSpeedQualityValues.UpdateValues(attackSpeedQualitySetting, 0.01f);
            durationQualityValues.UpdateValues(durationQualitySetting);
            durationStackingQualityValues.UpdateValues(durationStackingQualitySetting);
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
                int copperGearCount = inventory.GetItemCount(MainItem.itemDef);

                // Has Copper Gears?
                if (copperGearCount > 0)
                {
                    // Refresh Copper Gear buffs
                    Utils.RefreshTimedBuffs(_body, copperGearEffectBuff.buffDef, buffDuration);

                    // Get needed amount of buffs
                    int needed = copperGearCount - _body.GetBuffCount(copperGearEffectBuff.buffDef);

                    // Check if there are too many buffs (can happen if the player loses items while in the zone)
                    if (needed < 0)
                    {
                        // Remove excess buffs
                        for (int i = 0; i < -needed; i++)
                        {
                            // Remove Copper Gear buff
                            _body.RemoveOldestTimedBuff(copperGearEffectBuff.buffDef);
                        }
                    }

                    // Either has enough buffs or needs more
                    else
                    {
                        // Catch up buff count
                        for (int i = 0; i < needed; i++)
                        {
                            // Add Copper Gear buff
                            _body.AddTimedBuff(copperGearEffectBuff.buffDef, buffDuration);
                        }
                    }
                }
            }
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            if (characterBody)
            {
                // Update visual buff
                characterBody.SetBuffCount(copperGearBuff.buffDef.buffIndex, characterBody.GetBuffCount(copperGearEffectBuff.buffDef.buffIndex) > 0 ? 1 : 0);
            }
        }

        private void UncommonStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeedQualityValues.UNCOMMON * _count;
        }

        private void RareStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeedQualityValues.RARE * _count;
        }

        private void EpicStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeedQualityValues.EPIC * _count;
        }

        private void LegendaryStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeedQualityValues.LEGENDARY * _count;
        }

        private void OnCharacterDeath_Quality(DamageReport _report)
        {
            // Attempt to fetch attacker inventory and body
            if (_report == null) return;
            CharacterMaster master = _report.attackerMaster;
            if (master == null) return;
            Inventory inventory = master.inventory;
            if (inventory == null) return;
            CharacterBody body = master.GetBody();
            if (body == null) return;

            // Check if character is in a holdout zone
            if (Utils.GetHoldoutZonesContainingCharacter(master).Count <= 0) return;

            // Get item counts
            QualityCounts counts = QualityCompat.GetItemCountsEffective(inventory, MainItem);

            // Effective quality should be highest quality the attacker possesses
            Quality effectiveQuality = Quality.UNCOMMON;
            if (counts.LEGENDARY > 0) effectiveQuality = Quality.LEGENDARY;
            else if (counts.EPIC > 0) effectiveQuality = Quality.EPIC;
            else if (counts.RARE > 0) effectiveQuality = Quality.RARE;
            else if (counts.UNCOMMON <= 0) return;

            // Calculate buff duration
            float buffDuration = counts.UNCOMMON == 0 ? 0.0f : durationQualityValues.UNCOMMON + (counts.UNCOMMON - 1) * durationStackingQualityValues.UNCOMMON;
            buffDuration += counts.RARE == 0 ? 0.0f : durationQualityValues.RARE + (counts.RARE - 1) * durationStackingQualityValues.RARE;
            buffDuration += counts.EPIC == 0 ? 0.0f : durationQualityValues.EPIC + (counts.EPIC - 1) * durationStackingQualityValues.EPIC;
            buffDuration += counts.LEGENDARY == 0 ? 0.0f : durationQualityValues.LEGENDARY + (counts.LEGENDARY - 1) * durationStackingQualityValues.LEGENDARY;

            // Give buff
            switch (effectiveQuality)
            {
                case Quality.UNCOMMON:
                    body.AddTimedBuff(copperGearQualityUncommonBuff.buffDef, buffDuration);
                    break;

                case Quality.RARE:
                    body.AddTimedBuff(copperGearQualityRareBuff.buffDef, buffDuration);
                    break;

                case Quality.EPIC:
                    body.AddTimedBuff(copperGearQualityEpicBuff.buffDef, buffDuration);
                    break;

                case Quality.LEGENDARY:
                    body.AddTimedBuff(copperGearQualityLegendaryBuff.buffDef, buffDuration);
                    break;
            }

            // Give visual buff
            body.AddTimedBuff(copperGearQualityBuff.buffDef, buffDuration);
        }
    }
}
