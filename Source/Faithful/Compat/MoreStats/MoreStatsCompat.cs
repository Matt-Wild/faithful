using System.Runtime.CompilerServices;
using MoreStats;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal static class MoreStatsCompat
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Init()
        {
            Log.Info("Faithful detected MoreStats, initializing compatibility!");

            // Apply our reduction after MoreStats has combined all other modifiers
            Behaviour.AddOnRecalculateStatsCallback(OnRecalculateStats);
        }

        private static void OnRecalculateStats(CharacterBody _body)
        {
            MoreStatCoefficients stats = StatHooks.GetMoreStatsFromBody(_body);
            float minimumDelay = Mathf.Max(Behaviour.MinimumShieldRechargeDelay, BaseStats.MinShieldDelaySeconds);
            float delay = stats.shieldRechargeDelay;
            Behaviour.ModifyShieldRechargeDelay(_body, minimumDelay, ref delay);
            if (delay == stats.shieldRechargeDelay) return;

            stats.shieldRechargeDelay = delay;

            // Stats have just been recalculated - update readiness without requesting another recalculation
            stats.shieldRechargeReady = _body.outOfDangerStopwatch >= delay;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal static void Shutdown()
        {
            Behaviour.RemoveOnRecalculateStatsCallback(OnRecalculateStats);
        }
    }
}
