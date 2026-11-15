using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class LuckyFigurine : ItemBase
    {
        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> itemChanceSetting;
        Setting<float> itemChanceStackingSetting;

        // Store item stats
        float itemChance;
        float itemChanceStacking;

        // Store additional quality settings
        QualitySetting<float> itemChanceQualitySetting;
        QualitySetting<float> itemChanceStackingQualitySetting;
        QualitySetting<int> bonusItemCountQualitySetting;
        QualitySetting<int> bonusItemCountStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> itemChanceQualityValues = new();
        QualityValues<float> itemChanceStackingQualityValues = new();
        QualityValues<int> bonusItemCountQualityValues = new();
        QualityValues<int> bonusItemCountStackingQualityValues = new();

        // Constructor
        public LuckyFigurine(Toolbox _toolbox) : base(_toolbox, "LUCKY_FIGURINE")
        {
            // Create display settings
            CreateDisplaySettings("luckyfigurinedisplaymesh");

            // Create item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Lucky Figurine",
                tags: [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotDuplicate, ItemTag.SacrificeBlacklist],
                iconName: "texluckyfigurineicon",
                modelName: "luckyfigurinemesh")
            {
                Tier = ItemTier.VoidTier2,
                CorruptToken = "ITEM_EXTRASHRINEITEM_NAME",
                DefaultSimulacrumBanned = true,
                SupportsQuality = true,
                DisplaySettings = displaySettings
            });

            // Create and fetch item settings
            CreateSettings();
            FetchSettings();

            Behaviour.AddShrineChancePurchaseCallback(ModifyShrineChancePurchase);
        }

        public override void QualityConstructor()
        {
            // Quality logic is handled in ModifyShrineChancePurchase
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
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.07135F, 0.155F, -0.127F), new Vector3(10F, 220F, 178.75F), new Vector3(0.135F, 0.135F, 0.135F));
            displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.08235F, -0.0525F, 0.1075F), new Vector3(35F, 27.5F, 180F), new Vector3(0.14F, 0.14F, 0.14F));
            displaySettings.AddCharacterDisplay("Bandit", "Stomach", new Vector3(0.07F, -0.02855F, -0.1595F), new Vector3(344.25F, 158F, 344F), new Vector3(0.16F, 0.16F, 0.16F));
            displaySettings.AddCharacterDisplay("MUL-T", "ThighL", new Vector3(0.4345F, 2.08F, 0.71F), new Vector3(357F, 8.5F, 246F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Pelvis", new Vector3(0.17825F, 0.118F, 0.146F), new Vector3(354F, 34F, 163.25F), new Vector3(0.16F, 0.16F, 0.16F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.693F, 0.6875F, 0.66F), new Vector3(320F, 292.5F, 0F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0.7205F, 1.01F, -1.5125F), new Vector3(0F, 92.5F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Artificer", "Pelvis", new Vector3(0.094F, 0.02525F, -0.1555F), new Vector3(354F, 165F, 190F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Mercenary", "UpperArmR", new Vector3(-0.173F, 0.2905F, 0.005F), new Vector3(333F, 259.5F, 191F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontR", new Vector3(-0.005F, 0.575F, -0.095F), new Vector3(356F, 180F, 165F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Loader", "ThighR", new Vector3(-0.10425F, 0.28575F, -0.042F), new Vector3(6.75F, 223.75F, 170F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Acrid", "ThighL", new Vector3(0.7745F, -0.3645F, 0.6495F), new Vector3(13F, 53F, 148.75F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(-0.244F, 0.175F, 0.1957F), new Vector3(2.5F, 10F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Railgunner", "CalfL", new Vector3(-0.075F, 0.112F, -0.087F), new Vector3(345F, 210F, 183.75F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmL", new Vector3(0.01195F, 0.0996F, 0.1048F), new Vector3(354.75F, 6.125F, 182F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0F, 16.2F, 3.65F), new Vector3(3.5F, 0F, 0F), new Vector3(2.75F, 2.75F, 2.75F));
            displaySettings.AddCharacterDisplay("Seeker", "Pelvis", new Vector3(0.168F, 0.019F, 0.067F), new Vector3(337.5F, 47.5F, 350F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(0.355F, 0.1F, 0.046F), new Vector3(340F, 70F, 8.5F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0.02105F, 0.118F, 0.0405F), new Vector3(0F, 16F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Technician", "Shin.R", new Vector3(-0.08065F, 0.11375F, 0F), new Vector3(353F, 270F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(-0.2571F, 0.2954F, 0.0121F), new Vector3(359.75F, 351F, 4.5F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("Drifter", "BagBulgeLeft", new Vector3(-0.08F, -0.00625F, 0.025F), new Vector3(355F, 285F, 285F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Enforcer", "ShieldFace", new Vector3(0.005F, -0.1125F, -0.375F), new Vector3(0F, 90F, 352.5F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Acolyte", "HandR", new Vector3(0.0638F, 0.0976F, -0.0262F), new Vector3(285F, 180F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            itemChanceSetting = MainItem.CreateSetting("ITEM_CHANCE", "Item Chance", 0.0f, "What should the chance be for this item to drop an additional item when using a Shrine of Chance? (10.0 = 10% chance)", _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");
            itemChanceStackingSetting = MainItem.CreateSetting("ITEM_CHANCE_STACKING", "Item Chance Stacking", 10.0f, "How much additional chance should further stacks of this item provide to drop an additional item when using a Shrine of Chance? (10.0 = 10% chance)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            itemChanceQualitySetting = MainItem.CreateQualitySetting("ITEM_CHANCE", "Item Chance", 15.0f, 30.0f, 50.0f, 75.0f, "What should the chance be for this item to drop an additional item when using a Shrine of Chance? (15.0 = 15% chance)", _valueFormatting: "{0:0.0}%");
            itemChanceStackingQualitySetting = MainItem.CreateQualitySetting("ITEM_CHANCE_STACKING", "Item Chance Stacking", 15.0f, 30.0f, 50.0f, 75.0f, "How much additional chance should further stacks of this item provide to drop an additional item when using a Shrine of Chance? (15.0 = 15% chance)", _valueFormatting: "{0:0.0}%");
            bonusItemCountQualitySetting = MainItem.CreateQualitySetting("BONUS_ITEM_COUNT", "Bonus Drop Attempts", 2, 3, 4, 5, "How many bonus item rolls should the this item grant when using a Shrine of Chance? (2 = 2 items)", _valueFormatting: "{0}");
            bonusItemCountStackingQualitySetting = MainItem.CreateQualitySetting("BONUS_ITEM_COUNT_STACKING", "Bonus Drop Attempts Stacking", 2, 3, 4, 5, "How many extra bonus item rolls should further stacks of this item grant when using a Shrine of Chance? (2 = 2 items)", _valueFormatting: "{0}");
        }

        public override void FetchSettings()
        {
            // Get item settings
            itemChance = itemChanceSetting.Value;
            itemChanceStacking = itemChanceStackingSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            itemChanceQualityValues.UpdateValues(itemChanceQualitySetting);
            itemChanceStackingQualityValues.UpdateValues(itemChanceStackingQualitySetting);
            bonusItemCountQualityValues.UpdateValues(bonusItemCountQualitySetting);
            bonusItemCountStackingQualityValues.UpdateValues(bonusItemCountStackingQualitySetting);
        }

        private void ModifyShrineChancePurchase(ShrineChanceBehavior _shrine, Interactor _activator, ShrineChancePurchaseResult _result)
        {
            // Use the same inventory as the shrine
            if (!_activator.TryGetComponent(out CharacterBody body) || !body.inventory) return;

            // Check the activator's Lucky Figurine count
            int luckyFigurineCount = body.inventory.GetItemCountEffective(MainItem.itemDef);
            if (luckyFigurineCount <= 0) return;

            // Replace a failed roll with a reward from the shrine's normal pool
            if (!_result.PrimaryPickup.isValid)
            {
                _result.PrimaryPickup = RollShrineReward(_shrine);
            }

            // Let vanilla count the reward as the final successful use
            if (!_result.PrimaryPickup.isValid) return;
            _shrine.successfulPurchaseCount = _shrine.maxPurchaseCount - 1;

            // Calculate bonus item chance and attempts
            float itemChancePercent = 0.0f;
            int bonusDropAttempts = 1;

            // Quality behaviour
            if (Utils.qualityEnabled)
            {
                // Get quality item counts
                QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(body.inventory, MainItem);

                // Check for any Quality items
                if (qualityCounts.Total > 0)
                {
                    // Add normal item chance
                    itemChancePercent += Utils.CalculateStackingValue(luckyFigurineCount - qualityCounts.Total, itemChance, itemChanceStacking);

                    // Add quality item chances
                    itemChancePercent += Utils.CalculateStackingValue(qualityCounts.UNCOMMON, itemChanceQualityValues.UNCOMMON, itemChanceStackingQualityValues.UNCOMMON)
                                       + Utils.CalculateStackingValue(qualityCounts.RARE, itemChanceQualityValues.RARE, itemChanceStackingQualityValues.RARE)
                                       + Utils.CalculateStackingValue(qualityCounts.EPIC, itemChanceQualityValues.EPIC, itemChanceStackingQualityValues.EPIC)
                                       + Utils.CalculateStackingValue(qualityCounts.LEGENDARY, itemChanceQualityValues.LEGENDARY, itemChanceStackingQualityValues.LEGENDARY);

                    // Reset bonus drop attempts before adding quality item bonus attempts
                    bonusDropAttempts = 0;

                    // Add quality item bonus attempts
                    bonusDropAttempts += Utils.CalculateStackingValue(qualityCounts.UNCOMMON, bonusItemCountQualityValues.UNCOMMON, bonusItemCountStackingQualityValues.UNCOMMON)
                                       + Utils.CalculateStackingValue(qualityCounts.RARE, bonusItemCountQualityValues.RARE, bonusItemCountStackingQualityValues.RARE)
                                       + Utils.CalculateStackingValue(qualityCounts.EPIC, bonusItemCountQualityValues.EPIC, bonusItemCountStackingQualityValues.EPIC)
                                       + Utils.CalculateStackingValue(qualityCounts.LEGENDARY, bonusItemCountQualityValues.LEGENDARY, bonusItemCountStackingQualityValues.LEGENDARY);

                    // Fallback incase quality settings are unusual (there must be at least one bonus drop attempt)
                    if (bonusDropAttempts <= 0)
                    {
                        bonusDropAttempts = 1;
                    }
                }
                else
                {
                    // Add normal item chance
                    itemChancePercent += Utils.CalculateStackingValue(luckyFigurineCount, itemChance, itemChanceStacking);
                }
            }

            // Quality behaviour skipped
            else
            {
                // Add normal item chance
                itemChancePercent += Utils.CalculateStackingValue(luckyFigurineCount, itemChance, itemChanceStacking);
            }

            // Roll each bonus slot independently using the activator's luck
            List<UniquePickup> bonusPickups = new List<UniquePickup>(bonusDropAttempts);
            for (int bonusDropNumber = 1; bonusDropNumber <= bonusDropAttempts; bonusDropNumber++)
            {
                float bonusChance = CalculateBonusChance(itemChancePercent, bonusDropNumber);
                if (bonusChance <= 0.0f || !Util.CheckRoll(bonusChance, body.master)) continue;

                UniquePickup bonusPickup = RollShrineReward(_shrine);
                if (bonusPickup.isValid)
                {
                    bonusPickups.Add(bonusPickup);
                }
            }

            // Schedule the bonus drops is there are any
            bool hasBonusPickup = bonusPickups.Count > 0;
            if (hasBonusPickup)
            {
                _result.AfterPrimaryDrop += shrine =>
                {
                    Transform origin = shrine.dropletOrigin;
                    int startingDirection = shrine.rng.nextNormalizedFloat < 0.5f ? 1 : -1;
                    shrine.StartCoroutine(DropBonusPickups(bonusPickups, origin.position, origin.forward, startingDirection));
                };
            }

            // Use the abundant outcome only when the shrine has an extra reward
            _result.ChatTokenOverride = hasBonusPickup ? "FAITHFUL_LUCKY_FIGURINE_SHRINE_CHANCE_MESSAGE_SUCCESS" : "FAITHFUL_LUCKY_FIGURINE_SHRINE_CHANCE_MESSAGE_FAIL";
            _result.UseJackpotEffectOverride = hasBonusPickup;
            if (hasBonusPickup && Assets.luckyFigurineShrineEffectPrefab)
            {
                _result.SpawnEffectOverride = SpawnShrineRewardEffect;
            }
        }

        private void SpawnShrineRewardEffect(ShrineChanceBehavior _shrine)
        {
            EffectManager.SpawnEffect(Assets.luckyFigurineShrineEffectPrefab, new EffectData
            {
                origin = _shrine.transform.position,
                rotation = Quaternion.identity,
                scale = 1.0f,
                color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.VoidItem)
            }, true);
        }

        private static IEnumerator DropBonusPickups(List<UniquePickup> _pickups, Vector3 _position, Vector3 _forward, int _startingDirection)
        {
            int angleStep = 30;
            float delayPerItem = 0.8f;

            // Adjust step and delay for large bonus drops
            if (_pickups.Count > 6)
            {
                angleStep = 15;
                delayPerItem = 0.4f;
            }

            // Include forward and both sides of the fan through 90 degrees
            int slotsPerRing = 1 + 2 * (90 / angleStep);
            float initialSpeed = 20.0f;
            float speedPerRing = 5.0f;

            yield return new WaitForSeconds(delayPerItem);

            // The primary reward already occupies the first forward slot
            int dropNumber = 1;
            foreach (UniquePickup pickup in _pickups)
            {
                if (!pickup.isValid) continue;

                int slot = dropNumber % slotsPerRing;
                int direction = slot % 2 == 1 ? _startingDirection : -_startingDirection;
                float angle = ((slot + 1) / 2) * angleStep * direction;
                float speed = initialSpeed + (dropNumber / slotsPerRing) * speedPerRing;
                Vector3 velocity = Quaternion.AngleAxis(angle, Vector3.up) * _forward * speed;
                PickupDropletController.CreatePickupDroplet(pickup, _position, velocity);
                dropNumber++;

                yield return new WaitForSeconds(delayPerItem);
            }
        }

        private static float CalculateBonusChance(float _itemChancePercent, int _bonusDropNumber)
        {
            float chance = Mathf.Max(0.0f, _itemChancePercent) / 100.0f;
            return Mathf.Min(chance, 1.0f - Mathf.Pow(4.0f, -chance / Mathf.Sqrt(_bonusDropNumber))) * 100.0f;
        }

        private static UniquePickup RollShrineReward(ShrineChanceBehavior _shrine)
        {
            return _shrine.dropTable ? _shrine.dropTable.GeneratePickup(_shrine.rng) : RollLegacyShrineReward(_shrine);
        }

        private static UniquePickup RollLegacyShrineReward(ShrineChanceBehavior _shrine)
        {
            Run run = Run.instance;
            WeightedSelection<PickupIndex> rewards = new WeightedSelection<PickupIndex>(4);
            AddReward(run.availableTier1DropList, _shrine.tier1Weight);
            AddReward(run.availableTier2DropList, _shrine.tier2Weight);
            AddReward(run.availableTier3DropList, _shrine.tier3Weight);
            AddReward(run.availableEquipmentDropList, _shrine.equipmentWeight);

            return rewards.Count > 0 ? new UniquePickup(rewards.Evaluate(_shrine.rng.nextNormalizedFloat)) : UniquePickup.none;

            void AddReward(List<PickupIndex> _pool, float _weight)
            {
                if (_weight > 0f && _pool.Count > 0)
                {
                    rewards.AddChoice(_shrine.rng.NextElementUniform(_pool), _weight);
                }
            }
        }
    }
}
