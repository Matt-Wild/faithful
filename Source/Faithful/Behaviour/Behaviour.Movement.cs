using System.Collections.Generic;
using EntityStates;
using RoR2;

namespace Faithful
{
    internal delegate void OnProcessJumpCallback(GenericCharacterMain _character);

    // Return true to handle the operation without running the original method
    internal delegate bool ProcessJumpOverrideCallback(GenericCharacterMain _character);

    internal delegate bool ArtificerProcessJumpOverrideCallback(EntityStates.Mage.MageCharacterMain _character);

    internal delegate bool ArtificerJetpackFixedUpdateOverrideCallback(EntityStates.Mage.JetpackOn _state);

    internal delegate bool FallDamageImmunityCallback(GlobalEventManager _manager, CharacterBody _body);

    internal delegate void GenericCharacterCallback(GenericCharacterMain _character);

    internal delegate bool MapTeleportBodyCallback(MapZone self, CharacterBody characterBody);

    internal static partial class Behaviour
    {
        // Jumping: scoped overrides can handle the jump; normal callbacks run after the original method
        private static readonly List<OnProcessJumpCallback> onProcessJumpCallbacks = new List<OnProcessJumpCallback>();

        private static readonly CharacterScopedCallbacks<ProcessJumpOverrideCallback> processJumpOverrideCallbacks = new();

        private static readonly HookRegistration processJumpHook = new("EntityStates.GenericCharacterMain.ProcessJump",
            () => On.EntityStates.GenericCharacterMain.ProcessJump += HookProcessJump,
            () => On.EntityStates.GenericCharacterMain.ProcessJump -= HookProcessJump);

        // Add On Process Jump callback
        public static void AddOnProcessJumpCallback(OnProcessJumpCallback _callback)
        {
            onProcessJumpCallbacks.Add(_callback);

            DebugLog("Added On Process Jump behaviour");
        }

        // Register character-specific callbacks in execution order
        public static void AddProcessJumpOverrideCallback(CharacterBody _body, ProcessJumpOverrideCallback _callback)
        {
            processJumpOverrideCallbacks.Add(_body, _callback);
        }

        public static void RemoveProcessJumpOverrideCallback(CharacterBody _body, ProcessJumpOverrideCallback _callback)
        {
            processJumpOverrideCallbacks.Remove(_body, _callback);
        }

        private static void HookProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        {
            // Allow this character's callbacks to handle jumping
            if (CanRunHook(processJumpHook))
            {
                processJumpHook.StartProfiling();
                try
                {
                    foreach (ProcessJumpOverrideCallback callback in processJumpOverrideCallbacks.Get(self?.characterBody))
                    {
                        if (callback(self)) return;
                    }
                }
                finally
                {
                    processJumpHook.StopProfiling();
                }
            }

            orig(self); // Run normal processes first
            if (!CanRunHook(processJumpHook)) return;

            processJumpHook.StartProfiling();
            try
            {
                // Cycle through On Process Jump callbacks
                foreach (OnProcessJumpCallback callback in onProcessJumpCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                processJumpHook.StopProfiling();
            }
        }

        // Character states: scoped callbacks run before the original fixed update, normal callbacks after
        private static readonly List<GenericCharacterCallback> genericCharacterFixedUpdateCallbacks = new List<GenericCharacterCallback>();

        private static readonly CharacterScopedCallbacks<GenericCharacterCallback> characterPreFixedUpdateCallbacks = new();

        private static readonly HookRegistration genericCharacterFixedUpdateHook = new("EntityStates.GenericCharacterMain.FixedUpdate",
            () => On.EntityStates.GenericCharacterMain.FixedUpdate += HookGenericCharacterFixedUpdate,
            () => On.EntityStates.GenericCharacterMain.FixedUpdate -= HookGenericCharacterFixedUpdate);

        // Add generic character fixed update callback
        public static void AddGenericCharacterFixedUpdateCallback(GenericCharacterCallback _callback)
        {
            genericCharacterFixedUpdateCallbacks.Add(_callback);

            DebugLog("Added Generic Character Fixed Update behaviour");
        }

        public static void AddCharacterPreFixedUpdateCallback(CharacterBody _body, GenericCharacterCallback _callback)
        {
            characterPreFixedUpdateCallbacks.Add(_body, _callback);
        }

        public static void RemoveCharacterPreFixedUpdateCallback(CharacterBody _body, GenericCharacterCallback _callback)
        {
            characterPreFixedUpdateCallbacks.Remove(_body, _callback);
        }

        private static void HookGenericCharacterFixedUpdate(On.EntityStates.GenericCharacterMain.orig_FixedUpdate orig, EntityStates.GenericCharacterMain self)
        {
            // Run this character's callbacks before the normal state update
            if (CanRunHook(genericCharacterFixedUpdateHook))
            {
                genericCharacterFixedUpdateHook.StartProfiling();
                try
                {
                    foreach (GenericCharacterCallback callback in characterPreFixedUpdateCallbacks.Get(self?.characterBody))
                    {
                        callback(self);
                    }
                }
                finally
                {
                    genericCharacterFixedUpdateHook.StopProfiling();
                }
            }

            orig(self); // Run normal processes first
            if (!CanRunHook(genericCharacterFixedUpdateHook)) return;

            genericCharacterFixedUpdateHook.StartProfiling();
            try
            {
                // Cycle through Generic Character Fixed Update callbacks
                foreach (GenericCharacterCallback callback in genericCharacterFixedUpdateCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                genericCharacterFixedUpdateHook.StopProfiling();
            }
        }

        // Artificer jumping: a true callback result handles the operation
        private static readonly CharacterScopedCallbacks<ArtificerProcessJumpOverrideCallback> artificerProcessJumpOverrideCallbacks = new();

        private static readonly HookRegistration artificerProcessJumpHook = new("MageCharacterMain.ProcessJump",
            () => On.EntityStates.Mage.MageCharacterMain.ProcessJump += HookArtificerProcessJump,
            () => On.EntityStates.Mage.MageCharacterMain.ProcessJump -= HookArtificerProcessJump);

        public static void AddArtificerProcessJumpOverrideCallback(CharacterBody _body, ArtificerProcessJumpOverrideCallback _callback)
        {
            artificerProcessJumpOverrideCallbacks.Add(_body, _callback);
        }

        public static void RemoveArtificerProcessJumpOverrideCallback(CharacterBody _body, ArtificerProcessJumpOverrideCallback _callback)
        {
            artificerProcessJumpOverrideCallbacks.Remove(_body, _callback);
        }

        private static void HookArtificerProcessJump(On.EntityStates.Mage.MageCharacterMain.orig_ProcessJump orig, EntityStates.Mage.MageCharacterMain self)
        {
            if (CanRunHook(artificerProcessJumpHook))
            {
                artificerProcessJumpHook.StartProfiling();
                try
                {
                    foreach (ArtificerProcessJumpOverrideCallback callback in artificerProcessJumpOverrideCallbacks.Get(self?.characterBody))
                    {
                        if (callback(self)) return;
                    }
                }
                finally
                {
                    artificerProcessJumpHook.StopProfiling();
                }
            }

            orig(self); // Run normal processes
        }

        // Artificer jetpack: a true callback result handles the operation
        private static readonly CharacterScopedCallbacks<ArtificerJetpackFixedUpdateOverrideCallback> artificerJetpackFixedUpdateOverrideCallbacks = new();

        private static readonly HookRegistration artificerJetpackFixedUpdateHook = new("Mage.JetpackOn.FixedUpdate",
            () => On.EntityStates.Mage.JetpackOn.FixedUpdate += HookArtificerJetpackFixedUpdate,
            () => On.EntityStates.Mage.JetpackOn.FixedUpdate -= HookArtificerJetpackFixedUpdate);

        public static void AddArtificerJetpackFixedUpdateOverrideCallback(CharacterBody _body, ArtificerJetpackFixedUpdateOverrideCallback _callback)
        {
            artificerJetpackFixedUpdateOverrideCallbacks.Add(_body, _callback);
        }

        public static void RemoveArtificerJetpackFixedUpdateOverrideCallback(CharacterBody _body, ArtificerJetpackFixedUpdateOverrideCallback _callback)
        {
            artificerJetpackFixedUpdateOverrideCallbacks.Remove(_body, _callback);
        }

        private static void HookArtificerJetpackFixedUpdate(On.EntityStates.Mage.JetpackOn.orig_FixedUpdate orig, EntityStates.Mage.JetpackOn self)
        {
            if (CanRunHook(artificerJetpackFixedUpdateHook))
            {
                artificerJetpackFixedUpdateHook.StartProfiling();
                try
                {
                    foreach (ArtificerJetpackFixedUpdateOverrideCallback callback in artificerJetpackFixedUpdateOverrideCallbacks.Get(self?.characterBody))
                    {
                        if (callback(self)) return;
                    }
                }
                finally
                {
                    artificerJetpackFixedUpdateHook.StopProfiling();
                }
            }

            orig(self); // Run normal processes
        }

        // Fall damage: a true callback result grants immunity without running the original check
        private static readonly CharacterScopedCallbacks<FallDamageImmunityCallback> fallDamageImmunityCallbacks = new();

        private static readonly HookRegistration immuneToFallDamageHook = new("GlobalEventManager.IsImmuneToFallDamage",
            () => On.RoR2.GlobalEventManager.IsImmuneToFallDamage += HookIsImmuneToFallDamage,
            () => On.RoR2.GlobalEventManager.IsImmuneToFallDamage -= HookIsImmuneToFallDamage);

        public static void AddFallDamageImmunityCallback(CharacterBody _body, FallDamageImmunityCallback _callback)
        {
            fallDamageImmunityCallbacks.Add(_body, _callback);
        }

        public static void RemoveFallDamageImmunityCallback(CharacterBody _body, FallDamageImmunityCallback _callback)
        {
            fallDamageImmunityCallbacks.Remove(_body, _callback);
        }

        private static bool HookIsImmuneToFallDamage(On.RoR2.GlobalEventManager.orig_IsImmuneToFallDamage orig, GlobalEventManager self, CharacterBody body)
        {
            if (CanRunHook(immuneToFallDamageHook) && self != null)
            {
                immuneToFallDamageHook.StartProfiling();
                try
                {
                    foreach (FallDamageImmunityCallback callback in fallDamageImmunityCallbacks.Get(body))
                    {
                        if (callback(self, body)) return true;
                    }
                }
                finally
                {
                    immuneToFallDamageHook.StopProfiling();
                }
            }

            return orig(self, body);
        }

        // Map teleportation: a false callback result prevents the original teleport
        private static readonly List<MapTeleportBodyCallback> mapTeleportBodyCallbacks = new List<MapTeleportBodyCallback>();

        private static readonly HookRegistration mapTeleportBodyHook = new("RoR2.MapZone.TeleportBody",
            () => On.RoR2.MapZone.TeleportBody += HookMapTeleportBody,
            () => On.RoR2.MapZone.TeleportBody -= HookMapTeleportBody);

        public static void AddMapTeleportBodyCallback(MapTeleportBodyCallback _callback)
        {
            mapTeleportBodyCallbacks.Add(_callback);

            DebugLog("Added MapTeleportBody behaviour");
        }

        private static void HookMapTeleportBody(On.RoR2.MapZone.orig_TeleportBody orig, MapZone self, CharacterBody characterBody)
        {
            if (CanRunHook(mapTeleportBodyHook))
            {
                mapTeleportBodyHook.StartProfiling();
                try
                {
                    // A callback can prevent teleportation - otherwise run normal processes
                    foreach (MapTeleportBodyCallback callback in mapTeleportBodyCallbacks)
                    {
                        if (!callback(self, characterBody)) return;
                    }
                }
                finally
                {
                    mapTeleportBodyHook.StopProfiling();
                }
            }
            orig(self, characterBody);
        }
    }
}
