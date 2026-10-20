using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal delegate void OnIncomingDamageCallback(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim);

    internal delegate void OnHitEnemyCallback(DamageInfo _info, GameObject _victim);

    internal delegate void OnTakeDamageProcessCallback(HealthComponent _healthComponent, DamageInfo _info);

    internal delegate void DamageReportCallback(DamageReport _report);

    internal delegate void OnHealCallback(HealthComponent _healthComponent, ref float _amount, ref ProcChainMask _procChainMask, ref bool _nonRegen);

    internal delegate void HuntressSeekingArrowEnterCallback(EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self);

    internal delegate void DroneHeadbuttEnterCallback(EntityStates.Drone.Command.Headbutt self);

    internal static partial class Behaviour
    {
        // Health component helper: added before the original awake method
        private static readonly HookRegistration healthComponentAwakeHook = new("HealthComponent.Awake",
            () => On.RoR2.HealthComponent.Awake += HookHealthComponentAwake,
            () => On.RoR2.HealthComponent.Awake -= HookHealthComponentAwake);

        private static void HookHealthComponentAwake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            if (!CanRunHook(healthComponentAwakeHook))
            {
                orig(self);
                return;
            }

            healthComponentAwakeHook.StartProfiling();
            try
            {
                // Add custom health component behaviour
                FaithfulHealthComponentBehaviour component = self.gameObject.AddComponent<FaithfulHealthComponentBehaviour>();
            }
            finally
            {
                healthComponentAwakeHook.StopProfiling();
            }

            orig(self); // Run normal processes
        }

        // Healing: callbacks can change arguments before the original method
        private static readonly List<OnHealCallback> onHealCallbacks = new List<OnHealCallback>();

        private static readonly HookRegistration healHook = new("HealthComponent.Heal",
            () => On.RoR2.HealthComponent.Heal += HookHeal,
            () => On.RoR2.HealthComponent.Heal -= HookHeal);

        // Add On Heal callback
        public static void AddOnHealCallback(OnHealCallback _callback)
        {
            onHealCallbacks.Add(_callback);

            DebugLog("Added On Heal behaviour");
        }

        private static float HookHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if (!CanRunHook(healHook)) return orig(self, amount, procChainMask, nonRegen);

            healHook.StartProfiling();
            try
            {
                // Cycle through On Heal callbacks
                foreach (OnHealCallback callback in onHealCallbacks)
                {
                    // Call
                    callback(self, ref amount, ref procChainMask, ref nonRegen);
                }
            }
            finally
            {
                healHook.StopProfiling();
            }

            return orig(self, amount, procChainMask, nonRegen); // Run normal processes
        }

        // Incoming damage reported by the health component helper
        private static readonly List<OnIncomingDamageCallback> onIncomingDamageCallbacks = new List<OnIncomingDamageCallback>();

        // Add On Incoming Damage callback
        public static void AddOnIncomingDamageCallback(OnIncomingDamageCallback _callback)
        {
            onIncomingDamageCallbacks.Add(_callback);

            DebugLog("Added On Incoming Damage behaviour");
        }

        public static void OnIncomingDamageServer(DamageInfo _damageInfo, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Cycle through On Incoming Damage callbacks
            foreach (OnIncomingDamageCallback callback in onIncomingDamageCallbacks)
            {
                // Call
                callback(_damageInfo, _attacker, _victim);
            }
        }

        // Damage processing: normal callbacks run before the original method, late callbacks after
        private static readonly List<OnTakeDamageProcessCallback> onTakeDamageProcessCallbacks = new List<OnTakeDamageProcessCallback>();

        private static readonly List<OnTakeDamageProcessCallback> onTakeDamageProcessLateCallbacks = new List<OnTakeDamageProcessCallback>();

        private static readonly HookRegistration takeDamageProcessHook = new("HealthComponent.TakeDamageProcess",
            () => On.RoR2.HealthComponent.TakeDamageProcess += HookTakeDamageProcess,
            () => On.RoR2.HealthComponent.TakeDamageProcess -= HookTakeDamageProcess);

        // Add On Take Damage Process callback
        public static void AddOnTakeDamageProcessCallback(OnTakeDamageProcessCallback _callback)
        {
            onTakeDamageProcessCallbacks.Add(_callback);

            DebugLog("Added On Take Damage Process behaviour");
        }

        // Add On Take Damage Process Late callback
        public static void AddOnTakeDamageProcessLateCallback(OnTakeDamageProcessCallback _callback)
        {
            onTakeDamageProcessLateCallbacks.Add(_callback);

            DebugLog("Added On Take Damage Process Late behaviour");
        }

        private static void HookTakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (!CanRunHook(takeDamageProcessHook))
            {
                orig(self, damageInfo);
                return;
            }

            takeDamageProcessHook.StartProfiling();
            try
            {
                // Cycle through On Take Damage Process callbacks
                foreach (OnTakeDamageProcessCallback callback in onTakeDamageProcessCallbacks)
                {
                    // Call
                    callback(self, damageInfo);
                }
            }
            finally
            {
                takeDamageProcessHook.StopProfiling();
            }

            orig(self, damageInfo); // Run normal processes

            takeDamageProcessHook.StartProfiling();
            try
            {
                // Cycle through On Take Damage Process Late callbacks
                foreach (OnTakeDamageProcessCallback callback in onTakeDamageProcessLateCallbacks)
                {
                    // Call
                    callback(self, damageInfo);
                }
            }
            finally
            {
                takeDamageProcessHook.StopProfiling();
            }
        }

        // Enemy hits: callbacks run after the original method
        private static readonly List<OnHitEnemyCallback> onHitEnemyLateCallbacks = new List<OnHitEnemyCallback>();

        private static readonly HookRegistration onHitEnemyHook = new("GlobalEventManager.OnHitEnemy",
            () => On.RoR2.GlobalEventManager.OnHitEnemy += HookOnHitEnemy,
            () => On.RoR2.GlobalEventManager.OnHitEnemy -= HookOnHitEnemy);

        // Add On Hit Enemy Late callback
        public static void AddOnHitEnemyLateCallback(OnHitEnemyCallback _callback)
        {
            onHitEnemyLateCallbacks.Add(_callback);

            DebugLog("Added On Hit Enemy Late behaviour");
        }

        private static void HookOnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim); // Run normal processes
            if (!CanRunHook(onHitEnemyHook)) return;

            onHitEnemyHook.StartProfiling();
            try
            {
                // Cycle through On Hit Enemy Late callbacks
                foreach (OnHitEnemyCallback callback in onHitEnemyLateCallbacks)
                {
                    // Call
                    callback(damageInfo, victim);
                }
            }
            finally
            {
                onHitEnemyHook.StopProfiling();
            }
        }

        // Server damage reports
        private static readonly List<DamageReportCallback> onDamageDealtCallbacks = new List<DamageReportCallback>();

        private static readonly HookRegistration onDamageDealtHook = new("GlobalEventManager.onServerDamageDealt",
            () => GlobalEventManager.onServerDamageDealt += HookOnDamageDealt,
            () => GlobalEventManager.onServerDamageDealt -= HookOnDamageDealt);

        // Add On Damage Dealt callback
        public static void AddOnDamageDealtCallback(DamageReportCallback _callback)
        {
            onDamageDealtCallbacks.Add(_callback);

            DebugLog("Added On Damage Dealt behaviour");
        }

        private static void HookOnDamageDealt(DamageReport _report)
        {
            if (!CanRunHook(onDamageDealtHook)) return;

            onDamageDealtHook.StartProfiling();
            try
            {
                // Cycle through On Damage Dealt callbacks
                foreach (DamageReportCallback callback in onDamageDealtCallbacks)
                {
                    // Call
                    callback(_report);
                }
            }
            finally
            {
                onDamageDealtHook.StopProfiling();
            }
        }

        // Character death reports
        private static readonly List<DamageReportCallback> onCharacterDeathCallbacks = new List<DamageReportCallback>();

        private static readonly HookRegistration onCharacterDeathHook = new("GlobalEventManager.onCharacterDeathGlobal",
            () => GlobalEventManager.onCharacterDeathGlobal += HookOnCharacterDeath,
            () => GlobalEventManager.onCharacterDeathGlobal -= HookOnCharacterDeath);

        // Add On Character Death callback
        public static void AddOnCharacterDeathCallback(DamageReportCallback _callback)
        {
            onCharacterDeathCallbacks.Add(_callback);

            DebugLog("Added On Character Death behaviour");
        }

        private static void HookOnCharacterDeath(DamageReport _report)
        {
            if (!CanRunHook(onCharacterDeathHook)) return;

            onCharacterDeathHook.StartProfiling();
            try
            {
                // Cycle through On Character Death callbacks
                foreach (DamageReportCallback callback in onCharacterDeathCallbacks)
                {
                    // Call
                    callback(_report);
                }
            }
            finally
            {
                onCharacterDeathHook.StopProfiling();
            }
        }

        // Crit overrides: consume a pending override before rolling normally
        private static readonly Dictionary<CharacterBody, int> critOverrideCounts = new Dictionary<CharacterBody, int>();

        private static readonly HookRegistration characterBodyRollCritHook = new("CharacterBody.RollCrit",
            () => On.RoR2.CharacterBody.RollCrit += HookCharacterBodyRollCrit,
            () => On.RoR2.CharacterBody.RollCrit -= HookCharacterBodyRollCrit);

        public static void OverrideCritCheck(CharacterBody _body, int _count = 1)
        {
            // Check for existing entry
            if (critOverrideCounts.ContainsKey(_body))
            {
                // Increase count
                critOverrideCounts[_body] += _count;
            }
            else
            {
                // Add new entry
                critOverrideCounts.Add(_body, _count);
            }
        }

        private static bool HookCharacterBodyRollCrit(On.RoR2.CharacterBody.orig_RollCrit orig, CharacterBody self)
        {
            if (!CanRunHook(characterBodyRollCritHook)) return orig(self);

            characterBodyRollCritHook.StartProfiling();
            try
            {
                // Check for override
                if (critOverrideCounts.ContainsKey(self))
                {
                    // Get override count
                    int overrideCount = critOverrideCounts[self];

                    // Check for available overrides
                    if (overrideCount > 0)
                    {
                        // Decrease override count
                        critOverrideCounts[self] = overrideCount - 1;

                        // Return true for crit
                        return true;
                    }
                }
            }
            finally
            {
                characterBodyRollCritHook.StopProfiling();
            }

            // Return normal roll crit result
            return orig(self);
        }

        // Huntress seeking arrow: callbacks run after the original state entry
        private static readonly List<HuntressSeekingArrowEnterCallback> huntressSeekingArrowEnterCallbacks = new List<HuntressSeekingArrowEnterCallback>();

        private static readonly HookRegistration huntressSeekingArrowEnterHook = new("EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter",
            () => On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += HookHuntressSeekingArrowEnter,
            () => On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter -= HookHuntressSeekingArrowEnter);

        public static void AddHuntressSeekingArrowEnterCallback(HuntressSeekingArrowEnterCallback _callback)
        {
            huntressSeekingArrowEnterCallbacks.Add(_callback);

            DebugLog("Added HuntressSeekingArrowEnter behaviour");
        }

        private static void HookHuntressSeekingArrowEnter(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnEnter orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(huntressSeekingArrowEnterHook)) return;

            huntressSeekingArrowEnterHook.StartProfiling();
            try
            {
                // Cycle through HuntressSeekingArrowEnter callbacks
                foreach (HuntressSeekingArrowEnterCallback callback in huntressSeekingArrowEnterCallbacks)
                {
                    callback(self);
                }
            }
            finally
            {
                huntressSeekingArrowEnterHook.StopProfiling();
            }
        }

        // Drone headbutt: callbacks run before the original state entry
        private static readonly List<DroneHeadbuttEnterCallback> droneHeadbuttEnterCallbacks = new List<DroneHeadbuttEnterCallback>();

        private static readonly HookRegistration droneHeadbuttEnterHook = new("EntityStates.Drone.Command.Headbutt.OnEnter",
            () => On.EntityStates.Drone.Command.Headbutt.OnEnter += HookDroneHeadbuttEnter,
            () => On.EntityStates.Drone.Command.Headbutt.OnEnter -= HookDroneHeadbuttEnter);

        public static void AddDroneHeadbuttEnterCallback(DroneHeadbuttEnterCallback _callback)
        {
            droneHeadbuttEnterCallbacks.Add(_callback);

            DebugLog("Added DroneHeadbuttEnter behaviour");
        }

        private static void HookDroneHeadbuttEnter(On.EntityStates.Drone.Command.Headbutt.orig_OnEnter orig, EntityStates.Drone.Command.Headbutt self)
        {
            if (CanRunHook(droneHeadbuttEnterHook))
            {
                droneHeadbuttEnterHook.StartProfiling();
                try
                {
                    // Run callbacks before the state calculates its attack
                    foreach (DroneHeadbuttEnterCallback callback in droneHeadbuttEnterCallbacks)
                    {
                        callback(self);
                    }
                }
                finally
                {
                    droneHeadbuttEnterHook.StopProfiling();
                }
            }

            orig(self); // Run normal processes
        }
    }
}
