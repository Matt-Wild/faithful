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

        // Overlay for buff
        static readonly Overlays.Overlay overlay = Overlays.CreateOverlay(new Overlays.OverlaySettings
        {
            MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat",
            Colour = new Color(0.16f, 1.0f, 0.0f, 0.6f)
        });

        public Overclocked()
        {
            // Create buff
            buff = Buffs.AddBuff("OVERCLOCKED", "Overclocked", "texBuffElectroSpeed", Color.white, false, _overlay: overlay);

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
