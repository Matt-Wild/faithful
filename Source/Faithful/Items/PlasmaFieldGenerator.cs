using RoR2;
using R2API;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class PlasmaFieldGenerator : ItemBase
    {
        // Item display settings
        ItemDisplaySettings displaySettings;

        // Item settings
        Setting<float> shieldSetting;
        Setting<float> shieldStackingSetting;
        Setting<float> rechargeTimeReductionSetting;
        Setting<float> rechargeTimeReductionStackingSetting;

        // Item stats from settings
        float shield;
        float shieldStacking;
        float rechargeTimeReduction;
        float rechargeTimeReductionStacking;

        // Item Quality settings
        QualitySetting<float> shieldIncreaseQualitySetting;
        QualitySetting<float> shieldIncreaseStackingQualitySetting;

        // Item Quality stats from settings
        QualityValues<float> shieldIncreaseQualityValues = new();
        QualityValues<float> shieldIncreaseStackingQualityValues = new();

        // Constructor
        public PlasmaFieldGenerator(Toolbox _toolbox) : base(_toolbox, "PLASMA_FIELD_GENERATOR")
        {
            // Create display settings
            CreateDisplaySettings("PlasmaFieldGeneratorDisplayMesh");

            // Create main item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Plasma Field Generator",
                tags: [ItemTag.Utility, ItemTag.Technology],
                iconName: "texPlasmaFieldGeneratorIcon",
                modelName: "PlasmaFieldGeneratorMesh")
            {
                Tier = ItemTier.VoidTier1,
                CorruptToken = "ITEM_PERSONALSHIELD_NAME",
                SupportsQuality = true,
                DisplaySettings = displaySettings
            });

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add shield based on maximum health
            Behaviour.AddModifyShieldFromHealthCallback(ModifyShieldFromHealth);

            // Reduce the delay before shields recharge
            Behaviour.AddModifyShieldRechargeDelayCallback(ModifyShieldRechargeDelay);

            // Use the base game's void shield visuals
            Behaviour.AddModifyVoidShieldVisualsCallback(ModifyVoidShieldVisuals);
        }

        public override void QualityConstructor()
        {
            // Add quality stats mods
            Behaviour.AddStatsMod(StatsMod_Quality);

            // Recharge shields when activating an interactable
            Behaviour.AddOnInteractionBeginProcCallback(OnInteractionBeginProc_Quality);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName)) return;

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.315F, 0.184F), new Vector3(326.75F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(-0.001F, 0.1715F, 0.1635F), new Vector3(330F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Bandit", "UpperArmR", new Vector3(-0.00115F, 0.2015F, -0.092F), new Vector3(345F, 180F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-0.05F, 1.2325F, 3.325F), new Vector3(0F, 0F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.27862F, 0.24375F), new Vector3(355F, 0F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.65375F, -0.275F), new Vector3(0F, 180F, 180F), new Vector3(0.325F, 0.325F, 0.325F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.61625F, -1.1275F), new Vector3(21.25F, 180F, 180F), new Vector3(0.325F, 0.325F, 0.325F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0.1955F, 0.1285F), new Vector3(332.5F, 0F, 0F), new Vector3(0.065F, 0.065F, 0.065F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.175F, 0.175F), new Vector3(336.25F, 0F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0.61F, -0.19F, 0.149F), new Vector3(0F, 78.75F, 0F), new Vector3(0.1125F, 0.1125F, 0.1125F));
            displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0.19925F, 0.182F), new Vector3(333.75F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Acrid", "ClavicleR", new Vector3(1.005F, 1.925F, 1.6F), new Vector3(326.75F, 45F, 92.5F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.2285F, 0.204F), new Vector3(349.5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Railgunner", "UpperArmL", new Vector3(0.047F, 0.129F, -0.019F), new Vector3(0F, 105F, 0F), new Vector3(0.0625F, 0.0625F, 0.0625F));
            displaySettings.AddCharacterDisplay("Void Fiend", "UpperArmR", new Vector3(0.196F, -0.0585F, -0.0075F), new Vector3(15F, 103.375F, 345F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.06125F, 0.085F), new Vector3(350F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0F, 0.4175F, 0.17625F), new Vector3(322.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.28375F, 0.22775F, 0F), new Vector3(280F, 270F, 180F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(-0.01F, -0.1025F, -0.08575F), new Vector3(67.5F, 158.75F, 270F), new Vector3(0.04F, 0.04F, 0.04F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(-0.0145F, 0.285F, 0F), new Vector3(297.5F, 257.5F, 195F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(-2.5375F, 2.05F, -5.725F), new Vector3(302.5F, 206.25F, 157.5F), new Vector3(1.2F, 1.2F, 1.2F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0.115F, 0.145F), new Vector3(3.75F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Enforcer", "Stomach", new Vector3(0F, -0.0024F, 0.1925F), new Vector3(0F, 0F, 0F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Acolyte", "Chest", new Vector3(0F, 0.138F, 0.146F), new Vector3(329.5F, 0F, 0F), new Vector3(0.04F, 0.04F, 0.04F));
        }

        private void ModifyShieldFromHealth(CharacterBody _body, float _maxHealth, ref float _shield)
        {
            // Get item count
            if (!_body.inventory) return;
            int count = _body.inventory.GetItemCountEffective(MainItem.itemDef);
            if (count <= 0) return;

            // Add shield using the current maximum health
            float shieldPercent = Utils.CalculateStackingValue(count, shield, shieldStacking);
            _shield += _maxHealth * shieldPercent / 100.0f;
        }

        private void ModifyShieldRechargeDelay(CharacterBody _body, float _minimumDelay, ref float _delay)
        {
            // Get item count
            if (!_body.inventory) return;
            int count = _body.inventory.GetItemCountEffective(MainItem.itemDef);
            if (count <= 0) return;

            if (float.IsInfinity(_delay) || float.IsNaN(_delay)) return;

            // Reserve the minimum and reduce only the time above it
            _delay = Mathf.Max(_minimumDelay, _delay);
            float reducibleDelay = _delay - _minimumDelay;
            if (reducibleDelay <= 0.0f) return;

            // Keep the first stack's reduction relative to the full incoming delay
            float reductionScale = _delay / reducibleDelay;
            float firstMultiplier = Mathf.Clamp01(1.0f - rechargeTimeReduction / 100.0f * reductionScale);
            float stackingMultiplier = Mathf.Clamp01(1.0f - rechargeTimeReductionStacking / 100.0f * reductionScale);
            _delay = _minimumDelay + reducibleDelay * firstMultiplier * Mathf.Pow(stackingMultiplier, count - 1);
        }

        private void ModifyVoidShieldVisuals(CharacterBody _body, ref bool _hasVoidShields)
        {
            // Keep existing void shields
            if (_hasVoidShields || !_body.inventory) return;

            // Use void shields while holding this item
            if (_body.inventory.GetItemCountEffective(MainItem.itemDef) > 0)
            {
                _hasVoidShields = true;
            }
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            shieldSetting = MainItem.CreateSetting("SHIELD", "Shield", 4.0f, "How much of the user's maximum health should the first stack of this item grant as shield? (4.0 = 4% shield)", _valueFormatting: "{0:0.0}%");
            shieldStackingSetting = MainItem.CreateSetting("SHIELD_STACKING", "Shield Stacking", 4.0f, "How much additional shield should further stacks grant? (4.0 = 4% shield)", _valueFormatting: "{0:0.0}%");
            rechargeTimeReductionSetting = MainItem.CreateSetting("RECHARGE_TIME_REDUCTION", "Recharge Time Reduction", 5.0f, "How much should the first stack reduce the delay before shields begin recharging? The delay cannot go below 1 second. (5.0 = 5% reduction)", _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");
            rechargeTimeReductionStackingSetting = MainItem.CreateSetting("RECHARGE_TIME_REDUCTION_STACKING", "Recharge Time Reduction Stacking", 5.0f, "How strongly should further stacks reduce the remaining delay above the 1 second minimum? The percentage is scaled against the full unmodified delay, matching the first stack's strength. (5.0 = 5% reduction strength)", _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            shieldIncreaseQualitySetting = MainItem.CreateQualitySetting("SHIELD_INCREASE", "Shield Increase", 10.0f, 25.0f, 50.0f, 100.0f, "How much should the first stack of this quality item increase shield from all sources? (10.0 = 10% increase)", _valueFormatting: "{0:0.0}%");
            shieldIncreaseStackingQualitySetting = MainItem.CreateQualitySetting("SHIELD_INCREASE_STACKING", "Shield Increase Stacking", 10.0f, 25.0f, 50.0f, 100.0f, "How much additional shield increase should further stacks of this quality item grant? (10.0 = 10% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            shield = shieldSetting.Value;
            shieldStacking = shieldStackingSetting.Value;
            rechargeTimeReduction = rechargeTimeReductionSetting.Value;
            rechargeTimeReductionStacking = rechargeTimeReductionStackingSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            shieldIncreaseQualityValues.UpdateValues(shieldIncreaseQualitySetting);
            shieldIncreaseStackingQualityValues.UpdateValues(shieldIncreaseStackingQualitySetting);
        }

        private void StatsMod_Quality(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for the item before fetching quality counts
            if (!_body.inventory || _body.inventory.GetItemCountEffective(MainItem.itemDef) <= 0) return;

            QualityCounts counts = QualityCompat.GetItemCountsEffective(_body.inventory, MainItem);
            if (counts.Total <= 0) return;

            // Increase shield from all sources, adding the bonus from each quality
            _stats.shieldMultAdd += (Utils.CalculateStackingValue(counts.UNCOMMON, shieldIncreaseQualityValues.UNCOMMON, shieldIncreaseStackingQualityValues.UNCOMMON)
                + Utils.CalculateStackingValue(counts.EPIC, shieldIncreaseQualityValues.EPIC, shieldIncreaseStackingQualityValues.EPIC)
                + Utils.CalculateStackingValue(counts.RARE, shieldIncreaseQualityValues.RARE, shieldIncreaseStackingQualityValues.RARE)
                + Utils.CalculateStackingValue(counts.LEGENDARY, shieldIncreaseQualityValues.LEGENDARY, shieldIncreaseStackingQualityValues.LEGENDARY)) / 100.0f;
        }

        private void OnInteractionBeginProc_Quality(Interactor _interactor, IInteractable _interactable, GameObject _interactableObject)
        {
            // Only the server should start regeneration
            if (!NetworkServer.active || !_interactor) return;

            CharacterBody body = _interactor.GetComponent<CharacterBody>();
            if (!body || !body.inventory || !body.healthComponent) return;
            if (body.inventory.GetItemCountEffective(MainItem.itemDef) <= 0) return;

            // Any quality variant starts normal shield regeneration
            QualityCounts counts = QualityCompat.GetItemCountsEffective(body.inventory, MainItem);
            if (counts.Total > 0) body.healthComponent.ForceShieldRegen();
        }
    }
}
