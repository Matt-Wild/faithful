using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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
        QualitySetting<float> healQualitySetting;
        QualitySetting<float> healStackingQualitySetting;
        QualitySetting<float> blockChanceQualitySetting;
        QualitySetting<float> blockChanceStackingQualitySetting;

        // Store quality item stats
        QualityValues<int> blockCostQualityValues = new();
        QualityValues<float> healQualityValues = new();
        QualityValues<float> healStackingQualityValues = new();
        float blockChanceQuality;
        float blockChanceStackingQuality;

        // Store quality block hook state
        static bool blockHookSetupAttempted;
        static bool blockILHookRegistered;
        static bool blockILPatternMatched;
        static bool blockFallbackEnabled;

        // IL hook identifiers
        const string takeDamageProcessBlockHookName = "HermitsShawl.TakeDamageProcessBlock";

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
            // Use IL hook if possible, otherwise use old OnIncomingDamage behaviour
            SetupQualityBlockHook();

            // Add late on take damage hook for quality heal effect
            Behaviour.AddOnTakeDamageProcessLateCallback(OnTakeDamageProcessLate_Quality);
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
            healQualitySetting = MainItem.CreateQualitySetting("HEAL", "Heal", 5.0f, 15.0f, 30.0f, 50.0f, "How much should this item heal the player upon blocking damage? (5 = 5 health)", _valueFormatting: "{0:0.0}");
            healStackingQualitySetting = MainItem.CreateQualitySetting("HEAL_STACKING", "Heal Stacking", 5.0f, 15.0f, 30.0f, 50.0f, "How much should further stacks of this item heal the player upon blocking damage? (5 = 5 health)", _valueFormatting: "{0:0.0}");
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
            healQualityValues.UpdateValues(healQualitySetting);
            healStackingQualityValues.UpdateValues(healStackingQualitySetting);
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

        private void SetupQualityBlockHook()
        {
            // Don't attempt this more than once
            if (blockHookSetupAttempted) return;

            // Update hook state
            blockHookSetupAttempted = true;
            blockILHookRegistered = false;
            blockILPatternMatched = false;

            // Attempt to do preferred IL hook
            try
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += IL_HealthComponent_TakeDamageProcess_Block;
                blockILHookRegistered = true;
            }
            catch (Exception e)
            {
                Log.Error($"[{takeDamageProcessBlockHookName}] Failed to register IL hook.\nFalling back to OnIncomingDamage behaviour.\nException: {e}");
            }

            // Check if fallback is needed
            if (!blockILHookRegistered)
            {
                EnableQualityBlockFallback("IL hook registration failed.");
            }
            else if (!blockILPatternMatched)
            {
                EnableQualityBlockFallback("IL hook registered, but pattern was not matched.");
            }
        }

        private void EnableQualityBlockFallback(string _reason)
        {
            // Ignore if fallback already enabled
            if (blockFallbackEnabled) return;

            blockFallbackEnabled = true;

            // Existing behaviour - this means the block check happens at the old timing
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage_Quality);

            Log.Warning($"[{takeDamageProcessBlockHookName}] Using fallback OnIncomingDamage behaviour.\nReason: {_reason}");
        }

        private void IL_HealthComponent_TakeDamageProcess_Block(ILContext _il)
        {
            try
            {
                ILCursor c = new(_il);

                // Find:
                // array[i].OnIncomingDamageServer(damageInfo);
                //
                // This comes after vanilla block checks such as Tougher Times and Safer Spaces
                if (!ILHelper.TryGotoAfterMethodCall(
                    c,
                    takeDamageProcessBlockHookName,
                    "RoR2.IOnIncomingDamageServerReceiver",
                    "OnIncomingDamageServer"))
                {
                    blockILPatternMatched = false;

                    if (Utils.verboseConsole)
                    {
                        ILHelper.SafeDumpInstructions(_il, takeDamageProcessBlockHookName);
                    }

                    return;
                }

                // Find the rejected check immediately after the incoming damage receiver loop:
                //
                // if (damageInfo.rejected)
                // {
                //     return;
                // }
                //
                // We inject before this, so vanilla's rejected check handles our block
                if (!TryGotoBeforePostBlockRejectedCheck(c))
                {
                    blockILPatternMatched = false;

                    if (Utils.verboseConsole)
                    {
                        ILHelper.SafeDumpInstructions(_il, takeDamageProcessBlockHookName);
                    }

                    return;
                }

                c.Emit(OpCodes.Ldarg_0); // HealthComponent self
                c.Emit(OpCodes.Ldarg_1); // DamageInfo damageInfo

                c.EmitDelegate<Action<HealthComponent, DamageInfo>>((healthComponent, damageInfo) =>
                {
                    if (healthComponent == null || damageInfo == null) return;

                    CharacterBody victimBody = healthComponent.body;
                    if (victimBody == null) return;

                    TryApplyQualityBlock(damageInfo, victimBody);
                });

                blockILPatternMatched = true;

                if (Utils.verboseConsole)
                {
                    Log.Info($"[{takeDamageProcessBlockHookName}] Successfully hooked after vanilla block checks.");
                }
            }
            catch (Exception e)
            {
                blockILPatternMatched = false;

                Log.Error($"[{takeDamageProcessBlockHookName}] Exception while applying IL hook: {e}");

                if (Utils.verboseConsole)
                {
                    ILHelper.SafeDumpInstructions(_il, takeDamageProcessBlockHookName);
                }
            }
        }

        private static bool TryGotoBeforePostBlockRejectedCheck(ILCursor _cursor)
        {
            return ILHelper.TryGotoNext(
                _cursor,
                takeDamageProcessBlockHookName,
                MoveType.Before,
                ILHelper.MatchLoadLocal,
                x => ILHelper.MatchFieldLoadByName(x, "damageInfo"),
                x => ILHelper.MatchFieldLoad(x, "RoR2.DamageInfo", "rejected"));
        }

        private void TryApplyQualityBlock(DamageInfo _damageInfo, CharacterBody _victim)
        {
            // Validate input
            if (_damageInfo == null || _victim == null) return;

            // If vanilla or another mod already rejected it, do nothing
            // This prevents wasting Patience when TT / SS already blocked the hit
            if (_damageInfo.rejected) return;

            // Respect BypassBlock, consistent with vanilla TT / SS behaviour
            if ((_damageInfo.damageType & DamageType.BypassBlock) > 0UL) return;

            // Ignore non-damaging hits
            if (_damageInfo.damage <= 0.0f) return;

            // Try and get victim inventory
            Inventory victimInventory = _victim.inventory;
            if (victimInventory == null) return;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(victimInventory, MainItem);

            int totalQualityCount = qualityCounts.Total;
            if (totalQualityCount <= 0) return;

            // Get highest quality the victim possesses
            Quality highestQuality = qualityCounts.GetHighestQuality();

            // Get buffs required to block
            int requiredBuffs = blockCostQualityValues.GetValue(highestQuality);
            if (requiredBuffs <= 0) return;

            // Get victim buff count
            int victimBuffCount = _victim.GetBuffCount(buff.buffDef.buffIndex);

            // Reject if victim does not have enough Patience
            if (victimBuffCount < requiredBuffs) return;

            // Roll block chance
            if (!Util.CheckRoll(CalculateBlockChance(totalQualityCount) * 100.0f, 0.0f, null)) return;

            // Do bear effect for now
            EffectData effectData = new()
            {
                origin = _damageInfo.position,
                rotation = Util.QuaternionSafeLookRotation((_damageInfo.force != Vector3.zero) ? _damageInfo.force : UnityEngine.Random.onUnitSphere)
            };

            EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, effectData, true);

            // Reject attack
            _damageInfo.rejected = true;

            // Remove buffs equal to block cost
            _victim.SetBuffCount(buff.buffDef.buffIndex, Mathf.Max(0, victimBuffCount - requiredBuffs));
        }

        private void OnIncomingDamage_Quality(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // If the IL hook is active, do nothing
            // This prevents duplicate behaviour if fallback somehow exists while IL is working
            if (blockILPatternMatched) return;

            // Validate input
            if (_report == null || _victim == null) return;

            // Check for victim body
            CharacterBody victim = _victim.GetBody();
            if (victim == null) return;

            // Use shared block logic
            TryApplyQualityBlock(_report, victim);
        }

        private void OnTakeDamageProcessLate_Quality(HealthComponent _healthComponent, DamageInfo _info)
        {
            // Validate input
            if (_healthComponent == null || _info == null) return;

            // Check if damage was blocked
            if (!_info.rejected) return;

            // Try and get victim body
            CharacterBody victimBody = _healthComponent.body;
            if (victimBody == null) return;

            // Try and get victim inventory
            Inventory victimInventory = victimBody.inventory;
            if (victimInventory == null) return;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(victimInventory, MainItem);
            if (qualityCounts.Total <= 0) return;

            // Add up the healing amount
            float healAmount = Utils.CalculateStackingValue(qualityCounts.UNCOMMON, healQualityValues.UNCOMMON, healStackingQualityValues.UNCOMMON)
                + Utils.CalculateStackingValue(qualityCounts.RARE, healQualityValues.RARE, healStackingQualityValues.RARE)
                + Utils.CalculateStackingValue(qualityCounts.EPIC, healQualityValues.EPIC, healStackingQualityValues.EPIC)
                + Utils.CalculateStackingValue(qualityCounts.LEGENDARY, healQualityValues.LEGENDARY, healStackingQualityValues.LEGENDARY);

            // Heal the victim
            _healthComponent.Heal(healAmount, default, true);
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
