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

        // Store stats
        public float critChance = 1.0f;
        public float critDamageMult = 0.20f;

        // Constructor
        public Inspiration(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create Inspiration buff
            inspirationBuff = Buffs.AddBuff("INSPIRATION", "texbuffinspiredboost", Color.white);

            // Add stats modification
            Behaviour.AddStatsMod(inspirationBuff, InspirationStatsMod);
        }

        void InspirationStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify crit chance
            _stats.critAdd += critChance * _count;

            // Modify crit damage
            _stats.critDamageMultAdd += critDamageMult * _count;
        }
    }
}
