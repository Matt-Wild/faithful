using R2API;
using UnityEngine;

namespace Faithful
{
    internal class Overclocked
    {
        // Buff reference
        Buff buff;

        // Buffs
        public float m_attackSpeedBuff = 0.5f;
        public float m_moveSpeedBuff = 0.25f;

        public Overclocked()
        {
            // Create buff
            buff = Buffs.AddBuff("OVERCLOCKED", "texBuffElectroSpeed", Color.white, false);

            // Add stats modification
            Behaviour.AddStatsMod(buff, OverclockedStatsMod);
        }

        void OverclockedStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack and movement speed
            _stats.attackSpeedMultAdd += m_attackSpeedBuff * _count;
            _stats.moveSpeedMultAdd += m_moveSpeedBuff * _count;
        }
    }
}
