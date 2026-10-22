using System.Collections.Generic;
using RoR2;

namespace Faithful
{
    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);

    internal delegate void OnHoldoutZoneStartCallback(HoldoutZoneController _zone);

    internal delegate void OnHoldoutZoneCalcRadiusCallback(ref float _radius, HoldoutZoneController _zone);

    internal delegate void OnSupportBeaconStartCallback(SupportBeaconController _beacon);

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
