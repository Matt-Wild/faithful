using System.Collections.Generic;
using RoR2;
using UnityEngine;
using MonoMod.Cil;

namespace Faithful
{
    internal delegate void OnAddBuffCallback(BuffIndex _buff, CharacterBody _character);

    internal delegate void OnAddTimedBuffCallback(BuffDef _buff, ref float _duration, CharacterBody _character);

    internal delegate void OnAddTimedBuffMaxStacksCallback(BuffDef _buff, ref float _duration, ref int _maxStacks, CharacterBody _character);

    internal delegate void OnExtendTimedBuffsCallback(BuffDef _buff, ref float _duration, ref float _max, CharacterBody _character);

    internal delegate void OnSetTimedBuffDurationCallback(BuffDef _buff, ref float _duration, bool _allStacks, CharacterBody _character);

    internal delegate void OnInflictDamageOverTimeCallback(GameObject _victimObject, GameObject _attackerObject, DotController.DotIndex _dotIndex, float _duration, float _damageMultiplier, uint? _maxStacksFromAttacker);

    internal delegate void OnInflictDamageOverTimeRefCallback(ref InflictDotInfo _inflictDotInfo);

    internal delegate void CharacterBodyBuffCallback(CharacterBody _body, BuffDef _buffDef);

    internal static partial class Behaviour
    {
        // Buff addition: callbacks run before the original method
        private static readonly List<OnAddBuffCallback> onAddBuffCallbacks = new List<OnAddBuffCallback>();

        private static readonly HookRegistration addBuffIndexHook = new("CharacterBody.AddBuff_BuffIndex",
            () => On.RoR2.CharacterBody.AddBuff_BuffIndex += HookAddBuffIndex,
            () => On.RoR2.CharacterBody.AddBuff_BuffIndex -= HookAddBuffIndex);

        // Add On Add Buff callback
        public static void AddOnAddBuffCallback(OnAddBuffCallback _callback)
        {
            onAddBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Buff behaviour");
        }

        private static void HookAddBuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            if (!CanRunHook(addBuffIndexHook))
            {
                orig(self, buffType);
                return;
            }

            addBuffIndexHook.StartProfiling();
            try
            {
                // Cycle through On Add Buff callbacks
                foreach (OnAddBuffCallback callback in onAddBuffCallbacks)
                {
                    // Call
                    callback(buffType, self);
                }
            }
            finally
            {
                addBuffIndexHook.StopProfiling();
            }

            orig(self, buffType); // Run normal processes
        }

        // Timed buffs: callbacks can change duration before the original method
        private static readonly List<OnAddTimedBuffCallback> onAddTimedBuffCallbacks = new List<OnAddTimedBuffCallback>();

        private static readonly HookRegistration addTimedBuffDefHook = new("CharacterBody.AddTimedBuff_BuffDef_float",
            () => On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += HookAddTimedBuffDef,
            () => On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= HookAddTimedBuffDef);

        // Add On Add Timed Buff callback
        public static void AddOnAddTimedBuffCallback(OnAddTimedBuffCallback _callback)
        {
            onAddTimedBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Timed Buff behaviour");
        }

        private static void HookAddTimedBuffDef(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (!CanRunHook(addTimedBuffDefHook))
            {
                orig(self, buffDef, duration);
                return;
            }

            addTimedBuffDefHook.StartProfiling();
            try
            {
                // Cycle through On Add Timed Buff callbacks
                foreach (OnAddTimedBuffCallback callback in onAddTimedBuffCallbacks)
                {
                    // Call
                    callback(buffDef, ref duration, self);
                }
            }
            finally
            {
                addTimedBuffDefHook.StopProfiling();
            }

            orig(self, buffDef, duration); // Run normal processes
        }

        // Limited timed buffs: callbacks can change duration and maximum stacks before the original method
        private static readonly List<OnAddTimedBuffMaxStacksCallback> onAddTimedBuffMaxStacksCallbacks = new List<OnAddTimedBuffMaxStacksCallback>();

        private static readonly HookRegistration addTimedBuffDefMaxStacksHook = new("CharacterBody.AddTimedBuff_BuffDef_float_int",
            () => On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int += HookAddTimedBuffDefMaxStacks,
            () => On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float_int -= HookAddTimedBuffDefMaxStacks);

        // Add On Add Timed Buff Max Stacks callback
        public static void AddOnAddTimedBuffMaxStacksCallback(OnAddTimedBuffMaxStacksCallback _callback)
        {
            onAddTimedBuffMaxStacksCallbacks.Add(_callback);

            DebugLog("Added On Add Timed Buff Max Stacks behaviour");
        }

        private static void HookAddTimedBuffDefMaxStacks(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float_int orig, CharacterBody self, BuffDef buffDef, float duration, int maxStacks)
        {
            if (!CanRunHook(addTimedBuffDefMaxStacksHook))
            {
                orig(self, buffDef, duration, maxStacks);
                return;
            }

            addTimedBuffDefMaxStacksHook.StartProfiling();
            try
            {
                // Cycle through On Add Timed Buff Max Stacks callbacks
                foreach (OnAddTimedBuffMaxStacksCallback callback in onAddTimedBuffMaxStacksCallbacks)
                {
                    // Call
                    callback(buffDef, ref duration, ref maxStacks, self);
                }
            }
            finally
            {
                addTimedBuffDefMaxStacksHook.StopProfiling();
            }

            orig(self, buffDef, duration, maxStacks); // Run normal processes
        }

        // Extend timed buffs: callbacks can change duration and maximum before the original method
        private static readonly List<OnExtendTimedBuffsCallback> onExtendTimedBuffsCallbacks = new List<OnExtendTimedBuffsCallback>();

        private static readonly HookRegistration extendTimedBuffsHook = new("CharacterBody.ExtendTimedBuffIfPresent_BuffDef_float_float",
            () => On.RoR2.CharacterBody.ExtendTimedBuffIfPresent_BuffDef_float_float += HookExtendTimedBuffs,
            () => On.RoR2.CharacterBody.ExtendTimedBuffIfPresent_BuffDef_float_float -= HookExtendTimedBuffs);

        // Add On Extend Timed Buffs callback
        public static void AddOnExtendTimedBuffsCallback(OnExtendTimedBuffsCallback _callback)
        {
            onExtendTimedBuffsCallbacks.Add(_callback);

            DebugLog("Added On Extend Timed Buffs behaviour");
        }

        private static void HookExtendTimedBuffs(On.RoR2.CharacterBody.orig_ExtendTimedBuffIfPresent_BuffDef_float_float orig, CharacterBody self, BuffDef buffDef, float duration, float max)
        {
            if (!CanRunHook(extendTimedBuffsHook))
            {
                orig(self, buffDef, duration, max);
                return;
            }

            extendTimedBuffsHook.StartProfiling();
            try
            {
                // Cycle through On Extend Timed Buffs callbacks
                foreach (OnExtendTimedBuffsCallback callback in onExtendTimedBuffsCallbacks)
                {
                    // Call
                    callback(buffDef, ref duration, ref max, self);
                }
            }
            finally
            {
                extendTimedBuffsHook.StopProfiling();
            }

            orig(self, buffDef, duration, max); // Run normal processes
        }

        // Set timed buff duration: callbacks can change duration before the original method
        private static readonly List<OnSetTimedBuffDurationCallback> onSetTimedBuffDurationCallbacks = new List<OnSetTimedBuffDurationCallback>();

        private static readonly HookRegistration setTimedBuffDurationHook = new("CharacterBody.SetTimedBuffDurationIfPresent",
            () => On.RoR2.CharacterBody.SetTimedBuffDurationIfPresent += HookSetTimedBuffDuration,
            () => On.RoR2.CharacterBody.SetTimedBuffDurationIfPresent -= HookSetTimedBuffDuration);

        // Add On Set Timed Buff Duration callback
        public static void AddOnSetTimedBuffDurationCallback(OnSetTimedBuffDurationCallback _callback)
        {
            onSetTimedBuffDurationCallbacks.Add(_callback);

            DebugLog("Added On Set Timed Buff Duration behaviour");
        }

        private static void HookSetTimedBuffDuration(On.RoR2.CharacterBody.orig_SetTimedBuffDurationIfPresent orig, CharacterBody self, BuffDef buffDef, float duration, bool allStacks)
        {
            if (!CanRunHook(setTimedBuffDurationHook))
            {
                orig(self, buffDef, duration, allStacks);
                return;
            }

            setTimedBuffDurationHook.StartProfiling();
            try
            {
                // Cycle through On Set Timed Buff Duration callbacks
                foreach (OnSetTimedBuffDurationCallback callback in onSetTimedBuffDurationCallbacks)
                {
                    // Call
                    callback(buffDef, ref duration, allStacks, self);
                }
            }
            finally
            {
                setTimedBuffDurationHook.StopProfiling();
            }

            orig(self, buffDef, duration, allStacks); // Run normal processes
        }

        // First buff stack: callbacks run after the original method
        private static readonly List<CharacterBodyBuffCallback> onBuffFirstStackGainedCallbacks = new List<CharacterBodyBuffCallback>();

        private static readonly HookRegistration buffFirstStackGainedHook = new("CharacterBody.OnBuffFirstStackGained",
            () => On.RoR2.CharacterBody.OnBuffFirstStackGained += HookBuffFirstStackGained,
            () => On.RoR2.CharacterBody.OnBuffFirstStackGained -= HookBuffFirstStackGained);

        public static void AddOnBuffFirstStackGainedCallback(CharacterBodyBuffCallback _callback)
        {
            onBuffFirstStackGainedCallbacks.Add(_callback);

            DebugLog("Added CharacterBody.OnBuffFirstStackGained behaviour");
        }

        private static void HookBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef); // Run normal processes
            if (!CanRunHook(buffFirstStackGainedHook)) return;

            buffFirstStackGainedHook.StartProfiling();
            try
            {
                // Cycle through CharacterBody.OnBuffFirstStackGained callbacks
                foreach (CharacterBodyBuffCallback callback in onBuffFirstStackGainedCallbacks)
                {
                    // Call
                    callback(self, buffDef);
                }
            }
            finally
            {
                buffFirstStackGainedHook.StopProfiling();
            }
        }

        // Final buff stack: callbacks run after the original method
        private static readonly List<CharacterBodyBuffCallback> onBuffFinalStackLostCallbacks = new List<CharacterBodyBuffCallback>();

        private static readonly HookRegistration buffFinalStackLostHook = new("CharacterBody.OnBuffFinalStackLost",
            () => On.RoR2.CharacterBody.OnBuffFinalStackLost += HookBuffFinalStackLost,
            () => On.RoR2.CharacterBody.OnBuffFinalStackLost -= HookBuffFinalStackLost);

        public static void AddOnBuffFinalStackLostCallback(CharacterBodyBuffCallback _callback)
        {
            onBuffFinalStackLostCallbacks.Add(_callback);

            DebugLog("Added CharacterBody.OnBuffFinalStackLost behaviour");
        }

        private static void HookBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef); // Run normal processes
            if (!CanRunHook(buffFinalStackLostHook)) return;

            buffFinalStackLostHook.StartProfiling();
            try
            {
                // Cycle through CharacterBody.OnBuffFinalStackLost callbacks
                foreach (CharacterBodyBuffCallback callback in onBuffFinalStackLostCallbacks)
                {
                    // Call
                    callback(self, buffDef);
                }
            }
            finally
            {
                buffFinalStackLostHook.StopProfiling();
            }
        }

        // Damage over time: value callbacks run after the original method
        private static readonly List<OnInflictDamageOverTimeCallback> onInflictDamageOverTimeCallbacks = new List<OnInflictDamageOverTimeCallback>();

        private static readonly HookRegistration inflictDamageOverTimeHook = new("DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1",
            () => On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += HookInflictDamageOverTime,
            () => On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 -= HookInflictDamageOverTime);

        // Add On Inflict Damage Over Time callback
        public static void AddOnInflictDamageOverTimeCallback(OnInflictDamageOverTimeCallback _callback)
        {
            onInflictDamageOverTimeCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time behaviour");
        }

        private static void HookInflictDamageOverTime(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker); // Run normal processes
            if (!CanRunHook(inflictDamageOverTimeHook)) return;

            inflictDamageOverTimeHook.StartProfiling();
            try
            {
                // Cycle through On Inflict Damage Over Time callbacks
                foreach (OnInflictDamageOverTimeCallback callback in onInflictDamageOverTimeCallbacks)
                {
                    // Call
                    callback(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);
                }
            }
            finally
            {
                inflictDamageOverTimeHook.StopProfiling();
            }
        }

        // Damage over time by reference: callbacks run before the original method
        private static readonly List<OnInflictDamageOverTimeRefCallback> onInflictDamageOverTimeRefCallbacks = new List<OnInflictDamageOverTimeRefCallback>();

        private static readonly HookRegistration inflictDamageOverTimeRefHook = new("DotController.InflictDot_refInflictDotInfo",
            () => On.RoR2.DotController.InflictDot_refInflictDotInfo += HookInflictDamageOverTimeRef,
            () => On.RoR2.DotController.InflictDot_refInflictDotInfo -= HookInflictDamageOverTimeRef);

        // Add On Inflict Damage Over Time Ref callback
        public static void AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRefCallback _callback)
        {
            onInflictDamageOverTimeRefCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time Ref behaviour");
        }

        private static void HookInflictDamageOverTimeRef(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            if (!CanRunHook(inflictDamageOverTimeRefHook))
            {
                orig(ref inflictDotInfo);
                return;
            }

            inflictDamageOverTimeRefHook.StartProfiling();
            try
            {
                // Cycle through On Inflict Damage Over Time Ref callbacks
                foreach (OnInflictDamageOverTimeRefCallback callback in onInflictDamageOverTimeRefCallbacks)
                {
                    // Call
                    callback(ref inflictDotInfo);
                }
            }
            finally
            {
                inflictDamageOverTimeRefHook.StopProfiling();
            }

            orig(ref inflictDotInfo); // Run normal processes
        }

        // Buff display values: replace the UI count read without changing the buff stacks
        private static readonly HookRegistration buffDisplayValueHook = new("BuffDisplay.AllocateIcons (display values)",
            () => IL.RoR2.UI.BuffDisplay.AllocateIcons += HookBuffDisplayValues,
            () => IL.RoR2.UI.BuffDisplay.AllocateIcons -= HookBuffDisplayValues);

        internal static bool buffDisplayValuesAvailable => CanRunHook(buffDisplayValueHook);

        private static void HookBuffDisplayValues(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Replace only the count read used by the buff display's normal change detection
            if (!ILHelper.TryGotoNext(cursor, "BuffDisplay.AllocateIcons", MoveType.Before,
                instruction => ILHelper.MatchMethodCall(instruction, "RoR2.CharacterBody", "GetBuffCount")
                    && instruction.Operand is Mono.Cecil.MethodReference method
                    && method.Parameters.Count == 1 && method.Parameters[0].ParameterType.FullName == "RoR2.BuffIndex"))
            {
                throw new System.InvalidOperationException("Could not find the buff display count read.");
            }

            cursor.Remove();
            cursor.EmitDelegate<System.Func<CharacterBody, BuffIndex, int>>(GetBuffDisplayValue);
        }

        private static int GetBuffDisplayValue(CharacterBody _body, BuffIndex _buffIndex)
        {
            buffDisplayValueHook.StartProfiling();
            try
            {
                return Utils.GetBuffDisplayValue(_body, _buffIndex);
            }
            finally
            {
                buffDisplayValueHook.StopProfiling();
            }
        }
    }
}
