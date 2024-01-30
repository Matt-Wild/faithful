using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class MeltingWarbler
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item meltingWarblerItem;

        // Constructor
        public MeltingWarbler(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create Melting Warbler item
            meltingWarblerItem = toolbox.items.AddItem("MELTING_WARBLER", [ItemTag.Utility], "texmeltingwarblericon", "meltingwarblermesh", _tier: ItemTier.VoidTier2, _corruptToken: "ITEM_JUMPBOOST_NAME");

            // Add stats modification
            toolbox.behaviour.AddStatsMod(meltingWarblerItem, MeltingWarblerStatsMod);
        }

        void MeltingWarblerStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify jump power
            _stats.baseJumpPowerAdd += 1.8f * _count;
        }
    }
}
