using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class MatterAccelerator : ItemBase
    {
        // Store item and buff
        Item matterAcceleratorItem;
        Buff matterAcceleratorBuff;
        Buff matterAcceleratorHealthBuff;

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

        // Constructor
        public MatterAccelerator(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("MatterAcceleratorDisplayMesh");

            // Create Second Hand item and buff
            matterAcceleratorItem = Items.AddItem("MATTER_ACCELERATOR", "Matter Accelerator", [ItemTag.Utility, ItemTag.Healing, ItemTag.Technology, ItemTag.MobilityRelated], "texMatterAcceleratorIcon", "MatterAcceleratorMesh", _tier: ItemTier.Tier1, _displaySettings: displaySettings);
            matterAcceleratorBuff = Buffs.AddBuff("MATTER_ACCELERATOR", "Matter Accelerator", "texMatterAcceleratorIcon", Color.white, _isHidden: true, _hasConfig: false);
            matterAcceleratorHealthBuff = Buffs.AddBuff("MATTER_ACCELERATOR_HEALTH", "Matter Accelerator Health", "texMatterAcceleratorIcon", Color.white, _isHidden: true, _hasConfig: false);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(matterAcceleratorBuff, MatterAcceleratorBuffStatsMod);
            Behaviour.AddStatsMod(matterAcceleratorHealthBuff, MatterAcceleratorHealthBuffStatsMod);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
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
            displaySettings.AddCharacterDisplay("Commando", "ThighL", new Vector3(0.1125F, 0.1125F, 0.04F), new Vector3(270F, 257.5F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.1075F, 0.015F, 0.13F), new Vector3(80F, 210F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Bandit", "CalfR", new Vector3(0.1025F, 0.066F, 0.03F), new Vector3(72.5F, 55F, 336.75F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("MUL-T", "CalfL", new Vector3(0F, 0.905F, 0.815F), new Vector3(87.50002F, 180F, 180F), new Vector3(1.25F, 1.25F, 1.25F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.005F, 0.4575F, 0F), new Vector3(0F, 90F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(-0.005F, 0.4575F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
            displaySettings.AddCharacterDisplay("Walker Turret", "LegBar3", new Vector3(0F, 0.725F, 0.2F), new Vector3(0F, 270F, 255F), new Vector3(0.425F, 0.425F, 0.425F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.115F, 0.3325F, -0.1775F), new Vector3(0F, 87.5F, 7.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.0675F, 0.22F, -0.111F), new Vector3(277.5F, 150F, 180F), new Vector3(0.15F, 0.175F, 0.15F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.3275F, 0.8275F, 0.0315F), new Vector3(0F, 20F, 52.5F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Loader", "MechHandR", new Vector3(0F, 0.24F, 0F), new Vector3(0F, 175F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Jaw", new Vector3(0F, 2F, 0.2F), new Vector3(85F, 0F, 0F), new Vector3(1.25F, 1.25F, 1.25F));
            displaySettings.AddCharacterDisplay("Captain", "ThighL", new Vector3(0.0225F, 0.305F, -0.11F), new Vector3(357.5F, 77.75F, 268.75F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Railgunner", "Battery1R", new Vector3(0F, 0.225F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "UpperArmR", new Vector3(0.1825F, -0.06F, 0.1575F), new Vector3(77.5F, 307.5F, 266.25F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.268F, 0.179F, -0.226F), new Vector3(357F, 357F, 46.75F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("False Son", "UpperArmL", new Vector3(-0.123F, 0.257F, -0.0675F), new Vector3(277.5F, 65.75F, 349.5F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.0236F, -0.036F, -0.511F), new Vector3(314F, 234.25F, 177.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Operator", "ClawSpin", new Vector3(0F, 0F, 0.032F), new Vector3(0F, 270F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Drifter", "BagBottom", new Vector3(-0.19F, 0.128F, 0F), new Vector3(0F, 2.5F, 70F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Scavenger", "Backpack", new Vector3(-7.855F, 5.63F, -1.275F), new Vector3(0F, 340F, 84F), new Vector3(2F, 2F, 2F));
            displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(-0.09F, 0.025F, 0F), new Vector3(0F, 180F, 270F), new Vector3(0.1125F, 0.1125F, 0.1125F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            shieldGainSetting = matterAcceleratorItem.CreateSetting("SHIELD_GAIN", "Shield Gain", 5.0f, "How much shield should this item provide? (5.0 = 5% shield)", _valueFormatting: "{0:0.0}%");
            speedSetting = matterAcceleratorItem.CreateSetting("SPEED", "Movement Speed", 20.0f, "How much should this item increase movement speed while having shield or barrier? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingSetting = matterAcceleratorItem.CreateSetting("SPEED_STACKING", "Movement Speed Stacking", 20.0f, "How much should further stacks of this item increase movement speed while having shield or barrier? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            shieldGain = shieldGainSetting.Value / 100.0f;
            speed = speedSetting.Value / 100.0f;
            speedStacking = speedStackingSetting.Value / 100.0f;

            // Update item texts with new settings
            matterAcceleratorItem.UpdateItemTexts();
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
            int itemCount = inventory.GetItemCount(matterAcceleratorItem.itemDef);
            if (itemCount <= 0)
            {
                // Ensure no buffs
                if (characterBody.GetBuffCount(matterAcceleratorBuff.buffDef.buffIndex) > 0) characterBody.SetBuffCount(matterAcceleratorBuff.buffDef.buffIndex, 0);
                if (characterBody.GetBuffCount(matterAcceleratorHealthBuff.buffDef.buffIndex) > 0) characterBody.SetBuffCount(matterAcceleratorHealthBuff.buffDef.buffIndex, 0);
                return;
            }

            // Update speed buff based on shield or barrier
            characterBody.SetBuffCount(matterAcceleratorBuff.buffDef.buffIndex, healthComponent.shield > 0.0f || healthComponent.barrier > 0.0f ? itemCount : 0);

            // Update health buff based on max health
            characterBody.SetBuffCount(matterAcceleratorHealthBuff.buffDef.buffIndex, Mathf.CeilToInt(healthComponent.fullHealth * shieldGain));
        }
    }
}
