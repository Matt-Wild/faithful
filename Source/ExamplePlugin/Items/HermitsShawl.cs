using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class HermitsShawl : ItemBase
    {
        // Store buff
        Buff buff;

        // Store reference to buff behaviour
        Patience buffBehaviour;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<int> maxBuffsSetting;
        Setting<int> maxBuffsStackingSetting;
        Setting<float> buffCooldownSetting;
        Setting<float> damageSetting;

        // Store additional quality settings
        QualitySetting<int> blockCostQualitySetting;
        QualitySetting<float> blockChanceQualitySetting;
        QualitySetting<float> blockChanceStackingQualitySetting;

        // Store quality item stats
        QualityValues<int> blockCostQualityValues = new();
        float blockChanceQuality;
        float blockChanceStackingQuality;

        // Constructor
        public HermitsShawl(Toolbox _toolbox, Patience _patience) : base(_toolbox, "HERMITS_SHAWL")
        {
            // Assign vengeance behaviour
            buffBehaviour = _patience;

            // Get buff
            buff = Buffs.GetBuff("PATIENCE");

            // Create display settings
            CreateDisplaySettings("HermitShawlDisplayMesh");

            // Create item
            MainItem = Items.AddItem(token, "Hermits Shawl", [ItemTag.Damage], "texHermitShawlIcon", "HermitShawlMesh", ItemTier.Tier2, _modifyItemDisplayPrefabCallback: ModifyDisplayPrefab, _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
        }

        public override void QualityConstructor()
        {
            // Link On Character Death behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage_Quality);
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
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3375F, 0F), new Vector3(20F, 0F, 0F), new Vector3(0.2375F, 0.2375F, 0.2375F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0.25025F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0F, 0.37F, 0.004F), new Vector3(0F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 1.925F, 0.645F), new Vector3(0F, 0F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.4125F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.33F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Neck", new Vector3(0F, -0.33F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0.25F, 0F), new Vector3(15F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.295F, 0.0285F), new Vector3(10F, 0F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.265F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0.3F, 0.0125F), new Vector3(0F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, -0.6F, -0.64F), new Vector3(0F, 270F, 340F), new Vector3(2.125F, 2.125F, 2.125F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.37125F, 0.037F), new Vector3(340F, 175F, 5F), new Vector3(0.1675F, 0.1675F, 0.1675F));
            displaySettings.AddCharacterDisplay("Railgunner", "Neck", new Vector3(-0.01168F, -0.02155F, 0.00168F), new Vector3(345F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(-0.0045F, 0.26625F, -0.01544F), new Vector3(0F, 22.5F, 0F), new Vector3(0.2125F, 0.2125F, 0.2125F));
            displaySettings.AddCharacterDisplay("Scavenger", "MuzzleEnergyCannon", new Vector3(0F, 0F, -16.5F), new Vector3(75F, 270F, 270F), new Vector3(4.5F, 4.5F, 4.5F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.2805F, -0.07725F), new Vector3(357.5F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0.0075F, 0.45F, -0.01213F), new Vector3(0F, 220F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.305F, 0F, 0F), new Vector3(290F, 180F, 270F), new Vector3(0.2375F, 0.2375F, 0.2375F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0.27F, 0.00795F), new Vector3(30F, 0F, 0F), new Vector3(0.1625F, 0.1625F, 0.1625F));
            displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(-0.16F, 0.05F, 0F), new Vector3(85F, 90F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Drifter", "Neck", new Vector3(-0.0125F, -0.0125F, 0F), new Vector3(285F, 270F, 180F), new Vector3(0.3F, 0.225F, 0.3F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            maxBuffsSetting = MainItem.CreateSetting("MAX_BUFFS", "Max Buffs", 4, "What's the maximum stack of patience the player should be able to receive with a single stack of this item? (4 = 4 stacks)");
            maxBuffsStackingSetting = MainItem.CreateSetting("MAX_BUFFS_STACKING", "Max Buffs Stacking", 4, "How many additional stacks of patience should the player be able to receive per extra stack of this item? (4 = 4 stacks)");
            buffCooldownSetting = MainItem.CreateSetting("BUFF_RECHARGE", "Buff Recharge Time", 10.0f, "After leaving combat how long does it take to receive the maximum amount of patience? (10.0 = 10 seconds)", _valueFormatting: "{0:0.0}s");
            damageSetting = MainItem.CreateSetting("DAMAGE", "Damage", 25.0f, "How much should each stack of patience increase damage? (25.0 = 25% increase)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            blockCostQualitySetting = MainItem.CreateQualitySetting("BLOCK_COST", "Block Cost", 4, 3, 2, 1, "How many stacks of patience are consumed upon blocking damage? (4 = 4 stacks)", _minValue: 1);
            blockChanceQualitySetting = MainItem.CreateQualitySetting("BLOCK_CHANCE", "Block Chance", 25.0f, "What is the chance of this item blocking damage when the user has enough patience? (25.0 = 25% chance)", _valueFormatting: "{0:0.0}%", _maxValue: 50.0f);
            blockChanceStackingQualitySetting = MainItem.CreateQualitySetting("BLOCK_CHANCE_STACKING", "Block Chance Stacking", 25.0f, "What is the additional chance of further stacks of this item blocking damage when the user has enough patience? (25.0 = 25% chance)", _valueFormatting: "{0:0.0}%", _maxValue: 50.0f);
        }

        public override void FetchSettings()
        {
            // Apply damage to buff
            buffBehaviour.damage = damageSetting.Value / 100.0f;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            blockCostQualityValues.UpdateValues(blockCostQualitySetting);
            blockChanceQuality = blockChanceQualitySetting.Value / 100.0f;
            blockChanceStackingQuality = blockChanceStackingQualitySetting.Value / 100.0f;
        }

        void ModifyDisplayPrefab(GameObject _prefab)
        {
            // Get first shawl object
            GameObject shawl = Utils.FindChildByName(_prefab.transform, "Shawl.001");

            // Add dynamic bone behaviour
            DynamicBone dynamicBone = shawl.AddComponent<DynamicBone>();
            
            // Set up dynamic bone
            Utils.ConfigureDynamicBone(dynamicBone, new DynamicBoneConfig());
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Ignore DoTs
            if (_report.dotType != DotController.DotIndex.None) return;

            // Ignore if damage dealt is 0
            if (_report.damageDealt == 0.0f) return;
            
            // Check for attacker body
            CharacterBody attacker = _report.attackerBody;
            if (attacker != null)
            {
                // Get patience buff count
                int buffCount = attacker.GetBuffCount(buff.buffDef);
                
                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    attacker.SetBuffCount(buff.buffDef.buffIndex, 0);

                    // Get faithful helper
                    FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(attacker);
                    if (helper != null)
                    {
                        // Get hermit's shawl behaviour
                        FaithfulHermitsShawlBehaviour behaviour = helper.hermitsShawl;
                        if (behaviour != null)
                        {
                            // Force attacker into combat
                            behaviour.ForceIntoCombat();
                        }
                    }
                }
            }

            // Check for victim body
            CharacterBody victim = _report.victimBody;
            if (victim != null)
            {
                // Get patience buff count
                int buffCount = victim.GetBuffCount(buff.buffDef);

                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    victim.SetBuffCount(buff.buffDef.buffIndex, 0);
                }
            }
        }

        private void OnIncomingDamage_Quality(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Validate input
            if (_report == null || _attacker == null || _victim == null) return;

            // Check for victim body
            CharacterBody victim = _victim.GetBody();
            if (victim == null) return;

            // Try and get victim inventory
            Inventory victimInventory = victim.inventory;
            if (victimInventory == null) return;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(victimInventory, MainItem);
            int totalQualityCount = qualityCounts.Total;
            if (totalQualityCount <= 0) return;

            // Get highest quality the victim possesses
            Quality highestQuality = qualityCounts.GetHighestQuality();

            // Get buffs required to block
            int requiredBuffs = blockCostQualityValues.GetValue(highestQuality);

            // Get victim buff count
            int victimBuffCount = victim.GetBuffCount(buff.buffDef.buffIndex);

            // Reject attack if victim has enough patience and block chance check passes
            if (victimBuffCount > requiredBuffs && Util.CheckRoll(CalculateBlockChance(totalQualityCount) * 100.0f, 0.0f, null))
            {
                // Do bear effect for now
                EffectData effectData3 = new()
                {
                    origin = _report.position,
                    rotation = Util.QuaternionSafeLookRotation((_report.force != Vector3.zero) ? _report.force : UnityEngine.Random.onUnitSphere)
				};
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData3, true);
                _report.rejected = true;

                // Remove buffs equal to block cost
                victim.SetBuffCount(buff.buffDef.buffIndex, victimBuffCount - requiredBuffs);
            }
        }

        private float CalculateBlockChance(int _count)
        {
            // Check for any items
            if (_count <= 0) return 0.0f;

            // Get safe block chance and stacking block chance
            float baseChance = Mathf.Clamp(blockChanceQuality, 0.0f, 0.5f);
            float stackingChance = Mathf.Clamp(blockChanceStackingQuality, 0.0f, 0.5f);

            // Hyperbolic stacking formula
            return 1.0f - ((1.0f - baseChance) / (1.0f + stackingChance * (_count - 1)));
        }
    }
}
