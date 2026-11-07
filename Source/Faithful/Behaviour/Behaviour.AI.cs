using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;

namespace Faithful
{
    internal delegate bool AllowFriendlyAIRetaliationCallback(BaseAI _ai, DamageReport _report);

    internal static partial class Behaviour
    {
        private static readonly List<AllowFriendlyAIRetaliationCallback> allowFriendlyAIRetaliationCallbacks = new();

        private static readonly HookRegistration aiRetaliationHook = new("BaseAI.OnBodyDamaged",
            () => On.RoR2.CharacterAI.BaseAI.OnBodyDamaged += HookAIRetaliation,
            () => On.RoR2.CharacterAI.BaseAI.OnBodyDamaged -= HookAIRetaliation);

        public static void AddAllowFriendlyAIRetaliationCallback(AllowFriendlyAIRetaliationCallback _callback)
        {
            allowFriendlyAIRetaliationCallbacks.Add(_callback);
            DebugLog("Added Allow Friendly AI Retaliation behaviour");
        }

        private static bool AllowFriendlyAIRetaliation(BaseAI _ai, DamageReport _report)
        {
            aiRetaliationHook.StartProfiling();
            try
            {
                foreach (AllowFriendlyAIRetaliationCallback callback in allowFriendlyAIRetaliationCallbacks)
                {
                    if (callback(_ai, _report)) return true;
                }
            }
            finally
            {
                aiRetaliationHook.StopProfiling();
            }

            return false;
        }

        private static void HookAIRetaliation(On.RoR2.CharacterAI.BaseAI.orig_OnBodyDamaged orig, BaseAI self, DamageReport damageReport)
        {
            if (!CanRunHook(aiRetaliationHook) || allowFriendlyAIRetaliationCallbacks.Count == 0 || !self.body || self.currentEnemy == null ||
                damageReport == null || !damageReport.attacker || !AllowFriendlyAIRetaliation(self, damageReport))
            {
                orig(self, damageReport);
                return;
            }

            bool previousNeverRetaliateFriendlies = self.neverRetaliateFriendlies;
            bool previousRetaliateUntargetable = self.retaliateUntargetable;
            float previousAttention = self.enemyAttention;
            GameObject previousBufferedTarget = self.bufferedTarget;
            bool changeTarget = self.currentEnemy.gameObject != damageReport.attacker;
            bool retaliated = false;

            try
            {
                // Allow this friendly retaliation through vanilla's two team restrictions
                self.neverRetaliateFriendlies = false;
                self.retaliateUntargetable = true;
                if (changeTarget)
                {
                    // Give the attacker priority over an existing target or queued player target
                    self.enemyAttention = 0f;
                    self.bufferedTarget = null;
                }

                orig(self, damageReport);
                retaliated = self.currentEnemy.gameObject == damageReport.attacker;
            }
            finally
            {
                self.neverRetaliateFriendlies = previousNeverRetaliateFriendlies;
                self.retaliateUntargetable = previousRetaliateUntargetable;

                // Keep existing target commitment if vanilla rejected the attacker, including on failure
                if (changeTarget && !retaliated)
                {
                    self.enemyAttention = previousAttention;
                    self.bufferedTarget = previousBufferedTarget;
                }
            }
        }

        private static readonly HookRegistration findAIEnemyHook = new("BaseAI.FindEnemyHurtBox",
            () => On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += HookFindAIEnemy,
            () => On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox -= HookFindAIEnemy);

        private static HurtBox HookFindAIEnemy(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (!CanRunHook(findAIEnemyHook)) return orig(self, maxDistance, full360Vision, filterByLoS);

            TargetSearchContext previous = currentTargetSearchContext;

            // Apply native exclusions and player priority before callbacks choose a preference group
            currentTargetSearchContext = new(self.body, self, candidate => candidate.healthComponent.body && (!self.ignoreFliers || !candidate.healthComponent.body.isFlying) &&
                candidate.healthComponent.body.bodyIndex != DLC4Content.BodyPrefabs.PlayerSpiritForm.bodyIndex, candidates => self.prioritizePlayers ? candidates.OrderByDescending(candidate => candidate.healthComponent.body.isPlayerControlled) : candidates);

            findAIEnemyHook.StartProfiling();
            try
            {
                return orig(self, maxDistance, full360Vision, filterByLoS);
            }
            finally
            {
                currentTargetSearchContext = previous;
                findAIEnemyHook.StopProfiling();
            }
        }
    }
}
