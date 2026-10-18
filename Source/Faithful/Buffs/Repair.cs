using R2API;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class Repair
    {
        // Buff reference
        Buff buff;

        // Buffs
        public float m_healPerc = 0.025f;

        // Heals per second (tick rate)
        public float m_tickRate = 2.0f;

        // Overlay for buff
        static readonly Overlays.Overlay overlay = Overlays.CreateOverlay(new Overlays.OverlaySettings
        {
            MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat",
            Colour = new Color(0.0f, 0.0f, 0.64f, 0.64f)
        });

        public Repair()
        {
            // Create buff
            buff = Buffs.AddBuff("REPAIR", "Arc Healing", "texBuffElectroHeal", Color.white, false, _overlay: overlay);

            // Register on tick behaviour
            Behaviour.AddOnCharacterBodyTickCallback(m_tickRate, OnTick);
        }

        private void OnTick(CharacterBody _body)
        {
            // Check for health component
            HealthComponent healthComponent = _body.GetComponent<HealthComponent>();
            if (healthComponent == null) return;

            // Check for buff
            if (!_body.HasBuff(buff.buffDef.buffIndex)) return;

            // Heal character
            healthComponent.HealFraction(m_healPerc, default);
        }
    }
}
