using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class VengefulToaster : ItemBase
    {
        // Store buff
        Buff vengeanceBuff;

        // Store reference to vengeance behaviour
        Vengeance vengeanceBehaviour;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> durationSetting;
        Setting<float> durationStackingSetting;
        Setting<float> damageSetting;

        // Store item stats
        float duration;
        float durationStacking;

        // Store additional quality settings
        QualitySetting<float> radiusQualitySetting;
        QualitySetting<float> chanceQualitySetting;
        QualitySetting<float> chanceStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> radiusQualityValues = new();
        QualityValues<float> chanceQualityValues = new();
        QualityValues<float> chanceStackingQualityValues = new();

        // Store quality health state
        Dictionary<HealthComponent, float> combinedHealthBeforeDamage = [];

        // Constructor
        public VengefulToaster(Toolbox _toolbox, Vengeance _vengeance) : base(_toolbox, "VENGEFUL_TOASTER")
        {
            // Assign vengeance behaviour
            vengeanceBehaviour = _vengeance;

            // Get Vengeance buff
            vengeanceBuff = Buffs.GetBuff("VENGEANCE");

            // Create display settings
            CreateDisplaySettings("vengefultoasterdisplaymesh");

            // Create Vengeful Toaster item
            MainItem = Items.AddItem(token, "Vengeful Toaster", [ItemTag.Damage, ItemTag.Technology, ItemTag.AIBlacklist], "texvengefultoastericon", "vengefultoastermesh", ItemTier.Tier2, _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
        }

        public override void QualityConstructor()
        {
            // Store health before damage for quality calculations
            Behaviour.AddOnTakeDamageProcessCallback(OnTakeDamageProcess_Quality);

            // Clear stored quality state on scene exit
            Behaviour.AddOnPreSceneExitCallback((_exitController) =>
            {
                combinedHealthBeforeDamage.Clear();
            });
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
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.1738F, 0.0778F, 0.0148F), new Vector3(0F, 6F, 90F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "ThighR", new Vector3(-0.105F, -0.065F, 0.02785F), new Vector3(347.5F, 334F, 117.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighR", new Vector3(-0.09275F, 0.4F, 0.0715F), new Vector3(0F, 35F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.75F, 2.875F, -1.31F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.055F, 0.24F, 0.2575F), new Vector3(270F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.5375F, 0.82F, -0.5375F), new Vector3(0F, 45F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.7925F, -1.425F), new Vector3(0F, 180F, 180F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.239F, 0.05525F, -0.2225F), new Vector3(9.5F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.11575F, 0.13425F, -0.115F), new Vector3(2F, 45F, 260F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0.48F, 0.425F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0535F, 0.1525F, -0.1415F), new Vector3(352F, 90F, 270F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-1.83F, 3.05F, 3.025F), new Vector3(317.2F, 186.25F, 321F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "UpperArmL", new Vector3(0.0575F, 0.1375F, -0.1255F), new Vector3(0F, 61F, 265.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.1315F, 0.461F, -0.02325F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.082F, 0.27F, -0.191F), new Vector3(8F, 280F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.3675F, 0.012F, -0.21F), new Vector3(5F, 180F, 250F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(0.20668F, 0.2025F, 0.021F), new Vector3(4.5F, 40.5F, 15.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0.125F, -0.4F, -0.15F), new Vector3(85F, 12.5F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "UpperArmR", new Vector3(0.1025F, 0.17075F, 0.08525F), new Vector3(355F, 140F, 82.5F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(0.2125F, 0.0725F, -0.035F), new Vector3(0F, 0F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Drifter", "BagFlap", new Vector3(0.0115F, 0.19F, 0F), new Vector3(300F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            durationSetting = MainItem.CreateSetting("DURATION", "Duration", 4.0f, "How long should the vengeance buff last when granted? (4.0 = 4 seconds)", _valueFormatting: "{0:0.00}s");
            durationStackingSetting = MainItem.CreateSetting("DURATION_STACKING", "Duration Stacking", 2.0f, "How much longer should the vengeance buff last when granted with additional item stacks? (2.0 = 2 seconds)", _valueFormatting: "{0:0.00}s");
            damageSetting = MainItem.CreateSetting("DAMAGE", "Damage", 75.0f, "How much should each stack of vengeance increase damage? (75.0 = 75% increase)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            radiusQualitySetting = MainItem.CreateQualitySetting("RADIUS", "Ally Damage Radius", 20.0f, 30.0f, 40.0f, 50.0f, "How close should an ally need to be when they take damage for this quality item to grant vengeance? (20.0 = 20 meters)", _isStat: false, _minValue: 0.0f, _valueFormatting: "{0:0.0}m");
            chanceQualitySetting = MainItem.CreateQualitySetting("CHANCE", "Extra Vengeance Chance", 0.5f, 1.0f, 2.5f, 5.0f, "For each 1% of combined health missing, what percentage chance should this quality item have to grant an extra stack of vengeance? (0.5 = 0.5% chance)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
            chanceStackingQualitySetting = MainItem.CreateQualitySetting("CHANCE_STACKING", "Extra Vengeance Chance Stacking", 0.5f, 1.0f, 2.5f, 5.0f, "For each 1% of combined health missing, what extra percentage chance should further stacks of this quality item add to grant an extra stack of vengeance? (0.5 = 0.5% chance)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            duration = durationSetting.Value;
            durationStacking = durationStackingSetting.Value;

            // Apply damage to buff
            vengeanceBehaviour.damage = damageSetting.Value / 100.0f;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            radiusQualityValues.UpdateValues(radiusQualitySetting);
            chanceQualityValues.UpdateValues(chanceQualitySetting, 0.01f);
            chanceStackingQualityValues.UpdateValues(chanceStackingQualitySetting, 0.01f);
        }

        public override Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            return new Dictionary<string, string>
            {
                { "DURATION", durationSetting.Value.ToString() },
                { "DURATION_STACKING", durationStackingSetting.Value.ToString() },
                { "DAMAGE", damageSetting.Value.ToString() }
            };
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Ignore if damage dealt is 0
            if (_report.damageDealt == 0.0f) return;

            // Check for victim body
            CharacterBody victimBody = _report.victimBody;
            if (victimBody == null) return;

            // Track whether quality health state has been consumed
            bool victimCombinedHealthBeforeDamageFetched = false;
            float victimCombinedHealthBeforeDamage = 0.0f;

            // Check for inventory of victim
            Inventory inventory = victimBody.inventory;
            if (inventory != null)
            {
                // Get Vengeful Toaster amount
                int vengefulToasterCount = inventory.GetItemCountEffective(MainItem.itemDef.itemIndex);

                // Has Vengeful Toasters?
                if (vengefulToasterCount > 0)
                {
                    // Get quality item counts
                    QualityCounts qualityCounts = new();
                    if (MainItem.supportsQuality && Utils.qualityEnabled)
                    {
                        qualityCounts = QualityCompat.GetItemCountsEffective(inventory, MainItem);
                    }

                    // Get combined health before damage if quality needs it
                    if (qualityCounts.Total > 0)
                    {
                        victimCombinedHealthBeforeDamage = GetCombinedHealthBeforeDamage(victimBody, _report.damageDealt);
                        victimCombinedHealthBeforeDamageFetched = true;
                    }

                    // Grant Vengeance
                    GrantVengeance(victimBody, vengefulToasterCount, qualityCounts, victimCombinedHealthBeforeDamage);
                }
            }

            // Quality variants grant Vengeance when nearby allies take damage
            if (MainItem.supportsQuality && Utils.qualityEnabled)
            {
                if (!victimCombinedHealthBeforeDamageFetched) ClearCombinedHealthBeforeDamage(victimBody);
                TryGrantVengeanceToNearbyQualityAllies(victimBody);
            }
        }

        private void TryGrantVengeanceToNearbyQualityAllies(CharacterBody _victimBody)
        {
            // Validate input
            if (_victimBody == null || _victimBody.teamComponent == null) return;

            // Get victim team
            TeamIndex victimTeamIndex = _victimBody.teamComponent.teamIndex;

            // Check nearby allies
            foreach (CharacterBody allyBody in CharacterBody.readOnlyInstancesList)
            {
                // Validate ally
                if (allyBody == null || allyBody == _victimBody || allyBody.teamComponent == null || allyBody.teamComponent.teamIndex != victimTeamIndex) continue;
                if (allyBody.healthComponent == null || !allyBody.healthComponent.alive) continue;

                // Check for ally inventory
                Inventory allyInventory = allyBody.inventory;
                if (allyInventory == null) continue;

                // Get Vengeful Toaster amount
                int vengefulToasterCount = allyInventory.GetItemCountEffective(MainItem.itemDef.itemIndex);
                if (vengefulToasterCount <= 0) continue;

                // Get quality item counts
                QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(allyInventory, MainItem);
                if (qualityCounts.Total <= 0) continue;

                // Use the highest held quality for radius
                float radius = radiusQualityValues.GetValue(qualityCounts.GetHighestQuality());
                if (radius <= 0.0f) continue;

                // Check distance to damaged ally
                if ((allyBody.corePosition - _victimBody.corePosition).sqrMagnitude > radius * radius) continue;

                // Grant Vengeance
                GrantVengeance(allyBody, vengefulToasterCount, qualityCounts, GetCurrentCombinedHealth(allyBody));
            }
        }

        private void OnTakeDamageProcess_Quality(HealthComponent _healthComponent, DamageInfo _info)
        {
            // Validate input
            if (_healthComponent == null || _info == null || _info.damage <= 0.0f) return;

            // Store combined health before damage is applied
            combinedHealthBeforeDamage[_healthComponent] = _healthComponent.combinedHealth;
        }

        private void GrantVengeance(CharacterBody _body, int _itemCount, QualityCounts _qualityCounts, float _combinedHealthBeforeDamage)
        {
            // Validate input
            if (_body == null || _itemCount <= 0) return;

            // Calculate buff duration
            float buffDuration = Utils.CalculateStackingValue(_itemCount, duration, durationStacking);

            // Calculate buff stacks
            int buffStacks = 1 + GetExtraVengeanceStacks(_body, _qualityCounts, _combinedHealthBeforeDamage);

            // Add Vengeance buffs
            for (int i = 0; i < buffStacks; i++)
            {
                _body.AddTimedBuff(vengeanceBuff.buffDef, buffDuration);
            }
        }

        private int GetExtraVengeanceStacks(CharacterBody _body, QualityCounts _qualityCounts, float _combinedHealthBeforeDamage)
        {
            // Check for quality stacks
            if (_qualityCounts.Total <= 0) return 0;

            // Check for health component
            HealthComponent healthComponent = _body.healthComponent;
            if (healthComponent == null || healthComponent.fullCombinedHealth <= 0.0f) return 0;

            // Calculate missing health percentage from combined health before damage
            float combinedHealthBeforeDamage = Mathf.Clamp(_combinedHealthBeforeDamage, 0.0f, healthComponent.fullCombinedHealth);
            float missingHealthPercent = Mathf.Clamp01(1.0f - (combinedHealthBeforeDamage / healthComponent.fullCombinedHealth)) * 100.0f;
            if (missingHealthPercent <= 0.0f) return 0;

            // Calculate extra stack chance
            float extraStackChance = missingHealthPercent * CalculateExtraStackChance(_qualityCounts);
            if (extraStackChance <= 0.0f) return 0;

            // Every full 100% chance guarantees one extra stack
            int extraStacks = Mathf.FloorToInt(extraStackChance);

            // Roll the remaining fractional chance
            float rollChance = (extraStackChance - extraStacks) * 100.0f;
            if (rollChance > 0.0f && Util.CheckRoll(rollChance, _body.master))
            {
                extraStacks++;
            }

            // Return extra stacks
            return extraStacks;
        }

        private float CalculateExtraStackChance(QualityCounts _qualityCounts)
        {
            // Add up chance from all quality stacks
            float extraStackChance = 0.0f;
            extraStackChance += Utils.CalculateStackingValue(_qualityCounts.UNCOMMON, chanceQualityValues.UNCOMMON, chanceStackingQualityValues.UNCOMMON);
            extraStackChance += Utils.CalculateStackingValue(_qualityCounts.RARE, chanceQualityValues.RARE, chanceStackingQualityValues.RARE);
            extraStackChance += Utils.CalculateStackingValue(_qualityCounts.EPIC, chanceQualityValues.EPIC, chanceStackingQualityValues.EPIC);
            extraStackChance += Utils.CalculateStackingValue(_qualityCounts.LEGENDARY, chanceQualityValues.LEGENDARY, chanceStackingQualityValues.LEGENDARY);

            return extraStackChance;
        }

        private float GetCombinedHealthBeforeDamage(CharacterBody _body, float _damageDealt)
        {
            // Check for health component
            HealthComponent healthComponent = _body.healthComponent;
            if (healthComponent == null) return 0.0f;

            // Use exact health state if quality stored it before damage was applied
            if (combinedHealthBeforeDamage.TryGetValue(healthComponent, out float healthBeforeDamage))
            {
                combinedHealthBeforeDamage.Remove(healthComponent);
                return healthBeforeDamage;
            }

            // Reconstruct combined health before the hit when called from a damage report
            return Mathf.Min(healthComponent.fullCombinedHealth, healthComponent.combinedHealth + Mathf.Max(0.0f, _damageDealt));
        }

        private void ClearCombinedHealthBeforeDamage(CharacterBody _body)
        {
            // Check for health component
            HealthComponent healthComponent = _body.healthComponent;
            if (healthComponent == null) return;

            // Clear stored health state
            combinedHealthBeforeDamage.Remove(healthComponent);
        }

        private float GetCurrentCombinedHealth(CharacterBody _body)
        {
            // Check for health component
            HealthComponent healthComponent = _body.healthComponent;
            if (healthComponent == null) return 0.0f;

            // Return current combined health
            return healthComponent.combinedHealth;
        }
    }
}
