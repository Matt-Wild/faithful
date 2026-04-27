using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class LongshotGeode : ItemBase
    {
        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> enableRadiusIndicatorSetting;
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> distanceSetting;

        // Store item stats
        bool enableRadiusIndicator;
        float damage;
        float damageStacking;
        float distanceThreshold;

        // Store additional quality settings
        QualitySetting<float> distanceQualitySetting;
        QualitySetting<float> damageQualitySetting;
        QualitySetting<float> damageStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> distanceQualityValues = new();
        QualityValues<float> damageQualityValues = new();
        QualityValues<float> damageStackingQualityValues = new();

        // Constructor
        public LongshotGeode(Toolbox _toolbox) : base(_toolbox, "LONGSHOT_GEODE")
        {
            // Create display settings
            CreateDisplaySettings("longshotgeodedisplaymesh");

            // Create Longshot Geode item
            MainItem = Items.AddItem(token, "Longshot Geode", [ItemTag.Damage], "texlongshotgeodeicon", "longshotgeodemesh", ItemTier.VoidTier1, _corruptToken: "ITEM_NEARBYDAMAGEBONUS_NAME", _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add On Incoming Damage behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);
        }

        public override void QualityConstructor()
        {
            // Behaviour integrated into base hooks
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
            displaySettings.AddCharacterDisplay("Commando", "HandL", new Vector3(0.00635F, 0.0875F, 0.0875F), new Vector3(355F, 15F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Huntress", "HandL", new Vector3(-0.005F, 0.079F, 0.065F), new Vector3(5F, 355F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Bandit", "MainWeapon", new Vector3(-0.05F, 0.25F, -0.0425F), new Vector3(0F, 180F, 180F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("MUL-T", "LowerArmL", new Vector3(0.135F, 3.2F, 1.5F), new Vector3(0F, 0F, 55F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Engineer", "HandL", new Vector3(0.005F, 0.11F, 0.09F), new Vector3(0F, 345F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.438F, 0.8125F, 1.568F), new Vector3(300F, 330F, 25F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.7625F, 0.698F), new Vector3(0F, 0F, 15F), new Vector3(0.375F, 0.375F, 0.375F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0F, 0.1775F, -0.1325F), new Vector3(25F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "HandL", new Vector3(-0.005F, 0.124F, 0.097F), new Vector3(12.5F, 345F, 10F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, -0.087F, 0.559F), new Vector3(10F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Loader", "MechHandL", new Vector3(-0.075F, 0.15F, 0.18F), new Vector3(5F, 325F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "HandL", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 120F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Acrid", "HandR", new Vector3(0F, 0F, 0F), new Vector3(45F, 0F, 0F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0F, 0.125F, -0.064F), new Vector3(0F, 95F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Railgunner", "TopRail", new Vector3(0F, 0.6525F, 0.0585F), new Vector3(0F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ElbowR", new Vector3(0.01F, -0.0125F, 0.005F), new Vector3(345F, 90F, 60F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0F, 18.25F, 0F), new Vector3(280F, 330F, 90F), new Vector3(2.5F, 2.5F, 2.5F));
            displaySettings.AddCharacterDisplay("Seeker", "HandL", new Vector3(0.0023F, -0.1F, 0.025F), new Vector3(7.5F, 2.15F, 141.5F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "HandL", new Vector3(0.01213F, 0.1175F, -0.03225F), new Vector3(0F, 180F, 180F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Chef", "HandL", new Vector3(0.00315F, 0.01855F, -0.0625F), new Vector3(0F, 180F, 180F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "HandR", new Vector3(0.046F, 0.02185F, 0.00425F), new Vector3(0F, 65F, 0F), new Vector3(0.045F, 0.045F, 0.045F));
            displaySettings.AddCharacterDisplay("Operator", "MuzzleGun", new Vector3(0F, 0.0025F, 0.0925F), new Vector3(15F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Drifter", "BagBulgeLeft", new Vector3(-0.12F, -0.095F, 0.012F), new Vector3(15F, 280F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            enableRadiusIndicatorSetting = MainItem.CreateSetting("ENABLE_RADIUS_INDICATOR", "Enable Radius Indicator?", false, "Should this item have a radius indicator visual effect?", false, true, _canRandomise: false);
            damageSetting = MainItem.CreateSetting("DAMAGE", "Damage", 15.0f, "How much should this item increase damage while the target is beyond the distance threshold? (15.0 = 15% increase)", _valueFormatting: "{0:0.0}%");
            damageStackingSetting = MainItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 15.0f, "How much should further stacks of this item increase damage while the target is beyond the distance threshold? (15.0 = 15% increase)", _valueFormatting: "{0:0.0}%");
            distanceSetting = MainItem.CreateSetting("DISTANCE", "Distance", 40.0f, "How far should the target need to be for the damage bonus to be applied? (40.0 = 40 meters)", _valueFormatting: "{0:0.0}m");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            distanceQualitySetting = MainItem.CreateQualitySetting("DISTANCE", "Distance", 35.0f, 30.0f, 25.0f, 20.0f, "How far should the target need to be for the damage bonus to be applied? (35.0 = 35 meters)", _valueFormatting: "{0:0.0}m");
            damageQualitySetting = MainItem.CreateQualitySetting("DAMAGE", "Damage", 25.0f, 35.0f, 50.0f, 75.0f, "How much should this item increase damage while the target is beyond the distance threshold? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
            damageStackingQualitySetting = MainItem.CreateQualitySetting("DAMAGE_STACKING", "Damage Stacking", 25.0f, 35.0f, 50.0f, 75.0f, "How much should further stacks of this item increase damage while the target is beyond the distance threshold? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            enableRadiusIndicator = enableRadiusIndicatorSetting.Value;
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            distanceThreshold = distanceSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            distanceQualityValues.UpdateValues(distanceQualitySetting);
            damageQualityValues.UpdateValues(damageQualitySetting, 0.01f);
            damageStackingQualityValues.UpdateValues(damageStackingQualitySetting, 0.01f);
        }

        void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check for attacker and victim
            if (_attacker == null || _victim == null)
            {
                return;
            }

            // Check for attacker and victim bodies
            if (!_attacker.hasBody || !_victim.hasBody)
            {
                return;
            }

            // Get attacker and victim bodies
            CharacterBody attackerBody = _attacker.GetBody();
            CharacterBody victimBody = _victim.GetBody();

            // Check for attacker inventory
            if (!attackerBody.inventory)
            {
                return;
            }

            // Get item count
            int count = attackerBody.inventory.GetItemCountEffective(MainItem.itemDef);

            // Has item?
            if (count <= 0) return;

            // Get distance between bodies
            float distance = (attackerBody.transform.position - victimBody.transform.position).magnitude;

            // Remember original damage
            float originalDamage = _report.damage;

            // Get distance threshold
            float currentDistanceThreshold = distanceThreshold;

            // Quality behaviour
            if (Utils.qualityEnabled)
            {
                // Get quality item counts
                QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(attackerBody.inventory, MainItem);

                // Subtract quality variants from base count (they change behaviour from the base item)
                count -= qualityCounts.Total;

                // Update distance threshold
                if (qualityCounts.LEGENDARY > 0) currentDistanceThreshold = distanceQualityValues.LEGENDARY;
                else if (qualityCounts.EPIC > 0) currentDistanceThreshold = distanceQualityValues.EPIC;
                else if (qualityCounts.RARE > 0) currentDistanceThreshold = distanceQualityValues.RARE;
                else if (qualityCounts.UNCOMMON > 0) currentDistanceThreshold = distanceQualityValues.UNCOMMON;

                // Check if distance is further than distance threshold
                if (distance >= currentDistanceThreshold)
                {
                    // Update damage from quality counts
                    _report.damage *= 1.0f + (qualityCounts.UNCOMMON == 0 ? 0.0f : damageQualityValues.UNCOMMON + (qualityCounts.UNCOMMON - 1) * damageStackingQualityValues.UNCOMMON)
                                           + (qualityCounts.RARE == 0 ? 0.0f : damageQualityValues.RARE + (qualityCounts.RARE - 1) * damageStackingQualityValues.RARE)
                                           + (qualityCounts.EPIC == 0 ? 0.0f : damageQualityValues.EPIC + (qualityCounts.EPIC - 1) * damageStackingQualityValues.EPIC)
                                           + (qualityCounts.LEGENDARY == 0 ? 0.0f : damageQualityValues.LEGENDARY + (qualityCounts.LEGENDARY - 1) * damageStackingQualityValues.LEGENDARY);
                }
            }

            // Check if distance is greater or equal to 50 metres (check for item count after quality check)
            if (count > 0 && distance >= currentDistanceThreshold)
            {
                // Apply damage bonus
                _report.damage *= 1.0f + damage + (damageStacking * (count - 1));
            }

            // Check if damage was modified
            if (_report.damage != originalDamage)
            {
                // Update damage colour
                _report.damageColorIndex = DamageColorIndex.Nearby;
            }
        }
    }
}
