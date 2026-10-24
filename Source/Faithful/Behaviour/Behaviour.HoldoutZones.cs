using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using RoR2;

namespace Faithful
{
    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);

    internal delegate void OnHoldoutZoneStartCallback(HoldoutZoneController _zone);

    internal delegate void OnHoldoutZoneCalcRadiusCallback(ref float _radius, HoldoutZoneController _zone);

    internal delegate void OnSupportBeaconStartCallback(SupportBeaconController _beacon);

    internal delegate void OnHoldoutZoneCalcFixedRadiusCallback(ref float _radius, HoldoutZoneController _zone);

    internal static partial class Behaviour
    {
        // Holdout zone updates: callbacks run for contained characters before the original method
        private static readonly List<InHoldoutZoneCallback> inHoldoutZoneCallbacks = new List<InHoldoutZoneCallback>();

        private static readonly HookRegistration holdoutZoneControllerUpdateHook = new("HoldoutZoneController.Update",
            () => On.RoR2.HoldoutZoneController.Update += HookHoldoutZoneControllerUpdate,
            () => On.RoR2.HoldoutZoneController.Update -= HookHoldoutZoneControllerUpdate);

        // Add in Holdout Zone callback
        public static void AddInHoldoutZoneCallback(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            DebugLog("Added in Holdout Zone behaviour");
        }

        private static void HookHoldoutZoneControllerUpdate(On.RoR2.HoldoutZoneController.orig_Update orig, HoldoutZoneController self)
        {
            if (!CanRunHook(holdoutZoneControllerUpdateHook))
            {
                orig(self);
                return;
            }

            holdoutZoneControllerUpdateHook.StartProfiling();
            try
            {
                // Only scan the zone when callbacks need the results
                if (inHoldoutZoneCallbacks.Count > 0)
                {
                    // Keep a snapshot in case callbacks change the character collection
                    List<CharacterBody> characterBodies = HG.CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
                    try
                    {
                        foreach (CharacterBody current in CharacterBody.readOnlyInstancesList)
                        {
                            if (self.IsBodyInChargingRadius(current)) characterBodies.Add(current);
                        }

                        foreach (CharacterBody current in characterBodies)
                        {
                            foreach (InHoldoutZoneCallback callback in inHoldoutZoneCallbacks)
                            {
                                callback(current, self);
                            }
                        }
                    }
                    finally
                    {
                        HG.CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(characterBodies);
                    }
                }
            }
            finally
            {
                holdoutZoneControllerUpdateHook.StopProfiling();
            }

            orig(self); // Run normal processes
        }

        // Holdout zone start: set up the radius helper and callbacks before the original method
        private static readonly List<OnHoldoutZoneStartCallback> onHoldoutZoneStartCallbacks = new List<OnHoldoutZoneStartCallback>();

        private static readonly List<OnHoldoutZoneCalcRadiusCallback> onHoldoutZoneCalcRadiusCallbacks = new List<OnHoldoutZoneCalcRadiusCallback>();

        private static readonly HookRegistration holdoutZoneControllerStartHook = new("HoldoutZoneController.Start",
            () => On.RoR2.HoldoutZoneController.Start += HookHoldoutZoneControllerStart,
            () => On.RoR2.HoldoutZoneController.Start -= HookHoldoutZoneControllerStart);

        // Add on Holdout Zone start callback
        public static void AddOnHoldoutZoneStartCallback(OnHoldoutZoneStartCallback _callback)
        {
            onHoldoutZoneStartCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone start behaviour");
        }

        // Add on Holdout Zone calc radius callback
        public static void AddOnHoldoutZoneCalcRadiusCallback(OnHoldoutZoneCalcRadiusCallback _callback)
        {
            onHoldoutZoneCalcRadiusCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone calc radius behaviour");
        }

        private static void HookHoldoutZoneControllerStart(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            if (!CanRunHook(holdoutZoneControllerStartHook))
            {
                orig(self);
                return;
            }

            holdoutZoneControllerStartHook.StartProfiling();
            try
            {
                // Ensure the helper exists regardless of beacon and holdout Start order
                EnsureHoldoutZoneBehaviour(self);

                // Cycle through OnHoldoutZoneStart callbacks
                foreach (OnHoldoutZoneStartCallback callback in onHoldoutZoneStartCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                holdoutZoneControllerStartHook.StopProfiling();
            }

            orig(self); // Run normal processes
        }

        // Fixed holdout radius: callbacks adjust the target without changing the base radius
        private static readonly List<OnHoldoutZoneCalcFixedRadiusCallback> onHoldoutZoneCalcFixedRadiusCallbacks = new List<OnHoldoutZoneCalcFixedRadiusCallback>();

        private static readonly HookRegistration holdoutZoneFixedRadiusHook = new("HoldoutZoneController.DoUpdate (fixed radius)",
            () => IL.RoR2.HoldoutZoneController.DoUpdate += HookHoldoutZoneFixedRadius,
            () => IL.RoR2.HoldoutZoneController.DoUpdate -= HookHoldoutZoneFixedRadius);

        // Add on Holdout Zone calc fixed radius callback
        public static void AddOnHoldoutZoneCalcFixedRadiusCallback(OnHoldoutZoneCalcFixedRadiusCallback _callback)
        {
            onHoldoutZoneCalcFixedRadiusCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone calc fixed radius behaviour");
        }

        private static void HookHoldoutZoneFixedRadius(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Adjust the local radius before the game notifies calcRadius listeners and updates coverage
            if (!cursor.TryGotoNext(MoveType.After,
                instruction => instruction.MatchLdarg(0),
                instruction => instruction.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.baseRadius))))
            {
                throw new InvalidOperationException("Could not find the base radius read in HoldoutZoneController.DoUpdate.");
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, HoldoutZoneController, float>>(CalcHoldoutZoneFixedRadius);
        }

        private static float CalcHoldoutZoneFixedRadius(float _radius, HoldoutZoneController _zone)
        {
            // Ordinary zones keep the game's normal radius modifier path
            if (!CanRunHook(holdoutZoneFixedRadiusHook) || !_zone.ignoreRadiusModifiers) return _radius;

            holdoutZoneFixedRadiusHook.StartProfiling();
            try
            {
                foreach (OnHoldoutZoneCalcFixedRadiusCallback callback in onHoldoutZoneCalcFixedRadiusCallbacks)
                {
                    callback(ref _radius, _zone);
                }
                return _radius;
            }
            finally
            {
                holdoutZoneFixedRadiusHook.StopProfiling();
            }
        }

        // The beacon reads baseRadius directly during its initial coverage window
        private static ILHook supportBeaconInitialRadiusILHook;

        private static readonly HookRegistration supportBeaconInitialRadiusHook = new("SupportBeaconController.currentRadius (initial radius)",
            InstallSupportBeaconInitialRadiusHook,
            () =>
            {
                supportBeaconInitialRadiusILHook?.Dispose();
                supportBeaconInitialRadiusILHook = null;
            });

        // Keep the initial and ongoing beacon radius paths available together
        internal static bool supportBeaconRadiusAvailable => supportBeaconStartAvailable && CanRunHook(holdoutZoneFixedRadiusHook) && CanRunHook(supportBeaconInitialRadiusHook);

        private static void InstallSupportBeaconInitialRadiusHook()
        {
            // MMHOOK does not expose this property getter
            MethodInfo getter = typeof(SupportBeaconController).GetProperty("currentRadius", BindingFlags.Instance | BindingFlags.NonPublic)?.GetGetMethod(true);
            if (getter == null) throw new InvalidOperationException("Could not find SupportBeaconController.currentRadius.");

            supportBeaconInitialRadiusILHook = new ILHook(getter, HookSupportBeaconInitialRadius);
        }

        private static void HookSupportBeaconInitialRadius(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Replace only the initial base radius read and retain the later currentRadius path
            if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.baseRadius))))
            {
                throw new InvalidOperationException("Could not find the initial base radius read in SupportBeaconController.currentRadius.");
            }

            cursor.Remove();
            cursor.EmitDelegate<Func<HoldoutZoneController, float>>(GetSupportBeaconInitialRadius);
        }

        private static float GetSupportBeaconInitialRadius(HoldoutZoneController _zone)
        {
            if (!CanRunHook(supportBeaconInitialRadiusHook)) return _zone.baseRadius;

            supportBeaconInitialRadiusHook.StartProfiling();
            try
            {
                return CalcHoldoutZoneFixedRadius(_zone.baseRadius, _zone);
            }
            finally
            {
                supportBeaconInitialRadiusHook.StopProfiling();
            }
        }

        // Support beacon start: callbacks run before the original method
        private static readonly List<OnSupportBeaconStartCallback> onSupportBeaconStartCallbacks = new List<OnSupportBeaconStartCallback>();

        private static readonly HookRegistration supportBeaconControllerStartHook = new("SupportBeaconController.Start",
            () => On.RoR2.SupportBeaconController.Start += HookSupportBeaconControllerStart,
            () => On.RoR2.SupportBeaconController.Start -= HookSupportBeaconControllerStart);

        internal static bool supportBeaconStartAvailable => CanRunHook(supportBeaconControllerStartHook);

        // Add on Support Beacon start callback
        public static void AddOnSupportBeaconStartCallback(OnSupportBeaconStartCallback _callback)
        {
            onSupportBeaconStartCallbacks.Add(_callback);

            DebugLog("Added on Support Beacon start behaviour");
        }

        private static void HookSupportBeaconControllerStart(On.RoR2.SupportBeaconController.orig_Start orig, SupportBeaconController self)
        {
            if (!CanRunHook(supportBeaconControllerStartHook))
            {
                orig(self);
                return;
            }

            supportBeaconControllerStartHook.StartProfiling();
            try
            {
                // Cycle through OnSupportBeaconStart callbacks
                foreach (OnSupportBeaconStartCallback callback in onSupportBeaconStartCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                supportBeaconControllerStartHook.StopProfiling();
            }

            orig(self); // Run normal processes
        }

        internal static FaithfulHoldoutZoneBehaviour EnsureHoldoutZoneBehaviour(HoldoutZoneController _zone)
        {
            // Create and initialise the holdout helper only when needed
            FaithfulHoldoutZoneBehaviour behaviour = _zone.GetComponent<FaithfulHoldoutZoneBehaviour>();
            if (behaviour == null)
            {
                behaviour = _zone.gameObject.AddComponent<FaithfulHoldoutZoneBehaviour>();
                behaviour.Init(onHoldoutZoneCalcRadiusCallbacks);
            }
            return behaviour;
        }
    }
}
