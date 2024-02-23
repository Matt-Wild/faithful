using UnityEngine;
using R2API;

namespace Faithful
{
    internal class Inspiration
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store buff
        Buff inspirationBuff;

        // Constructor
        public Inspiration(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create Inspiration buff
            inspirationBuff = toolbox.buffs.AddBuff("INSPIRATION", "texbuffinspiredboost", Color.white);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(inspirationBuff, InspirationStatsMod);
        }

        void InspirationStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify crit chance
            _stats.critAdd += 1.0f * _count;

            // Modify crit damage
            _stats.critDamageMultAdd += 0.10f * _count;
        }
    }
}
