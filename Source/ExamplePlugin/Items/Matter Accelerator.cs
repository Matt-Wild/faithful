using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class MatterAccelerator : ItemBase
    {
        // Store buffs
        Buff matterAcceleratorBuff;
        Buff matterAcceleratorSpeedBuff;
        Buff matterAcceleratorHealthBuff;

        // Quality buffs
        Buff matterAcceleratorQualityBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> shieldGainSetting;
        Setting<float> speedSetting;
        Setting<float> speedStackingSetting;

        // Store item stats
        float shieldGain;
        float speed;
        float speedStacking;

        // Store additional quality settings
        QualitySetting<float> healthPercQualitySetting;
        QualitySetting<float> speedQualitySetting;
        QualitySetting<float> speedStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> healthPercQualityValues = new();
        QualityValues<float> speedQualityValues = new();
        QualityValues<float> speedStackingQualityValues = new();

        // Constructor
        public MatterAccelerator(Toolbox _toolbox) : base(_toolbox, "MATTER_ACCELERATOR")
        {
            // Create display settings
            CreateDisplaySettings("MatterAcceleratorDisplayMesh");

            // Create Second Hand item and buff
            mainItem = Items.AddItem(token, "Matter Accelerator", [ItemTag.Utility, ItemTag.Healing, ItemTag.Technology, ItemTag.MobilityRelated], "texMatterAcceleratorIcon", "MatterAcceleratorMesh", _tier: ItemTier.Tier1, _supportsQuality: true, _displaySettings: displaySettings);
            matterAcceleratorBuff = Buffs.AddBuff("MATTER_ACCELERATOR", "Matter Accelerator", "texMatterAcceleratorBuff", Color.white, false);
            matterAcceleratorSpeedBuff = Buffs.AddBuff("MATTER_ACCELERATOR_SPEED", "Matter Accelerator Speed", "texMatterAcceleratorBuff", Color.white, _isHidden: true, _hasConfig: false, _langTokenOverride: "MATTER_ACCELERATOR");
            matterAcceleratorHealthBuff = Buffs.AddBuff("MATTER_ACCELERATOR_HEALTH", "Matter Accelerator Health", "texMatterAcceleratorBuff", Color.white, _isHidden: true, _hasConfig: false, _langTokenOverride: "MATTER_ACCELERATOR");

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(matterAcceleratorSpeedBuff, MatterAcceleratorBuffStatsMod);
            Behaviour.AddStatsMod(matterAcceleratorHealthBuff, MatterAcceleratorHealthBuffStatsMod);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            matterAcceleratorQualityBuff = Buffs.AddBuff("MATTER_ACCELERATOR_QUALITY", "Matter Accelerator", "texBuffDeathGear", Color.white, _qualityBuff: true, _isHidden: true, _hasConfig: false, _langTokenOverride: "MATTER_ACCELERATOR");

            // Add stats mods for buffs
            Behaviour.AddStatsMod(matterAcceleratorQualityBuff, StatsMod_Quality);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate_Quality);
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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmR", new Vector3(-0.01F, 0.1725F, 0F), new Vector3(87.5F, 90F, 90F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Huntress", "UpperArmR", new Vector3(0.0185F, 0.22F, 0.005F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Bandit", "CalfL", new Vector3(0F, 0.275F, 0.0125F), new Vector3(87.5F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("MUL-T", "LowerArmL", new Vector3(0.2625F, 1.9F, 0F), new Vector3(270F, 0F, 0F), new Vector3(2.75F, 2.75F, 2.75F));
            displaySettings.AddCharacterDisplay("Engineer", "LowerArmR", new Vector3(-0.005F, 0.145F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.575F, 1.5625F), new Vector3(0F, 0F, 0F), new Vector3(0.6F, 0.6F, 0.6F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.785F, 0.395F), new Vector3(0F, 0F, 0F), new Vector3(0.9F, 0.9F, 1F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfL", new Vector3(-0.0075F, 0.16125F, 0.05F), new Vector3(270F, 0F, 0F), new Vector3(0.2125F, 0.2125F, 0.2125F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighR", new Vector3(-0.0115F, 0.3275F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("REX", "CalfBackR", new Vector3(0F, 0.475F, -0.0325F), new Vector3(90F, 0F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Loader", "LowerArmR", new Vector3(-0.005F, 0.15F, 0.0025F), new Vector3(85F, 180F, 180F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Acrid", "UpperArmR", new Vector3(0.15F, 3.1F, 0F), new Vector3(90F, 0F, 0F), new Vector3(3F, 3F, 3F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(-0.00375F, 0.134F, 0.0035F), new Vector3(90F, 0F, 0F), new Vector3(0.1275F, 0.1275F, 0.1275F));
            displaySettings.AddCharacterDisplay("Railgunner", "MuzzleSniper", new Vector3(0F, 0F, -0.0635F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.06F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ThighL", new Vector3(0.01F, 0.26675F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.275F, 0.275F, 0.225F));
            displaySettings.AddCharacterDisplay("Seeker", "CalfL", new Vector3(-0.02F, 0.1365F, -0.015F), new Vector3(86.25F, 90F, 90F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.01115F, 0.288F, 0F), new Vector3(271.25F, 63.25F, 63.250F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Chef", "Pelvis", new Vector3(0.06875F, 0F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.375F, 0.375F, 0.175F));
            displaySettings.AddCharacterDisplay("Operator", "UpperArmL", new Vector3(-0.136F, -0.0065F, -0.009F), new Vector3(10F, 262.5F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Drifter", "LowerArmR", new Vector3(-0.175F, 0F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.225F, 0.225F, 0.15F));
            displaySettings.AddCharacterDisplay("Best Buddy", "CableR", new Vector3(0.0075F, 0.00025F, 0.0075F), new Vector3(315F, 307.5F, 35F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Scavenger", "LowerArmR", new Vector3(0F, 1.375F, 0F), new Vector3(90F, 0F, 0F), new Vector3(1.75F, 1.75F, 1.75F));
            displaySettings.AddCharacterDisplay("Technician", "LowerArmL", new Vector3(0F, 0.225F, 0F), new Vector3(275F, 90F, 270F), new Vector3(0.15F, 0.15F, 0.125F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            shieldGainSetting = mainItem.CreateSetting("SHIELD_GAIN", "Shield Gain", 5.0f, "How much shield should this item provide? (5.0 = 5% shield)", _valueFormatting: "{0:0.0}%");
            speedSetting = mainItem.CreateSetting("SPEED", "Movement Speed", 20.0f, "How much should this item increase movement speed while having shield or barrier? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingSetting = mainItem.CreateSetting("SPEED_STACKING", "Movement Speed Stacking", 20.0f, "How much should further stacks of this item increase movement speed while having shield or barrier? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (mainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            healthPercQualitySetting = mainItem.CreateQualitySetting("HEALTH_PERC", "Health Percentage", 1.0f, 1.0f, 1.0f, 1.0f, "How much shield is required to receive one instance of the buff? (1.0 = 1% max health)", _valueFormatting: "{0:0.0}%");
            speedQualitySetting = mainItem.CreateQualitySetting("SPEED", "Movement Speed", 1.0f, 2.0f, 3.0f, 5.0f, "How much should the buff increase movement speed? (1.0 = 1% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingQualitySetting = mainItem.CreateQualitySetting("SPEED_STACKING", "Movement Speed Stacking", 1.0f, 2.0f, 3.0f, 5.0f, "How much should further stacks of the buff increase movement speed? (1.0 = 1% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            shieldGain = shieldGainSetting.Value / 100.0f;
            speed = speedSetting.Value / 100.0f;
            speedStacking = speedStackingSetting.Value / 100.0f;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (mainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            mainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            healthPercQualityValues.UpdateValues(healthPercQualitySetting, 0.01f);
            speedQualityValues.UpdateValues(speedQualitySetting);
            speedStackingQualityValues.UpdateValues(speedStackingQualitySetting);
        }

        void MatterAcceleratorHealthBuffStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify shield
            _stats.baseShieldAdd += _count;
        }

        void MatterAcceleratorBuffStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += _count > 1 ? speed + (speedStacking * (_count - 1)) : speed;
        }

        private void StatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += _count * 0.01f;
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (!characterBody || !inventory) return;

            // Check for health components
            HealthComponent healthComponent = characterBody.healthComponent;
            if (!healthComponent) return;

            // Get item count
            int itemCount = inventory.GetItemCount(mainItem.itemDef);
            if (itemCount <= 0)
            {
                // Ensure no buffs
                characterBody.SetBuffCount(matterAcceleratorBuff.buffDef.buffIndex, 0);
                characterBody.SetBuffCount(matterAcceleratorSpeedBuff.buffDef.buffIndex, 0);
                characterBody.SetBuffCount(matterAcceleratorHealthBuff.buffDef.buffIndex, 0);
                return;
            }

            // Get needed amount of speed buffs
            int neededSpeed = healthComponent.shield > 0.0f || healthComponent.barrier > 0.0f ? itemCount : 0;

            // Update visual buff
            characterBody.SetBuffCount(matterAcceleratorBuff.buffDef.buffIndex, neededSpeed > 0 ? 1 : 0);

            // Update speed buff based on shield or barrier
            characterBody.SetBuffCount(matterAcceleratorSpeedBuff.buffDef.buffIndex, neededSpeed);

            // Update health buff based on max health
            characterBody.SetBuffCount(matterAcceleratorHealthBuff.buffDef.buffIndex, Mathf.CeilToInt(healthComponent.fullHealth * shieldGain));
        }

        void GenericCharacterFixedUpdate_Quality(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (!characterBody || !inventory) return;

            // Check for health components
            HealthComponent healthComponent = characterBody.healthComponent;
            if (!healthComponent) return;

            // Get item counts
            QualityCounts counts = QualityCompat.GetItemCountsEffective(inventory, mainItem);

            // Check for any valid item
            if (counts.Total == 0)
            {
                characterBody.SetBuffCount(matterAcceleratorQualityBuff.buffDef.buffIndex, 0);
                return;
            }

            // Get percentage of max health that is shield
            float shieldPerc = healthComponent.shield / (characterBody.baseMaxHealth + characterBody.levelMaxHealth * (characterBody.level - 1));

            // 0 shield shortcut
            if (shieldPerc == 0.0f)
            {
                characterBody.SetBuffCount(matterAcceleratorQualityBuff.buffDef.buffIndex, 0);
                return;
            }

            // Total up needed extra speed
            float neededSpeed = 0.0f;

            float uncommonBonus = shieldPerc / healthPercQualityValues.UNCOMMON;
            float rareBonus = shieldPerc / healthPercQualityValues.RARE;
            float epicBonus = shieldPerc / healthPercQualityValues.EPIC;
            float legendaryBonus = shieldPerc / healthPercQualityValues.LEGENDARY;

            neededSpeed += counts.UNCOMMON == 0 ? 0.0f : (speedQualityValues.UNCOMMON + (counts.UNCOMMON - 1) * speedStackingQualityValues.UNCOMMON) * uncommonBonus;
            neededSpeed += counts.RARE == 0 ? 0.0f : (speedQualityValues.RARE + (counts.RARE - 1) * speedStackingQualityValues.RARE) * rareBonus;
            neededSpeed += counts.EPIC == 0 ? 0.0f : (speedQualityValues.EPIC + (counts.EPIC - 1) * speedStackingQualityValues.EPIC) * epicBonus;
            neededSpeed += counts.LEGENDARY == 0 ? 0.0f : (speedQualityValues.LEGENDARY + (counts.LEGENDARY - 1) * speedStackingQualityValues.LEGENDARY) * legendaryBonus;

            // Update speed bonus from quality
            characterBody.SetBuffCount(matterAcceleratorQualityBuff.buffDef.buffIndex, Mathf.FloorToInt(neededSpeed));
        }
    }
}
