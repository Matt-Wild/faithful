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

            // Link On Incoming Damage behaviour
            toolbox.behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);

            // Link On Purchase Interaction behaviour
            toolbox.behaviour.AddOnPurchaseInteractionBeginCallback(OnPurchaseInteractionBegin);
            toolbox.behaviour.AddOnPurchaseCanBeAffordedCallback(OnPurchaseCanBeAfforded);

            // Add stats modification
            toolbox.behaviour.AddStatsMod(godModeBuff, GodModeStatsMod);

            // Add On Heal behaviour
            toolbox.behaviour.AddOnHealCallback(OnHeal);

            // Link update function
            toolbox.behaviour.AddUpdateCallback(Update, true);

            // Add On Get User Name behaviour
            On.RoR2.CharacterBody.GetUserName += OnGetUsername;

            // Override On Map Zone Teleport Body
            On.RoR2.MapZone.TeleportBody += OnMapTeleportBody;
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
                    toolbox.utils.ChargeHoldoutZone(_zone);
                }
            }
        }

        void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check for victim
            if (_victim == null)
            {
                return;
            }

            // Check for victim body
            if (_victim.hasBody)
            {
                // Get victim body
                CharacterBody victimBody = _victim.GetBody();

                // Does victim have God Mode
                if (victimBody.GetBuffCount(godModeBuff.buffDef) > 0)
                {
                    // Modify report to avoid damage and knockback
                    _report.rejected = true;
                    _report.canRejectForce = true;
                    _report.damage = 0.0f;
                    _report.force = new Vector3();
                }
            }
        }

        void OnPurchaseInteractionBegin(PurchaseInteraction _shop, CharacterMaster _activator)
        {
            // Check for body
            if (!_activator.hasBody)
            {
                return;
            }

            // Check for God Mode
            if (_activator.GetBody().GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Set cost of shop to 0
                _shop.cost = 0;
            }
        }

        bool OnPurchaseCanBeAfforded(PurchaseInteraction _shop, CharacterMaster _activator)
        {
            // Check for body
            if (!_activator.hasBody)
            {
                return false;
            }

            // Check for God Mode
            if (_activator.GetBody().GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Force afforded
                return true;
            }

            // Otherwise return false
            return false;
        }

        void GodModeStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // God Mode damage
            _stats.baseDamageAdd += float.PositiveInfinity;

            // God Mode attack speed
            _stats.attackSpeedMultAdd += 3.0f;

            // God Mode regen
            _stats.baseRegenAdd += 9999999.9f;

            // 3x move speed
            _stats.moveSpeedMultAdd += 2.0f;

            // 1.5x jump power
            _stats.jumpPowerMultAdd += 0.5f;

            // God Mode cooldowns
            _stats.cooldownReductionAdd += float.PositiveInfinity;
        }

        void OnHeal(HealthComponent _healthComponent, ref float _amount, ref ProcChainMask _procChainMask, ref bool _nonRegen)
        {
            // Attempt to get Character Body
            CharacterBody body = _healthComponent.gameObject.GetComponent<CharacterBody>();
            if (body != null)
            {
                // Check for buff
                if (body.GetBuffCount(godModeBuff.buffDef) > 0)
                {
                    // Apply God Mode healing
                    _amount = float.PositiveInfinity;
                }
            }
        }

        void Update()
        {
            // Host only
            if (!toolbox.utils.hosting)
            {
                return;
            }

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
        }

        string OnGetUsername(On.RoR2.CharacterBody.orig_GetUserName orig, CharacterBody self)
        {
            // Check for buff
            if (self.GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Add "Godly" to username
                return "Godly " + orig(self);
            }

            // Otherwise ignore behaviour
            return orig(self);
        }

        void OnMapTeleportBody(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
        {
            // Check for buff
            if (characterBody.GetBuffCount(godModeBuff.buffDef) > 0)
            {
                // Ignore behaviour
                return;
            }

            // Otherwise run normal processes
            orig(self, characterBody);
        }
    }
}
