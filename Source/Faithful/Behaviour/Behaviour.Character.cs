using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal delegate void StatsModCallback(int _count, RecalculateStatsAPI.StatHookEventArgs _stats);

    internal delegate void CharacterBodyCallback(CharacterBody _body);

    internal delegate void CharacterModelCallback(CharacterModel _model);

    internal delegate string GetUserNameCallback(CharacterBody self, string _name);

    internal static partial class Behaviour
    {
        // Character body helper
        internal static GameObject characterBodyHelperPrefab;

        private static void CreatePrefabs()
        {
            // We create an empty gameobject to hold all the networked components
            var tempGO = new GameObject("temp GO");

            // Add the NetworkIdentity so that Unity knows which Object it's going to be networking all about
            tempGO.AddComponent<NetworkIdentity>();

            // Use InstantiateClone from the PrefabAPI to make sure we have full control over the GameObject
            characterBodyHelperPrefab = tempGO.InstantiateClone("faithfulCharacterBodyHelper");

            // Delete the now useless temporary GameObject
            Object.Destroy(tempGO);

            // Add a specific components (which can be networked)
            characterBodyHelperPrefab.AddComponent<FaithfulCharacterBodyBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulTJetpackBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulTargetingMatrixBehaviour>();
        }

        // Character body awake: callbacks run after the original method
        private static readonly List<CharacterBodyCallback> onCharacterBodyAwakeCallbacks = new List<CharacterBodyCallback>();

        private static readonly HookRegistration characterBodyAwakeHook = new("CharacterBody.Awake",
            () => On.RoR2.CharacterBody.Awake += HookCharacterBodyAwake,
            () => On.RoR2.CharacterBody.Awake -= HookCharacterBodyAwake);

        // Add On Character Body Awake callback
        public static void AddOnCharacterBodyAwakeCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyAwakeCallbacks.Add(_callback);

            DebugLog("Added On Character Body Awake behaviour");
        }

        private static void HookCharacterBodyAwake(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(characterBodyAwakeHook)) return;

            characterBodyAwakeHook.StartProfiling();
            try
            {
                // Cycle through On Character Body Awake callbacks
                foreach (CharacterBodyCallback callback in onCharacterBodyAwakeCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                characterBodyAwakeHook.StopProfiling();
            }
        }

        // Character body start: create the helper before the original method, then run callbacks
        private static readonly List<CharacterBodyCallback> onCharacterBodyStartCallbacks = new List<CharacterBodyCallback>();

        private static readonly Dictionary<float, CharacterBodyCallback> onCharacterBodyTickCallbacks = new Dictionary<float, CharacterBodyCallback>();

        private static readonly HookRegistration characterBodyStartHook = new("CharacterBody.Start",
            () => On.RoR2.CharacterBody.Start += HookCharacterBodyStart,
            () => On.RoR2.CharacterBody.Start -= HookCharacterBodyStart);

        // Add On Character Body Start callback
        public static void AddOnCharacterBodyStartCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyStartCallbacks.Add(_callback);

            DebugLog("Added On Character Body Start behaviour");
        }

        public static void AddOnCharacterBodyTickCallback(float _tickRate, CharacterBodyCallback _callback)
        {
            onCharacterBodyTickCallbacks.Add(_tickRate, _callback);

            DebugLog("Added On Character Body Tick behaviour");
        }

        private static void HookCharacterBodyStart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            if (!CanRunHook(characterBodyStartHook))
            {
                orig(self);
                return;
            }

            characterBodyStartHook.StartProfiling();
            try
            {
                //Log.Message($"Character body start for character '{self.name}' with netID {self.GetComponent<NetworkIdentity>().netId}");

                // Check if server and valid character
                if (NetworkServer.active && self.master != null)
                {
                    // Add character body behaviour object
                    GameObject characterBodyBehaviourObj = Object.Instantiate(characterBodyHelperPrefab);

                    // Get character body behaviour
                    FaithfulCharacterBodyBehaviour characterBodyBehaviour = characterBodyBehaviourObj.GetComponent<FaithfulCharacterBodyBehaviour>();

                    // Link the character body ID
                    characterBodyBehaviour.characterID = self.GetComponent<NetworkIdentity>().netId;

                    // Setup on tick callbacks
                    foreach (KeyValuePair<float, CharacterBodyCallback> entry in onCharacterBodyTickCallbacks) characterBodyBehaviour.AddTickBehaviour(entry.Value, entry.Key);

                    // Include the body ID in the initial spawn and link directly on the server
                    NetworkServer.Spawn(characterBodyBehaviourObj);
                    characterBodyBehaviour.AssignCharacter(self);
                }
            }
            finally
            {
                characterBodyStartHook.StopProfiling();
            }

            orig(self); // Run normal processes

            characterBodyStartHook.StartProfiling();
            try
            {
                // Cycle through On Character Body Start callbacks
                foreach (CharacterBodyCallback callback in onCharacterBodyStartCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                characterBodyStartHook.StopProfiling();
            }
        }

        // Character body fixed update: callbacks run after the original method and helper update
        private static readonly List<CharacterBodyCallback> onCharacterBodyFixedUpdateCallbacks = new List<CharacterBodyCallback>();

        private static readonly HookRegistration characterBodyFixedUpdateHook = new("CharacterBody.FixedUpdate",
            () => On.RoR2.CharacterBody.FixedUpdate += HookCharacterBodyFixedUpdate,
            () => On.RoR2.CharacterBody.FixedUpdate -= HookCharacterBodyFixedUpdate);

        public static void AddOnCharacterBodyFixedUpdateCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyFixedUpdateCallbacks.Add(_callback);

            DebugLog("Added On Character Body Fixed Update behaviour");
        }

        public static void RemoveOnCharacterBodyFixedUpdateCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyFixedUpdateCallbacks.Remove(_callback);

            DebugLog("Removed On Character Body Fixed Update behaviour");
        }

        private static void HookCharacterBodyFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(characterBodyFixedUpdateHook)) return;

            characterBodyFixedUpdateHook.StartProfiling();
            try
            {
                // Update only this character's behaviours after its normal physics update
                FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(self, _warnIfMissing: false);
                if (helper != null) helper.OnCharacterBodyFixedUpdate();

                // Cycle through callbacks
                foreach (CharacterBodyCallback callback in onCharacterBodyFixedUpdateCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                characterBodyFixedUpdateHook.StopProfiling();
            }
        }

        // Item and buff stat coefficients
        private static readonly List<ItemStatsMod> itemStatsMods = new List<ItemStatsMod>();

        private static readonly List<BuffStatsMod> buffStatsMods = new List<BuffStatsMod>();

        private static readonly HookRegistration statsModHook = new("RecalculateStatsAPI.GetStatCoefficients",
            () => RecalculateStatsAPI.GetStatCoefficients += HookStatsMod,
            () => RecalculateStatsAPI.GetStatCoefficients -= HookStatsMod);

        // Add item stats mod
        public static void AddStatsMod(Item _item, StatsModCallback _callback)
        {
            // Add item stats mod
            itemStatsMods.Add(new ItemStatsMod(_item, _callback));

            DebugLog($"Added stat mods for '{_item.token}' item");
        }

        // Add buff stats mod
        public static void AddStatsMod(Buff _buff, StatsModCallback _callback)
        {
            // Add buff stats mod
            buffStatsMods.Add(new BuffStatsMod(_buff, _callback));

            DebugLog($"Added stat mods for '{_buff.token}' buff");
        }

        // Do stat modifications
        private static void HookStatsMod(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            if (!CanRunHook(statsModHook)) return;

            statsModHook.StartProfiling();
            try
            {
                // Cycle through item stats mod callbacks
                foreach (ItemStatsMod itemStatsMod in itemStatsMods)
                {
                    // Process item stats mod
                    itemStatsMod.Process(_body, _stats);
                }

                // Cycle through buff stats mod callbacks
                foreach (BuffStatsMod buffStatsMod in buffStatsMods)
                {
                    // Process buff stats mod
                    buffStatsMod.Process(_body, _stats);
                }
            }
            finally
            {
                statsModHook.StopProfiling();
            }
        }

        // Recalculate stats: callbacks run after the original method
        private static readonly List<CharacterBodyCallback> onRecalculateStatsCallbacks = new List<CharacterBodyCallback>();

        private static readonly HookRegistration recalculateStatsHook = new("CharacterBody.RecalculateStats",
            () => On.RoR2.CharacterBody.RecalculateStats += HookRecalculateStats,
            () => On.RoR2.CharacterBody.RecalculateStats -= HookRecalculateStats);

        // Add On Recalculate Stats callback
        public static void AddOnRecalculateStatsCallback(CharacterBodyCallback _callback)
        {
            onRecalculateStatsCallbacks.Add(_callback);

            DebugLog("Added On Recalculate Stats behaviour");
        }

        private static void HookRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(recalculateStatsHook)) return;

            recalculateStatsHook.StartProfiling();
            try
            {
                // Cycle through On Recalculate Stats callbacks
                foreach (CharacterBodyCallback callback in onRecalculateStatsCallbacks)
                {
                    // Call
                    callback(self);
                }

                // Update all temporary visual effects (again)
                self.UpdateAllTemporaryVisualEffects();
            }
            finally
            {
                recalculateStatsHook.StopProfiling();
            }
        }

        // Character level changes during stat calculation
        private static readonly HookRegistration characterLevelHook = new("CharacterBody.RecalculateStats (character level)",
            () => IL.RoR2.CharacterBody.RecalculateStats += HookCharacterLevel,
            () => IL.RoR2.CharacterBody.RecalculateStats -= HookCharacterLevel);

        internal static bool characterLevelChangesAvailable => CanRunHook(characterLevelHook);

        private static void HookCharacterLevel(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Find the level multiplier after team, ambient and bonus levels have been applied
            if (!cursor.TryGotoNext(MoveType.Before,
                instruction => instruction.MatchLdarg(0),
                instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_level"),
                instruction => instruction.MatchLdcR4(1.0f),
                instruction => instruction.MatchSub()))
            {
                throw new System.InvalidOperationException("Could not find the character level multiplier in RecalculateStats.");
            }

            // Adjust the level before vanilla calculates stats and handles level-up effects
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<System.Action<CharacterBody>>(ApplyCharacterLevelOffset);
        }

        private static void ApplyCharacterLevelOffset(CharacterBody _body)
        {
            characterLevelHook.StartProfiling();
            try
            {
                Utils.ApplyCharacterLevelOffset(_body);
            }
            finally
            {
                characterLevelHook.StopProfiling();
            }
        }

        // Model overlays: callbacks run after the original method
        private static readonly List<CharacterModelCallback> onUpdateOverlaysCallbacks = new List<CharacterModelCallback>();

        private static readonly HookRegistration updateOverlaysHook = new("CharacterModel.UpdateOverlays",
            () => On.RoR2.CharacterModel.UpdateOverlays += HookUpdateOverlays,
            () => On.RoR2.CharacterModel.UpdateOverlays -= HookUpdateOverlays);

        public static void AddOnUpdateOverlaysCallback(CharacterModelCallback _callback)
        {
            onUpdateOverlaysCallbacks.Add(_callback);

            DebugLog("Added CharacterModel.UpdateOverlays behaviour");
        }

        private static void HookUpdateOverlays(On.RoR2.CharacterModel.orig_UpdateOverlays orig, CharacterModel self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(updateOverlaysHook)) return;

            updateOverlaysHook.StartProfiling();
            try
            {
                // Cycle through CharacterModel.UpdateOverlays callbacks
                foreach (CharacterModelCallback callback in onUpdateOverlaysCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                updateOverlaysHook.StopProfiling();
            }
        }

        // Visual effects: callbacks run after the original method
        private static readonly List<CharacterBodyCallback> onUpdateVisualEffectsCallbacks = new List<CharacterBodyCallback>();

        private static readonly HookRegistration updateVisualEffectsHook = new("CharacterBody.UpdateAllTemporaryVisualEffects",
            () => On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += HookUpdateVisualEffects,
            () => On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects -= HookUpdateVisualEffects);

        // Add On Update Visual Effects callback
        public static void AddOnUpdateVisualEffectsCallback(CharacterBodyCallback _callback)
        {
            onUpdateVisualEffectsCallbacks.Add(_callback);

            DebugLog("Added On Update Visual Effects behaviour");
        }

        private static void HookUpdateVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(updateVisualEffectsHook)) return;

            updateVisualEffectsHook.StartProfiling();
            try
            {
                // Cycle through On Update Visual Effects callbacks
                foreach (CharacterBodyCallback callback in onUpdateVisualEffectsCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                updateVisualEffectsHook.StopProfiling();
            }
        }

        // User names: callbacks modify the result of the original method in registration order
        private static readonly List<GetUserNameCallback> getUserNameCallbacks = new List<GetUserNameCallback>();

        private static readonly HookRegistration getUserNameHook = new("RoR2.CharacterBody.GetUserName",
            () => On.RoR2.CharacterBody.GetUserName += HookGetUserName,
            () => On.RoR2.CharacterBody.GetUserName -= HookGetUserName);

        // Add user name callback
        public static void AddGetUserNameCallback(GetUserNameCallback _callback)
        {
            getUserNameCallbacks.Add(_callback);

            DebugLog("Added GetUserName behaviour");
        }

        private static string HookGetUserName(On.RoR2.CharacterBody.orig_GetUserName orig, CharacterBody self)
        {
            string result = orig(self);
            if (CanRunHook(getUserNameHook))
            {
                getUserNameHook.StartProfiling();
                try
                {
                    foreach (GetUserNameCallback callback in getUserNameCallbacks)
                    {
                        result = callback(self, result);
                    }
                }
                finally
                {
                    getUserNameHook.StopProfiling();
                }
            }
            return result;
        }
    }
}
