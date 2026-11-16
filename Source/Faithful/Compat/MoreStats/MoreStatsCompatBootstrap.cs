using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;

namespace Faithful
{
    internal static class MoreStatsCompatBootstrap
    {
        internal const string MoreStatsGuid = "com.RiskOfBrainrot.MoreStats";

        internal static bool IsAvailable => Chainloader.PluginInfos.ContainsKey(MoreStatsGuid);

        internal static void TryInit()
        {
            // Check if we have MoreStats
            if (!IsAvailable) return;

            // Safer init method
            InitNoInline();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void InitNoInline()
        {
            MoreStatsCompat.Init();
        }

        internal static void TryShutdown()
        {
            // Check if we have MoreStats
            if (!IsAvailable) return;

            ShutdownNoInline();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void ShutdownNoInline()
        {
            MoreStatsCompat.Shutdown();
        }
    }
}
