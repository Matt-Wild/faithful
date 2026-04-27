using EntityStates;
using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class MeltingWarbler : ItemBase
    {
        // Quality buffs
        Buff meltingWarblerQualityBuff;
        Buff meltingWarblerQualityUncommonBuff;
        Buff meltingWarblerQualityRareBuff;
        Buff meltingWarblerQualityEpicBuff;
        Buff meltingWarblerQualityLegendaryBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> jumpBoostSetting;
        Setting<float> jumpBoostStackingSetting;

        // Store item stats
        float jumpBoost;
        float jumpBoostStacking;

        // Store additional quality settings
        QualitySetting<float> speedQualitySetting;
        QualitySetting<float> speedStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> speedQualityValues = new();
        QualityValues<float> speedStackingQualityValues = new();

        // Constructor
        public MeltingWarbler(Toolbox _toolbox) : base(_toolbox, "MELTING_WARBLER")
        {
            // Create display settings
            CreateDisplaySettings("meltingwarblerdisplaymesh");

            // Create Melting Warbler item
            MainItem = Items.AddItem(token, "Melting Warbler", [ItemTag.Utility, ItemTag.MobilityRelated], "texmeltingwarblericon", "meltingwarblermesh", ItemTier.VoidTier2, _corruptToken: "ITEM_JUMPBOOST_NAME", _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(MainItem, MeltingWarblerStatsMod);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            meltingWarblerQualityBuff = Buffs.AddBuff("MELTING_WARBLER_QUALITY", "Melting Warbler", "texMeltingWarblerBuff", Color.white, _qualityBuff: true, _canStack: false);
            meltingWarblerQualityUncommonBuff = Buffs.AddBuff("MELTING_WARBLER_QUALITY_UNCOMMON", "Melting Warbler", "texMeltingWarblerBuff", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "MELTING_WARBLER_QUALITY");
            meltingWarblerQualityRareBuff = Buffs.AddBuff("MELTING_WARBLER_QUALITY_RARE", "Melting Warbler", "texMeltingWarblerBuff", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "MELTING_WARBLER_QUALITY");
            meltingWarblerQualityEpicBuff = Buffs.AddBuff("MELTING_WARBLER_QUALITY_EPIC", "Melting Warbler", "texMeltingWarblerBuff", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "MELTING_WARBLER_QUALITY");
            meltingWarblerQualityLegendaryBuff = Buffs.AddBuff("MELTING_WARBLER_QUALITY_LEGENDARY", "Melting Warbler", "texMeltingWarblerBuff", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "MELTING_WARBLER_QUALITY");

            // Add stats modification
            Behaviour.AddStatsMod(meltingWarblerQualityUncommonBuff, UncommonStatsMod_Quality);
            Behaviour.AddStatsMod(meltingWarblerQualityRareBuff, RareStatsMod_Quality);
            Behaviour.AddStatsMod(meltingWarblerQualityEpicBuff, EpicStatsMod_Quality);
            Behaviour.AddStatsMod(meltingWarblerQualityLegendaryBuff, LegendaryStatsMod_Quality);

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
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0F, 0.46155F, 0.00827F), new Vector3(15F, 0F, 0F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.3575F, -0.025F), new Vector3(10F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.275F, 0.02F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0F, 3F, 2.15F), new Vector3(305F, 180F, 0F), new Vector3(0.6F, 0.6F, 0.6F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.7715F, 0.0975F), new Vector3(15F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.85F, -0.7525F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 1.3965F, 0.80775F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.25F, -0.0375F), new Vector3(25F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0F, 0.325F, 0.07F), new Vector3(12.5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "HandL", new Vector3(0F, 0.3F, 0.125F), new Vector3(280F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0F, 0.3F, 0.0375F), new Vector3(5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 1.4325F, 2.15F), new Vector3(280F, 180F, 0F), new Vector3(1.35F, 1.35F, 1.35F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.3415F, 0.5145F, -0.045F), new Vector3(5F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunStock", new Vector3(-0.001F, -0.015F, 0.09F), new Vector3(85.00005F, 180F, 180F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(0F, 0.25F, -0.065F), new Vector3(345F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 7.25F, 1.4F), new Vector3(350F, 180F, 0F), new Vector3(2.4F, 2.4F, 2.4F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.30125F, 0.011F), new Vector3(30F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("False Son", "Head", new Vector3(-0.3925F, 0.45F, -0.0675F), new Vector3(1.25F, 262.25F, 0.5F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.8325F, -0.2225F, 0.20175F), new Vector3(35F, 40F, 135F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Technician", "Head", new Vector3(0F, 0.3385F, 0.025F), new Vector3(5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.255F, -0.058F, 0F), new Vector3(90F, 270F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.375F, -0.0375F, 0F), new Vector3(277.5F, 270F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Best Buddy", "Body", new Vector3(0.009F, -0.95F, -0.0125F), new Vector3(0F, 180F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            jumpBoostSetting = MainItem.CreateSetting("JUMP_BOOST", "Jump Boost", 2.0f, "How much should this item increase the jump height of the player? (2.0 = 2 meters)", _valueFormatting: "{0:0.00}m");
            jumpBoostStackingSetting = MainItem.CreateSetting("JUMP_BOOST_STACKING", "Jump Boost Stacking", 2.0f, "How much should further stacks of this item increase the jump height of the player? (2.0 = 2 meters)", _valueFormatting: "{0:0.00}m");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            speedQualitySetting = MainItem.CreateQualitySetting("SPEED", "Speed", 30.0f, 60.0f, 100.0f, 150.0f, "How much should this item increase movement speed while airborne? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingQualitySetting = MainItem.CreateQualitySetting("SPEED_STACKING", "Speed Stacking", 30.0f, 60.0f, 100.0f, 150.0f, "How much should further stacks of this item increase movement speed while airborne? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            jumpBoost = jumpBoostSetting.Value;
            jumpBoostStacking = jumpBoostStackingSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            speedQualityValues.UpdateValues(speedQualitySetting, 0.01f);
            speedStackingQualityValues.UpdateValues(speedStackingQualitySetting, 0.01f);
        }

        void MeltingWarblerStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for item
            if (_count == 0) return;

            // Modify jump power
            _stats.baseJumpPowerAdd += 1.75f * jumpBoost + 1.75f * jumpBoostStacking * (_count - 1);
        }

        void GenericCharacterFixedUpdate_Quality(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (characterBody && inventory)
            {
                // Check if grounded or character motor is null
                if (_character.isGrounded || _character.characterMotor == null)
                {
                    // Update all buff counts
                    characterBody.SetBuffCount(meltingWarblerQualityBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityUncommonBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityRareBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityEpicBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityLegendaryBuff.buffDef.buffIndex, 0);

                    // Done
                    return;
                }

                // Get quality item counts
                QualityCounts counts = QualityCompat.GetItemCountsEffective(inventory, MainItem);

                // Check for any quality stacks
                if (counts.Total <= 0)
                {
                    // Update all buff counts
                    characterBody.SetBuffCount(meltingWarblerQualityBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityUncommonBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityRareBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityEpicBuff.buffDef.buffIndex, 0);
                    characterBody.SetBuffCount(meltingWarblerQualityLegendaryBuff.buffDef.buffIndex, 0);

                    // Done
                    return;
                }

                // Update buff counts based on quality item counts
                characterBody.SetBuffCount(meltingWarblerQualityBuff.buffDef.buffIndex, 1);
                characterBody.SetBuffCount(meltingWarblerQualityUncommonBuff.buffDef.buffIndex, counts.UNCOMMON);
                characterBody.SetBuffCount(meltingWarblerQualityRareBuff.buffDef.buffIndex, counts.RARE);
                characterBody.SetBuffCount(meltingWarblerQualityEpicBuff.buffDef.buffIndex, counts.EPIC);
                characterBody.SetBuffCount(meltingWarblerQualityLegendaryBuff.buffDef.buffIndex, counts.LEGENDARY);
            }
        }

        private void UncommonStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Increase movement speed
            _stats.moveSpeedMultAdd += _count == 0 ? 0.0f : speedQualityValues.UNCOMMON + (_count - 1) * speedStackingQualityValues.UNCOMMON;
        }

        private void RareStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Increase movement speed
            _stats.moveSpeedMultAdd += _count == 0 ? 0.0f : speedQualityValues.RARE + (_count - 1) * speedStackingQualityValues.RARE;
        }

        private void EpicStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Increase movement speed
            _stats.moveSpeedMultAdd += _count == 0 ? 0.0f : speedQualityValues.EPIC + (_count - 1) * speedStackingQualityValues.EPIC;
        }

        private void LegendaryStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Increase movement speed
            _stats.moveSpeedMultAdd += _count == 0 ? 0.0f : speedQualityValues.LEGENDARY + (_count - 1) * speedStackingQualityValues.LEGENDARY;
        }
    }
}
