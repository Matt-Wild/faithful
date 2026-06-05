using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class AppraisersEye : ItemBase
    {
        // Store buff
        static Buff scrutinizedBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store item settings
        Setting<bool> hiddenFromLogbookSetting;
        Setting<int> maxDebuffsSetting;
        Setting<int> maxDebuffsStackingSetting;
        Setting<float> critDamageSetting;

        // Store quality settings
        QualitySetting<float> damageQualitySetting;
        QualitySetting<float> damageStackingQualitySetting;

        // Store item stats
        bool hiddenFromLogbook;
        int maxDebuffs;
        int maxDebuffsStacking;
        static float critDamage;

        // Store quality item stats
        QualityValues<float> damageQualityValues = new();
        QualityValues<float> damageStackingQualityValues = new();

        // Store quality explosion candidates by victim
        static readonly Dictionary<HealthComponent, List<QualityExplosionCandidate>> qualityExplosionCandidates = new();

        const float qualityExplosionRadius = 18.0f;

        // Overlay for Scrutinized debuff
        static readonly Overlays.Overlay scrutinizedOverlay = Overlays.CreateOverlay(new Overlays.OverlaySettings
        {
            MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat",
            Colour = new Color(0.16f, 0.0f, 0.32f, 0.64f)
        });

        // Hook state
        static bool critDamageHookSetupAttempted;
        static bool critDamageILHookRegistered;
        static bool critDamageILPatternMatched;
        static bool critDamageFallbackEnabled;

        // IL hook identifiers
        const string takeDamageProcessHookName = "AppraisersEye.TakeDamageProcess";

        // Constructor
        public AppraisersEye(Toolbox _toolbox) : base(_toolbox, "APPRAISERS_EYE")
        {
            // Create display settings
            CreateDisplaySettings("appraiserseyedisplaymesh");

            // Create item and buff
            MainItem = Items.AddItem(token, "Appraisers Eye", [ItemTag.Damage, ItemTag.Technology, ItemTag.WorldUnique], "texappraiserseyeicon", "appraiserseyemesh", ItemTier.VoidTier3, _supportsQuality: true, _displaySettings: displaySettings, _namePrefix: "Collectors Vision", _hiddenFromLogbook: true);
            scrutinizedBuff = Buffs.AddBuff("SCRUTINIZED", "Scrutinized", "texBuffScrutinizedEye", Color.white, _isDebuff: true, _overlay: scrutinizedOverlay);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link behaviour
            Behaviour.AddOnHitEnemyLateCallback(OnHitEnemyLate);

            // More specific crit damage setup
            SetupCritDamageModifier();
        }

        public override void QualityConstructor()
        {
            // Link Quality death explosion behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath_Quality);
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
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0.0925F, 0.25F, 0.13375F), new Vector3(355F, 37.5F, 352.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.1621F, 0.12725F), new Vector3(344F, 0F, 0F), new Vector3(0.0825F, 0.0825F, 0.0825F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.055F, 0.12375F), new Vector3(0F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(-0.93F, 3.3725F, -0.46F), new Vector3(303.75F, 180F, 180F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.5745F, -0.26F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.79125F, 0.6F), new Vector3(0F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.099F, 0.1025F), new Vector3(338.5F, 0F, 0F), new Vector3(0.0475F, 0.0475F, 0.0475F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0.0541F, 0.14375F, 0.12125F), new Vector3(345.75F, 37.5F, 358.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "Eye", new Vector3(0F, 0.8375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0.05375F, 0.121F, 0.112F), new Vector3(345F, 31.25F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 2.145F, 1.1125F), new Vector3(350F, 0F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "Head", new Vector3(0F, 0.0675F, 0.1325F), new Vector3(355F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Head", new Vector3(0F, 0.125F, 0.1075F), new Vector3(337.5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Hand", new Vector3(-0.019F, 0.09F, 0.0145F), new Vector3(350F, 270F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.1615F, 0.1215F), new Vector3(351.25F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "HandR", new Vector3(0.01525F, 0.144F, -0.0404F), new Vector3(348.75F, 191.25F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.2065F, 0.1195F, -0.08675F), new Vector3(287.5F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.12555F, -0.1748F, 0.05195F), new Vector3(67.5F, 313.75F, 317.5F), new Vector3(0.05125F, 0.05125F, 0.05125F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.121F, 0.1885F, -0.06F), new Vector3(290F, 202.5F, 158.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "ChestEye", new Vector3(0F, 0F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            hiddenFromLogbookSetting = MainItem.CreateSetting("HIDDEN_FROM_LOGBOOK", "Hide From Logbook?", true, "Should this item be hidden from the logbook?", false, _canRandomise: false, _restartRequired: true);
            maxDebuffsSetting = MainItem.CreateSetting("MAX_DEBUFFS", "Max Debuffs", 1, "What's the maximum amount of scrutinized you should be able to apply to a single enemy using this item? (1 = 1 scrutinized)");
            maxDebuffsStackingSetting = MainItem.CreateSetting("MAX_DEBUFFS_STACKING", "Max Debuffs Stacking", 1, "What's the maximum amount of scrutinized you should be able to apply to a single enemy using further stacks of this item? (1 = 1 scrutinized)");
            critDamageSetting = MainItem.CreateSetting("CRIT_DAMAGE_MULT", "Crit Damage Multiplier", 20.0f, "How much should each stack of scrutinized increase crit damage? (20.0 = 20% increase)");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            damageQualitySetting = MainItem.CreateQualitySetting("DAMAGE", "Explosion Damage", 100.0f, 200.0f, 300.0f, 400.0f, "How much base damage should scrutinized enemies explode for on death per scrutinize stack? (100.0 = 100% base damage)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
            damageStackingQualitySetting = MainItem.CreateQualitySetting("DAMAGE_STACKING", "Explosion Damage Stacking", 100.0f, 200.0f, 300.0f, 400.0f, "How much additional base damage should further quality stacks add to scrutinized enemy death explosions per scrutinize stack? (100.0 = 100% base damage)", _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            hiddenFromLogbook = hiddenFromLogbookSetting.Value;
            maxDebuffs = maxDebuffsSetting.Value;
            maxDebuffsStacking = maxDebuffsStackingSetting.Value;
            critDamage = critDamageSetting.Value / 100.0f;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update if hidden in logbook
            MainItem.hiddenFromLogbook = hiddenFromLogbook;

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            damageQualityValues.UpdateValues(damageQualitySetting, 0.01f);
            damageStackingQualityValues.UpdateValues(damageStackingQualitySetting, 0.01f);
        }

        public override Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            return new Dictionary<string, string>
            {
                { "MAX_DEBUFFS", maxDebuffsSetting.Value.ToString() },
                { "MAX_DEBUFFS_STACKING", maxDebuffsStackingSetting.Value.ToString() },
                { "CRIT_DAMAGE_MULT", critDamageSetting.Value.ToString() }
            };
        }

        private void SetupCritDamageModifier()
        {
            // Don't attempt this more than once
            if (critDamageHookSetupAttempted) return;

            // Update hook state
            critDamageHookSetupAttempted = true;
            critDamageILHookRegistered = false;
            critDamageILPatternMatched = false;

            // Attempt to do preferred IL hook
            try
            {
                IL.RoR2.HealthComponent.TakeDamageProcess += IL_HealthComponent_TakeDamageProcess;
                critDamageILHookRegistered = true;
            }
            catch (Exception e)
            {
                Log.Error($"[{takeDamageProcessHookName}] Failed to register IL hook. Falling back to behavioural replacement. Exception: {e}");
            }

            // Check if fallback is needed
            if (!critDamageILHookRegistered)
            {
                EnableCritDamageFallback("IL hook registration failed.");
            }
            else if (!critDamageILPatternMatched)
            {
                EnableCritDamageFallback("IL hook registered, but pattern was not matched.");
            }
        }

        private void EnableCritDamageFallback(string _reason)
        {
            // Ignore if fallback already enabled
            if (critDamageFallbackEnabled) return;

            critDamageFallbackEnabled = true;
            Behaviour.AddOnTakeDamageProcessCallback(OnTakeDamageProcess);

            Log.Warning($"[{takeDamageProcessHookName}] Using last-resort behavioural fallback. Reason: {_reason}");
        }

        private void OnTakeDamageProcess(HealthComponent _healthComponent, DamageInfo _info)
        {
            // If the IL hook is active, do nothing - this is the ideal behaviour
            if (critDamageILPatternMatched) return;

            // Validate input
            if (_info == null) return;

            // Modify damage
            // This isn't ideal since it applies the crit bonus before some skills register the damage as a crit
            // For attacks like Bandit's backstab this will not work properly - it is only a fallback
            _info.damage = ModifyFinalCritDamage(_info.damage, _healthComponent, _info);
        }

        private void OnHitEnemyLate(DamageInfo _info, GameObject _victim)
        {
            // Validate input
            if (_info == null || _victim == null) return;

            // Get victim body
            CharacterBody victimBody = _victim.GetComponent<CharacterBody>();
            if (victimBody == null) return;

            // Only crits should apply the debuff and damage bonus
            if (_info.crit == false) return;

            // Get attacker body
            if (_info.attacker == null) return;

            CharacterBody attackerBody = _info.attacker.GetComponent<CharacterBody>();
            if (attackerBody == null) return;

            // Get attacker inventory
            Inventory attackerInventory = attackerBody.inventory;
            if (attackerInventory == null) return;

            // Validate item and buff
            if (scrutinizedBuff == null || scrutinizedBuff.buffDef == null) return;
            if (MainItem == null || MainItem.itemDef == null) return;

            // Get item count
            int attackerItemCount = attackerInventory.GetItemCountEffective(MainItem.itemDef.itemIndex);
            if (attackerItemCount <= 0) return;

            // Quality variants should remember crit attempts even if the victim is already at this attacker's cap
            if (MainItem.supportsQuality && Utils.qualityEnabled)
            {
                TryRecordQualityExplosionCandidate(victimBody, attackerBody, attackerInventory);
            }

            // Calculate max debuffs this attacker can apply
            int currentMaxDebuffs = Utils.CalculateStackingValue(attackerItemCount, maxDebuffs, maxDebuffsStacking);

            // Get current debuff count
            int victimDebuffCount = victimBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);

            // Apply debuff if below cap
            if (victimDebuffCount < currentMaxDebuffs)
            {
                victimBody.AddBuff(scrutinizedBuff.buffDef.buffIndex);
            }
        }

        private void OnCharacterDeath_Quality(DamageReport _report)
        {
            // Host only behaviour
            if (!Utils.hosting) return;

            // Check if quality behaviour is available
            if (!MainItem.supportsQuality || !Utils.qualityEnabled) return;

            // Check for victim body and buff
            CharacterBody victimBody = _report.victimBody;
            if (victimBody == null || victimBody.healthComponent == null) return;
            if (scrutinizedBuff == null || scrutinizedBuff.buffDef == null) return;

            // Get and forget any stored quality explosion candidates for this victim
            HealthComponent victimHealthComponent = victimBody.healthComponent;
            if (!qualityExplosionCandidates.TryGetValue(victimHealthComponent, out List<QualityExplosionCandidate> candidates))
            {
                return;
            }
            qualityExplosionCandidates.Remove(victimHealthComponent);

            // Check if victim was scrutinized
            int debuffCount = victimBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);
            if (debuffCount <= 0) return;

            // Check for killer body and team
            CharacterBody killerBody = _report.attackerBody;
            if (killerBody == null || killerBody.teamComponent == null) return;
            TeamIndex killerTeamIndex = killerBody.teamComponent.teamIndex;

            // Get the strongest eligible ally who attempted to scrutinize this victim with a quality Appraiser's Eye
            QualityExplosionCandidate bestCandidate = null;
            float bestCombinedDamage = 0.0f;
            foreach (QualityExplosionCandidate candidate in candidates)
            {
                if (candidate == null || candidate.teamIndex != killerTeamIndex) continue;

                float combinedDamage = GetCandidateCombinedDamage(candidate);
                if (combinedDamage > bestCombinedDamage)
                {
                    bestCandidate = candidate;
                    bestCombinedDamage = combinedDamage;
                }
            }

            // No quality attacker from the killer's team attempted to scrutinize this enemy
            if (bestCandidate == null || bestCombinedDamage <= 0.0f) return;

            // Explode based on the strongest candidate, multiplied by scrutinize count
            FireQualityExplosion(victimBody, bestCandidate, bestCombinedDamage * debuffCount);
        }

        private void TryRecordQualityExplosionCandidate(CharacterBody _victimBody, CharacterBody _attackerBody, Inventory _attackerInventory)
        {
            // Validate input
            if (_victimBody == null || _victimBody.healthComponent == null || _attackerBody == null || _attackerInventory == null) return;
            if (_attackerBody.teamComponent == null) return;

            // Check if quality behaviour is available
            if (!MainItem.supportsQuality || !Utils.qualityEnabled) return;

            // Calculate the attacker's quality explosion damage
            float combinedDamage = CalculateQualityExplosionCombinedDamage(_attackerBody, _attackerInventory);
            if (combinedDamage <= 0.0f) return;

            // Get candidate list for this victim
            HealthComponent victimHealthComponent = _victimBody.healthComponent;
            if (!qualityExplosionCandidates.TryGetValue(victimHealthComponent, out List<QualityExplosionCandidate> candidates))
            {
                candidates = [];
                qualityExplosionCandidates[victimHealthComponent] = candidates;
            }

            // Update existing candidate if this attacker has already attempted to scrutinize this victim
            CharacterMaster attackerMaster = _attackerBody.master;
            foreach (QualityExplosionCandidate candidate in candidates)
            {
                if (candidate.Matches(attackerMaster, _attackerBody))
                {
                    candidate.attackerMaster = attackerMaster;
                    candidate.attackerBody = _attackerBody;
                    candidate.teamIndex = _attackerBody.teamComponent.teamIndex;
                    candidate.lastCombinedDamage = Mathf.Max(candidate.lastCombinedDamage, combinedDamage);
                    return;
                }
            }

            // Add new candidate
            candidates.Add(new QualityExplosionCandidate
            {
                attackerMaster = attackerMaster,
                attackerBody = _attackerBody,
                teamIndex = _attackerBody.teamComponent.teamIndex,
                lastCombinedDamage = combinedDamage
            });
        }

        private float CalculateQualityExplosionCombinedDamage(CharacterBody _attackerBody, Inventory _attackerInventory)
        {
            // Validate input
            if (_attackerBody == null || _attackerInventory == null) return 0.0f;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(_attackerInventory, MainItem);
            if (qualityCounts.Total <= 0) return 0.0f;

            // Calculate quality explosion damage coefficient
            float damageCoefficient = 0.0f;
            damageCoefficient += Utils.CalculateStackingValue(qualityCounts.UNCOMMON, damageQualityValues.UNCOMMON, damageStackingQualityValues.UNCOMMON);
            damageCoefficient += Utils.CalculateStackingValue(qualityCounts.RARE, damageQualityValues.RARE, damageStackingQualityValues.RARE);
            damageCoefficient += Utils.CalculateStackingValue(qualityCounts.EPIC, damageQualityValues.EPIC, damageStackingQualityValues.EPIC);
            damageCoefficient += Utils.CalculateStackingValue(qualityCounts.LEGENDARY, damageQualityValues.LEGENDARY, damageStackingQualityValues.LEGENDARY);
            if (damageCoefficient <= 0.0f) return 0.0f;

            // Use vanilla on-kill proc damage handling, matching the Shatterspleen
            return Util.OnKillProcDamage(_attackerBody.damage, damageCoefficient);
        }

        private float GetCandidateCombinedDamage(QualityExplosionCandidate _candidate)
        {
            // Validate candidate
            if (_candidate == null) return 0.0f;

            // Prefer current body and inventory values at death time when available
            CharacterBody currentBody = _candidate.attackerMaster != null ? _candidate.attackerMaster.GetBody() : _candidate.attackerBody;
            if (currentBody != null && currentBody.inventory != null)
            {
                float currentCombinedDamage = CalculateQualityExplosionCombinedDamage(currentBody, currentBody.inventory);
                if (currentCombinedDamage > 0.0f)
                {
                    _candidate.attackerBody = currentBody;
                    _candidate.lastCombinedDamage = currentCombinedDamage;
                    return currentCombinedDamage;
                }
            }

            // Fall back to the best stored value from previous crit attempts
            return _candidate.lastCombinedDamage;
        }

        private void FireQualityExplosion(CharacterBody _victimBody, QualityExplosionCandidate _candidate, float _baseDamage)
        {
            // Validate input
            if (_victimBody == null || _candidate == null || _baseDamage <= 0.0f) return;

            // Use the candidate who supplied the highest combined damage as the explosion attacker
            CharacterBody attackerBody = _candidate.attackerMaster != null ? _candidate.attackerMaster.GetBody() : _candidate.attackerBody;
            GameObject attackerObject = attackerBody != null ? attackerBody.gameObject : null;

            // Create Shatterspleen-style explosion
            Vector3 position = _victimBody.corePosition;
            Util.PlaySound("Play_bleedOnCritAndExplode_explode", _victimBody.gameObject);
            GameObject blastObject = UnityEngine.Object.Instantiate(GlobalEventManager.CommonAssets.bleedOnHitAndExplodeBlastEffect, position, Quaternion.identity);
            DelayBlast delayBlast = blastObject.GetComponent<DelayBlast>();
            if (delayBlast == null)
            {
                UnityEngine.Object.Destroy(blastObject);
                return;
            }

            delayBlast.position = position;
            delayBlast.baseDamage = _baseDamage;
            delayBlast.baseForce = 0.0f;
            delayBlast.radius = qualityExplosionRadius;
            delayBlast.attacker = attackerObject;
            delayBlast.inflictor = null;
            delayBlast.crit = attackerBody != null && Util.CheckRoll(attackerBody.crit, _candidate.attackerMaster);
            delayBlast.maxTimer = 0.0f;
            delayBlast.damageColorIndex = DamageColorIndex.Item;
            delayBlast.falloffModel = BlastAttack.FalloffModel.SweetSpot;

            TeamFilter teamFilter = blastObject.GetComponent<TeamFilter>();
            if (teamFilter != null)
            {
                teamFilter.teamIndex = _candidate.teamIndex;
            }

            NetworkServer.Spawn(blastObject);
        }

        private static void IL_HealthComponent_TakeDamageProcess(ILContext _il)
        {
            try
            {
                ILCursor c = new(_il);

                // Hopefully compatible with vanilla and Hypercrit2
                // Find critMultiplier load/getter, then find the next mul shortly afterwards
                if (!ILHelper.TryGotoAfterMemberThenNextOp(
                    c,
                    takeDamageProcessHookName,
                    x => ILHelper.MatchFieldLoadOrPropertyGetter(x, "RoR2.CharacterBody", "critMultiplier"),
                    OpCodes.Mul,
                    8))
                {
                    critDamageILPatternMatched = false;

                    if (Utils.verboseConsole) ILHelper.SafeDumpInstructions(_il, takeDamageProcessHookName);
                    return;
                }

                ILHelper.EmitFloatModifier<HealthComponent, DamageInfo>(
                    c,
                    OpCodes.Ldarg_0,
                    OpCodes.Ldarg_1,
                    ModifyFinalCritDamage);

                critDamageILPatternMatched = true;

                if (Utils.verboseConsole) Log.Info($"[{takeDamageProcessHookName}] Successfully hooked crit damage multiplier.");
            }
            catch (Exception e)
            {
                critDamageILPatternMatched = false;
                Log.Error($"[{takeDamageProcessHookName}] Exception while applying IL hook: {e}");

                if (Utils.verboseConsole) ILHelper.SafeDumpInstructions(_il, takeDamageProcessHookName);
            }
        }

        private static float ModifyFinalCritDamage(float _currentDamage, HealthComponent _healthComponent, DamageInfo _info)
        {
            // Validate input
            if (_healthComponent == null || _info == null) return _currentDamage;

            // Ignore if not a crit
            if (_info.crit == false) return _currentDamage;

            // Attempt to get character body
            CharacterBody characterBody = _healthComponent.body;
            if (characterBody == null) return _currentDamage;

            // Validate buff
            if (scrutinizedBuff == null || scrutinizedBuff.buffDef == null) return _currentDamage;

            // Get debuff count
            int debuffCount = characterBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);

            // Apply bonus damage
            if (debuffCount > 0)
            {
                _currentDamage *= 1.0f + (critDamage * debuffCount);
            }

            return _currentDamage;
        }

        private class QualityExplosionCandidate
        {
            public CharacterMaster attackerMaster;
            public CharacterBody attackerBody;
            public TeamIndex teamIndex;
            public float lastCombinedDamage;

            public bool Matches(CharacterMaster _attackerMaster, CharacterBody _attackerBody)
            {
                // Prefer master matching because bodies can be recreated
                if (_attackerMaster != null && attackerMaster == _attackerMaster) return true;

                // Fall back to body matching for masterless attackers
                return _attackerBody != null && attackerBody == _attackerBody;
            }
        }
    }
}
