using System.Collections.Generic;
using System.Globalization;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class NoxiousSlime : ItemBase
    {
        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> blightChanceSetting;

        // Store item stats
        float damage;
        float damageStacking;
        float blightChance;

        // Store quality settings
        protected QualitySetting<float> blightChanceQualitySetting;
        protected QualitySetting<float> blightChanceStackingQualitySetting;
        protected QualitySetting<float> blightDamageQualitySetting;
        protected QualitySetting<float> blightDamageStackingQualitySetting;

        // Store fetched quality values
        protected readonly QualityValues<float> blightChanceQualityValues = new();
        protected readonly QualityValues<float> blightChanceStackingQualityValues = new();
        protected readonly QualityValues<float> blightDamageQualityValues = new();
        protected readonly QualityValues<float> blightDamageStackingQualityValues = new();

        private const float BlightDuration = 5.0f;

        // Vanilla Blight at 5 seconds is effectively 300% total damage
        // Quality damage is expressed as total damage, so we convert it to a damageMultiplier
        private const float DefaultBlightTotalDamageCoefficient = 3.0f;

        // Constructor
        public NoxiousSlime(Toolbox _toolbox) : base(_toolbox, "NOXIOUS_SLIME")
        {
            // Create display settings
            CreateDisplaySettings("noxiousslimedisplaymesh");

            // Create Noxious Slime item
            MainItem = Items.AddItem(token, "Noxious Slimes", [ItemTag.Damage], "texnoxiousslimeicon", "noxiousslimemesh", ItemTier.Tier3, _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Inject DoT behaviour
            Behaviour.AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRef);

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
        }

        public override void QualityConstructor()
        {
            // Quality behaviour is handled through the existing damage-over-time and on-hit hooks
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
            displaySettings.AddCharacterDisplay("Commando", "Pelvis", new Vector3(0.232F, -0.03F, -0.071F), new Vector3(12.5F, 110F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.22F, 0.02F, -0.017F), new Vector3(20.5F, 82.5F, 163.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "Pelvis", new Vector3(-0.1815F, -0.061F, 0.085F), new Vector3(348F, 130F, 180.5F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("MUL-T", "Hip", new Vector3(0.01F, -0.45F, 1.725F), new Vector3(0F, 0F, 180F), new Vector3(0.65F, 0.65F, 0.65F));
            displaySettings.AddCharacterDisplay("Engineer", "Pelvis", new Vector3(0.278F, -0.006F, 0.009F), new Vector3(350F, 281.25F, 173.5F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.2F, -1.225F), new Vector3(0F, 210F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.7125F, -1.165F), new Vector3(20F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Artificer", "Pelvis", new Vector3(0.2F, 0.04F, 0.098F), new Vector3(12F, 66F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "Pelvis", new Vector3(0.255F, 0.128F, 0.022F), new Vector3(350F, 264.25F, 176.5F), new Vector3(0.095F, 0.095F, 0.095F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.505F, 0.3995F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Loader", "Pelvis", new Vector3(0.27F, 0.14F, -0.032F), new Vector3(7.5F, 100F, 190F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-2.525F, 2.05F, 3.08F), new Vector3(350F, 304F, 9.25F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "Pelvis", new Vector3(0.2875F, 0.163F, -0.085F), new Vector3(357.5F, 324.5F, 168F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Pelvis", new Vector3(0.165F, 0.066F, 0.005F), new Vector3(20.5F, 80F, 197.25F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Pelvis", new Vector3(0.05275F, 0.045F, 0.1725F), new Vector3(353.5F, 19.8F, 186.25F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Scavenger", "Backpack", new Vector3(-4.0285F, 13.375F, 0.00053F), new Vector3(359.2F, 359.85F, 10.175F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Seeker", "ThighR", new Vector3(0F, 0.075F, 0.13F), new Vector3(12.5F, 5F, 180F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(-0.14F, 0.265F, -0.0575F), new Vector3(12.5F, 32F, 0.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.027F, 0.075F, 0.1465F), new Vector3(330F, 0F, 90F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Technician", "Pelvis", new Vector3(0.195F, 0.0275F, -0.04525F), new Vector3(345F, 110F, 2.5F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Operator", "Stomach", new Vector3(-0.04F, 0.0425F, 0.125F), new Vector3(330F, 345F, 97.5F), new Vector3(0.0375F, 0.0375F, 0.0375F));
            displaySettings.AddCharacterDisplay("Drifter", "BagPocketR", new Vector3(-0.0675F, -0.2525F, 0F), new Vector3(50F, 270F, 270F), new Vector3(0.125F, 0.125F, 0.125F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            damageSetting = MainItem.CreateSetting("DAMAGE", "Damage", 100.0f, "How much should this item increase the damage of damaging debuffs? (100.0 = 100% increase)", _valueFormatting: "{0:0.0}%");
            damageStackingSetting = MainItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 100.0f, "How much should further stacks of this item increase the damage of damaging debuffs? (100.0 = 100% increase)", _valueFormatting: "{0:0.0}%");
            blightChanceSetting = MainItem.CreateSetting("BLIGHT_CHANCE", "Blight Chance", 10.0f, "What percentage chance should this item have to inflict blight on hit? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            blightChanceQualitySetting = MainItem.CreateQualitySetting("BLIGHT_CHANCE", "Blight Chance", 10.0f, 20.0f, 30.0f, 40.0f, "How much blight chance does this quality provide? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");
            blightChanceStackingQualitySetting = MainItem.CreateQualitySetting("BLIGHT_CHANCE_STACKING", "Blight Chance Stacking", 10.0f, 20.0f, 30.0f, 40.0f, "How much additional blight chance do further stacks of this quality provide? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");
            blightDamageQualitySetting = MainItem.CreateQualitySetting("BLIGHT_DAMAGE", "Blight Damage", 400.0f, 500.0f, 750.0f, 1000.0f, "How much blight damage does this quality provide? (400.0 = 400% increase)", _valueFormatting: "{0:0.0}%");
            blightDamageStackingQualitySetting = MainItem.CreateQualitySetting("BLIGHT_DAMAGE_STACKING", "Blight Damage Stacking", 400.0f, 500.0f, 750.0f, 1000.0f, "How much additional blight damage does this quality provide? (400.0 = 400% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            blightChance = blightChanceSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            blightChanceQualityValues.UpdateValues(blightChanceQualitySetting);
            blightChanceStackingQualityValues.UpdateValues(blightChanceStackingQualitySetting);
            blightDamageQualityValues.UpdateValues(blightDamageQualitySetting, 0.01f);
            blightDamageStackingQualityValues.UpdateValues(blightDamageStackingQualitySetting, 0.01f);
        }

        public override Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            return new Dictionary<string, string>
            {
                { "DAMAGE", damageSetting.Value.ToString() },
                { "DAMAGE_STACKING", damageStackingSetting.Value.ToString() }
            };
        }

        void OnInflictDamageOverTimeRef(ref InflictDotInfo _inflictDotInfo)
        {
            // Check if hosting
            if (!Utils.hosting)
            {
                return;
            }

            // Check for victim and attacker
            if (_inflictDotInfo.victimObject == null || _inflictDotInfo.attackerObject == null)
            {
                return;
            }

            // Attempt to get attacker body and inventory
            CharacterBody attackerBody = _inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            if (attackerBody == null || attackerBody.inventory == null)
            {
                return;
            }

            // Get item count
            int count = attackerBody.inventory.GetItemCountEffective(MainItem.itemDef);

            // Has item?
            if (count > 0)
            {
                // Calculate damage multiplier
                float damageMult = 1.0f + damage + (damageStacking * (count - 1));

                // Modify DoT damage
                _inflictDotInfo.damageMultiplier *= damageMult;
                _inflictDotInfo.totalDamage *= damageMult;
            }
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Ignore DoTs
            if (_report.dotType != DotController.DotIndex.None) return;

            // Ignore attacks with no proc
            if (_report.damageInfo.procCoefficient <= 0.0f) return;

            // Check for attack character master
            CharacterMaster attacker = _report.attackerMaster;
            if (attacker == null) return;

            // Check for attacker body
            CharacterBody attackerBody = _report.attackerBody;
            if (attackerBody == null) return;

            // Check for victim body
            CharacterBody victimBody = _report.victimBody;
            if (victimBody == null || victimBody.healthComponent == null || !victimBody.healthComponent.alive) return;

            // Check for inventory of attacker
            Inventory inventory = attacker.inventory;
            if (inventory == null) return;

            // Get effective item count
            int count = inventory.GetItemCountEffective(MainItem.itemDef.itemIndex);

            // Check for item
            if (count == 0) return;

            // No Quality route
            if (!Utils.qualityEnabled)
            {
                // Roll dice
                if (Util.CheckRoll(blightChance * _report.damageInfo.procCoefficient, attacker))
                {
                    // Inflict blight
                    InflictBlight(victimBody, attackerBody, DefaultBlightTotalDamageCoefficient);
                }

                return;
            }

            // Quality route
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(inventory, MainItem);

            // Calculate quality blight chance
            float totalBlightChance = 0.0f;
            totalBlightChance += Utils.CalculateStackingValue(qualityCounts.UNCOMMON, blightChanceQualityValues.UNCOMMON, blightChanceStackingQualityValues.UNCOMMON);
            totalBlightChance += Utils.CalculateStackingValue(qualityCounts.RARE, blightChanceQualityValues.RARE, blightChanceStackingQualityValues.RARE);
            totalBlightChance += Utils.CalculateStackingValue(qualityCounts.EPIC, blightChanceQualityValues.EPIC, blightChanceStackingQualityValues.EPIC);
            totalBlightChance += Utils.CalculateStackingValue(qualityCounts.LEGENDARY, blightChanceQualityValues.LEGENDARY, blightChanceStackingQualityValues.LEGENDARY);

            // Calculate quality blight damage
            float totalBlightDamage = 0.0f;
            totalBlightDamage += Utils.CalculateStackingValue(qualityCounts.UNCOMMON, blightDamageQualityValues.UNCOMMON, blightDamageStackingQualityValues.UNCOMMON);
            totalBlightDamage += Utils.CalculateStackingValue(qualityCounts.RARE, blightDamageQualityValues.RARE, blightDamageStackingQualityValues.RARE);
            totalBlightDamage += Utils.CalculateStackingValue(qualityCounts.EPIC, blightDamageQualityValues.EPIC, blightDamageStackingQualityValues.EPIC);
            totalBlightDamage += Utils.CalculateStackingValue(qualityCounts.LEGENDARY, blightDamageQualityValues.LEGENDARY, blightDamageStackingQualityValues.LEGENDARY);

            // If there are normal Noxious Slimes as well, preserve their normal blight behaviour
            int normalCount = count - qualityCounts.Total;
            if (normalCount > 0)
            {
                totalBlightChance += blightChance;
                totalBlightDamage += DefaultBlightTotalDamageCoefficient;
            }

            // Check if there is anything meaningful to apply
            if (totalBlightChance <= 0.0f || totalBlightDamage <= 0.0f) return;

            // Roll dice
            if (Util.CheckRoll(totalBlightChance * _report.damageInfo.procCoefficient, attacker))
            {
                // Inflict blight
                InflictBlight(victimBody, attackerBody, totalBlightDamage);
            }
        }

        static void InflictBlight(CharacterBody _victimBody, CharacterBody _attackerBody, float _totalDamageCoefficient)
        {
            // Validate
            if (_victimBody == null || _attackerBody == null || _totalDamageCoefficient <= 0.0f)
            {
                return;
            }

            // Vanilla Blight at 5 seconds is effectively 300% total damage
            float damageMultiplier = _totalDamageCoefficient / DefaultBlightTotalDamageCoefficient;

            // Inflict blight
            DotController.InflictDot(
                _victimBody.gameObject,
                _attackerBody.gameObject,
                _victimBody.mainHurtBox,
                DotController.DotIndex.Blight,
                BlightDuration,
                damageMultiplier);
        }
    }
}
