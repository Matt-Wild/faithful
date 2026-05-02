using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class TJetpack : ItemBase
    {
        // Buffs for amount of fuel remaining
        Buff emptyFuelBuff;
        Buff lowFuelBuff;
        Buff highFuelBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> showBuffSetting;
        Setting<bool> disableFallDamageSetting;
        Setting<float> fuelTimeSetting;
        Setting<float> fuelTimeStackingSetting;
        Setting<float> rechargeTimeSetting;
        Setting<float> rechargeTimeReductionSetting;
        Setting<float> maxVelocityMultiplierSetting;
        Setting<float> accelerationMultiplierSetting;

        // Item settings
        bool showBuff;

        // Store additional quality settings
        QualitySetting<float> damageQualitySetting;
        QualitySetting<float> damageStackingQualitySetting;
        QualitySetting<float> frequencyQualitySetting;
        QualitySetting<float> frequencyStackingQualitySetting;
        QualitySetting<int> frequencyMaxQualitySetting;

        // Store quality item stats
        QualityValues<float> damageQualityValues = new();
        QualityValues<float> damageStackingQualityValues = new();
        QualityValues<float> frequencyQualityValues = new();
        QualityValues<float> frequencyStackingQualityValues = new();
        QualityValues<int> frequencyMaxQualityValues = new();

        // Quality setting accessors
        public QualityValues<float> DamageQualityValues => damageQualityValues;
        public QualityValues<float> DamageStackingQualityValues => damageStackingQualityValues;
        public QualityValues<float> FrequencyQualityValues => frequencyQualityValues;
        public QualityValues<float> FrequencyStackingQualityValues => frequencyStackingQualityValues;
        public QualityValues<int> FrequencyMaxQualityValues => frequencyMaxQualityValues;

        // Constructor
        public TJetpack(Toolbox _toolbox) : base(_toolbox, "4T0N_JETPACK")
        {
            // Create display settings
            CreateDisplaySettings("4t0njetpackdisplaymesh");

            // Create item
            MainItem = Items.AddItem(token, "4-T0N Jetpack", [ItemTag.Utility, ItemTag.Technology, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist, ItemTag.DevotionBlacklist, ItemTag.ExtractorUnitBlacklist], "tex4t0njetpackicon", "4t0njetpackmesh", ItemTier.Tier3, _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Create buffs
            emptyFuelBuff = Buffs.AddBuff("4T0N_JETPACK_EMPTY", "Jetpack Fuel Empty", "texBuff4T0NJetpackEmpty", Color.white, false, _hasConfig: false, _isHidden: !showBuff);
            lowFuelBuff = Buffs.AddBuff("4T0N_JETPACK_LOW", "Jetpack Fuel Low", "texBuff4T0NJetpackLow", Color.white, _hasConfig: false, _usePercentageDisplay: true, _isHidden: !showBuff);
            highFuelBuff = Buffs.AddBuff("4T0N_JETPACK_HIGH", "Jetpack Fuel High", "texBuff4T0NJetpackHigh", Color.white, _hasConfig: false, _usePercentageDisplay: true, _isHidden: !showBuff);

            // Inject on transfer item behaviours
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);

            // Inject character body behaviours
            Behaviour.AddOnCharacterBodyStartCallback(OnCharacterBodyStart);
        }

        public override void QualityConstructor()
        {
            // No extra quality behaviour needed here
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
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3F, -0.235F), new Vector3(345F, 180F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.1125F, 0.15F, -0.135F), new Vector3(0F, 135F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0.007F, 0.1295F, -0.225F), new Vector3(0F, 172.5F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 1.1F, -2.1F), new Vector3(0F, 180F, 0F), new Vector3(1.85F, 1.85F, 1.85F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.2475F, -0.325F), new Vector3(345F, 180F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.00125F, 0.1025F, -0.25F), new Vector3(350F, 180F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.1875F, -0.265F), new Vector3(345F, 180F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.15F, -0.95F), new Vector3(0F, 180F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0F, 0.125F, -0.205F), new Vector3(350F, 179.6F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Acrid", "SpineStomach1", new Vector3(0F, 1.25F, 1F), new Vector3(270F, 0F, 0F), new Vector3(3F, 3F, 3F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.125F, -0.3075F), new Vector3(0F, 180F, 0F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(-0.00375F, -0.2875F, 0.02425F), new Vector3(0F, 178.75F, 0F), new Vector3(0.3725F, 0.3725F, 0.3725F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Stomach", new Vector3(-0.001F, 0F, -0.2F), new Vector3(5F, 179F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(0.0016F, -0.1755F, -0.125F), new Vector3(348.75F, 154.5F, 317F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0.00375F, 0F, -0.46F), new Vector3(22.5F, 180.25F, 0.25F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.0375F, -0.3F, 0F), new Vector3(65F, 270F, 0F), new Vector3(0.375F, 0.375F, 0.375F));
            displaySettings.AddCharacterDisplay("Technician", "Backpack", new Vector3(0F, 0.00175F, -0.025F), new Vector3(2.5F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(0F, -0.075F, -0.265F), new Vector3(0F, 180F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Drifter", "Stomach", new Vector3(-0.2F, -0.3375F, 0F), new Vector3(72.5F, 270F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            showBuffSetting = MainItem.CreateSetting("SHOW_BUFF", "Enable Fuel Buff?", true, "Should the fuel remaining be shown as a buff?", false, true, _restartRequired: true, _canRandomise: false);
            disableFallDamageSetting = MainItem.CreateSetting("DISABLE_FALL_DAMAGE", "Disable Fall Damage?", true, "When using the jetpack, should fall damage be mitigated?", false, _canRandomise: false);
            fuelTimeSetting = MainItem.CreateSetting("FUEL_TIME", "Fuel Time", 3.0f, "How much fuel should the jetpack have? (3.0 = 3 seconds)", _valueFormatting: "{0:0.00}s");
            fuelTimeStackingSetting = MainItem.CreateSetting("FUEL_TIME_STACKING", "Fuel Time Stacking", 1.5f, "How much additional fuel should the jetpack get per stack? (1.5 = 1.5 seconds)", _valueFormatting: "{0:0.00}s");
            rechargeTimeSetting = MainItem.CreateSetting("RECHARGE_TIME", "Recharge Time", 8.0f, "How long should it take for the jetpack to refuel after touching the ground? (8.0 = 8 seconds)", _valueFormatting: "{0:0.00}s");
            rechargeTimeReductionSetting = MainItem.CreateSetting("RECHARGE_TIME_REDUCTION", "Recharge Time Reduction", 20.0f, "How much should further stacks of this item decrease the recharge time of the jetpack? (20.0 = 20% reduction)", _maxValue: 100.0f, _randomiserMin: 1.0f, _randomiserMax: 40.0f, _valueFormatting: "{0:0.0}%");
            maxVelocityMultiplierSetting = MainItem.CreateSetting("MAX_VELOCITY_MULTIPLIER", "Max Velocity Multiplier", 1.0f, "How much faster or slower would you like the jetpack's max velocity to be? (1.0 = 1x max velocity)", _valueFormatting: "{0:0.00}x");
            accelerationMultiplierSetting = MainItem.CreateSetting("ACCELERATION_MULTIPLIER", "Acceleration Multiplier", 1.0f, "How much stronger or weaker would you like the jetpack to be? (1.0 = 1x acceleration)", _valueFormatting: "{0:0.00}x");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            damageQualitySetting = MainItem.CreateQualitySetting("IGNITE_DAMAGE", "Ignite Damage", 200.0f, 300.0f, 400.0f, 500.0f, "How much base damage should each jetpack ignite deal? (200.0 = 200% base damage)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
            damageStackingQualitySetting = MainItem.CreateQualitySetting("IGNITE_DAMAGE_STACKING", "Ignite Damage Stacking", 200.0f, 300.0f, 400.0f, 500.0f, "How much additional base damage should further quality stacks add to the jetpack ignite? (200.0 = 200% base damage)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
            frequencyQualitySetting = MainItem.CreateQualitySetting("IGNITE_FREQUENCY", "Ignite Frequency", 1.0f, 2.0f, 3.0f, 4.0f, "How many times per second should the jetpack ignite enemies below it? (1.0 = once per second)", _minValue: 0.0f, _valueFormatting: "{0:0.00}/s");
            frequencyStackingQualitySetting = MainItem.CreateQualitySetting("IGNITE_FREQUENCY_STACKING", "Ignite Frequency Stacking", 1.0f, 2.0f, 3.0f, 4.0f, "How many additional ignites per second should further quality stacks add? (1.0 = once per second)", _minValue: 0.0f, _valueFormatting: "{0:0.00}/s");
            frequencyMaxQualitySetting = MainItem.CreateQualitySetting("IGNITE_FREQUENCY_MAX", "Ignite Max Frequency", 15, 20, 30, 60, "What is the highest ignite frequency this quality can reach? (15 = 15 ignites per second)", _minValue: 1);
        }

        public override void FetchSettings()
        {
            // Get settings
            showBuff = showBuffSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            damageQualityValues.UpdateValues(damageQualitySetting, 0.01f);
            damageStackingQualityValues.UpdateValues(damageStackingQualitySetting, 0.01f);
            frequencyQualityValues.UpdateValues(frequencyQualitySetting);
            frequencyStackingQualityValues.UpdateValues(frequencyStackingQualitySetting);
            frequencyMaxQualityValues.UpdateValues(frequencyMaxQualitySetting);
        }

        void OnInventoryChanged(Inventory _inventory)
        {
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

            // Get new item count
            int count = _inventory.GetItemCountEffective(MainItem.itemDef);

            // Update TJetpack item count
            helper.tJetpack.UpdateItemCount(count);
        }

        void OnCharacterBodyStart(CharacterBody _character)
        {
            // Check if valid
            if (_character == null || _character.inventory == null)
            {
                return;
            }

            // Get item count
            int count = _character.inventory.GetItemCountEffective(MainItem.itemDef);

            // Has item?
            if (count > 0)
            {
                // Attempt to get Faithful behaviour
                FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(_character);
                if (helper == null)
                {
                    return;
                }

                // Update TJetpack item count
                helper.tJetpack.UpdateItemCount(count);
            }
        }
    }
}
