using UnityEngine;
using RoR2;
using R2API;
using System.Collections.Generic;

namespace Faithful
{
    internal class GodMode
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store buff
        Buff godModeBuff;

        // Constructor
        public GodMode(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create God Mode buff
            godModeBuff = toolbox.buffs.AddBuff("DEBUG_MODE", "texbuffdevmode", Color.white, _canStack: false);

            // Link Holdout Zone behaviour
            toolbox.behaviour.AddInHoldoutZoneCallback(InHoldoutZone);

            // Link On Damage Dealt behaviour
            toolbox.behaviour.AddOnDamageDealtCallback(OnDamageDealt);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(godModeBuff, GodModeStatsMod);

            // Link update function
            toolbox.behaviour.AddUpdateCallback(Update, true);
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Debug mode behaviour
            if (toolbox.utils.debugMode)
            {
                // Is character in God Mode
                if (_body.GetBuffCount(godModeBuff.buffDef) > 0)
                {
                    // Instantly charge Holdout Zone
                    _zone.baseChargeDuration = 0.0f;
                }
            }
        }

        void OnDamageDealt(DamageReport report)
        {
            // Does victim have God Mode
            if (report.victimBody.GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Set health to max
                report.victim.health = report.victim.fullHealth;
            }
        }

        void GodModeStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // God Mode damage
            _stats.baseDamageAdd += float.PositiveInfinity;

            // 3x move speed
            _stats.moveSpeedMultAdd += 2.0f;

            // 1.5x jump power
            _stats.jumpPowerMultAdd += 0.5f;

            // God Mode cooldowns
            _stats.cooldownReductionAdd += float.PositiveInfinity;
        }

        void Update()
        {
            if (PlayerCharacterMasterController.instances.Count <= 0)
            {
                // No player character
                return;
            }

            // Get character body
            CharacterBody body = PlayerCharacterMasterController.instances[0].master.GetBody();

            if (!body)
            {
                // Player body not present
                return;
            }

            // Is F2 pressed - God mode
            if (Input.GetKeyDown(KeyCode.F2))
            {
                // Has god mode already?
                if (body.GetBuffCount(godModeBuff.buffDef) > 0)
                {
                    // Disable godmode
                    body.RemoveBuff(godModeBuff.buffDef);

                    // REMOVED UNTIL TESTING
                    //body.name = body.name.Replace("[GODMODE] ", "");   // Remove God Mode name

                    Log.Debug("God Mode disabled");
                }
                else
                {
                    // Enable godmode
                    body.AddBuff(godModeBuff.buffDef);

                    // REMOVED UNTIL TESTING
                    //body.name = "[GODMODE] " + body.name;   // Set God Mode name

                    Log.Debug("God Mode enabled!");
                }
            }

            // Has God Mode?
            if (body.GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Clear timed debuffs
                List<BuffIndex> buffClearList = [];
                foreach (CharacterBody.TimedBuff timed in body.timedBuffs)
                {
                    // Is debuff?
                    if (BuffCatalog.GetBuffDef(timed.buffIndex).isDebuff)
                    {
                        // Add to clear list
                        buffClearList.Add(timed.buffIndex);
                    }
                }
                foreach (BuffIndex buffIndex in buffClearList)
                {
                    // Clear debuffs
                    body.ClearTimedBuffs(buffIndex);
                }
            }
        }
    }
}
