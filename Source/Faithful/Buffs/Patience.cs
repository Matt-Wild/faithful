using UnityEngine;
using RoR2;
using R2API;

namespace Faithful
{
    internal class Patience
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store buff
        Buff buff;

        // Store damage buff per stack
        public float damage = 0.25f;

        // Constructor
        public Patience(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create buff
            buff = Buffs.AddBuff("PATIENCE", "Patience", "texBuffHermitShawl", Color.white);

            // Add stats modification
            Behaviour.AddStatsMod(buff, PatienceStatsMod);
        }

        void PatienceStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage multiplier
            _stats.damageMultAdd += damage * _count;
        }
    }
}
