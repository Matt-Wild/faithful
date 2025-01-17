﻿using UnityEngine;
using RoR2;
using R2API;

namespace Faithful
{
    internal class Vengeance
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store buff
        Buff vengeanceBuff;

        // Store damage buff per stack
        public float damage = 0.75f;

        // Constructor
        public Vengeance(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create Vengeance buff
            vengeanceBuff = Buffs.AddBuff("VENGEANCE", "texbuffvengefulboost", Color.red);

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);

            // Add stats modification
            Behaviour.AddStatsMod(vengeanceBuff, VengeanceStatsMod);
        }

        void OnDamageDealt(DamageReport report)
        {
            // Check for attacker body
            CharacterBody attackerBody = report.attackerBody;
            if (attackerBody)
            {
                // Does attacker have Vengeance
                if (report.attackerBody.GetBuffCount(vengeanceBuff.buffDef) > 0)
                {
                    // Remove Vengeance
                    report.attackerBody.ClearTimedBuffs(vengeanceBuff.buffDef);
                }
            }
        }

        void VengeanceStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage multiplier
            _stats.damageMultAdd += damage * _count;
        }
    }
}
