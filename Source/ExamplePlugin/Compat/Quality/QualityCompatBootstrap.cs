using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;

namespace Faithful
{
    internal static class QualityCompatBootstrap
    {
        internal const string QualityGuid = "com.Gorakh.ItemQualities";

        internal static bool IsAvailable => Chainloader.PluginInfos.ContainsKey(QualityGuid);

        internal static void TryInit()
        {
            // Check if we have Quality
            if (!IsAvailable) return;

            // Safer init method
            InitNoInline();

            // Init item quality constructors
            foreach (KeyValuePair<string, ItemBase> pair in Utils.Items)
            {
                if (!pair.Value.SupportsQuality) continue;
                pair.Value.QualityConstructor();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        static void InitNoInline()
        {
            QualityCompat.Init();
        }
    }
}