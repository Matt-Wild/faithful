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

    internal delegate void BodyStatsModCallback(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats);

    internal delegate void CharacterBodyCallback(CharacterBody _body);

    internal delegate void ModifyShieldFromHealthCallback(CharacterBody _body, float _maxHealth, ref float _shield);

    internal delegate void ModifyShieldRechargeDelayCallback(CharacterBody _body, float _minimumDelay, ref float _delay);

    internal delegate bool CanPlayShieldRechargeSoundCallback(CharacterBody _body);

    internal delegate void ModifyHealthBarValuesCallback(HealthComponent _healthComponent, ref HealthComponent.HealthBarValues _values);

    internal delegate void ModifyVoidShieldVisualsCallback(CharacterBody _body, ref bool _hasVoidShields);

    internal delegate void CharacterModelCallback(CharacterModel _model);

    internal delegate string GetUserNameCallback(CharacterBody self, string _name);

    internal static partial class Behaviour
    {
        // Character body helper
        internal static GameObject characterBodyHelperPrefab;

        // Features currently requiring state context for target searches
        private static readonly HashSet<object> stateSearchContextUsers = new();

        public static void SetStateSearchContextRequired(object _source, bool _required)
        {
            if (_source == null) return;
            if (_required) stateSearchContextUsers.Add(_source);
            else stateSearchContextUsers.Remove(_source);
        }

        private static bool ShouldTrackStateSearchContext()
        {
            // Finish nested scopes correctly even if the last user stops during an outer call
            return Utils.hosting && (stateSearchContextUsers.Count > 0 || !ReferenceEquals(currentStateMachine, null) || currentTargetSearchContext != null);
        }

        // Character whose state is currently executing on this thread
        [System.ThreadStatic]
        private static CharacterBody currentStateCharacter;

        [System.ThreadStatic]
        private static EntityStateMachine currentStateMachine;

        private static readonly HookRegistration stateMachineUpdateHook = new("EntityStateMachine.ManagedUpdate",
            () => On.RoR2.EntityStateMachine.ManagedUpdate += HookStateMachineUpdate,
            () => On.RoR2.EntityStateMachine.ManagedUpdate -= HookStateMachineUpdate);

        private static readonly HookRegistration stateMachineFixedUpdateHook = new("EntityStateMachine.ManagedFixedUpdate",
            () => On.RoR2.EntityStateMachine.ManagedFixedUpdate += HookStateMachineFixedUpdate,
            () => On.RoR2.EntityStateMachine.ManagedFixedUpdate -= HookStateMachineFixedUpdate);

        private static readonly HookRegistration stateMachineSetStateHook = new("EntityStateMachine.SetState",
            () => On.RoR2.EntityStateMachine.SetState += HookStateMachineSetState,
            () => On.RoR2.EntityStateMachine.SetState -= HookStateMachineSetState);

        private static readonly HookRegistration stateMachineDestroyHook = new("EntityStateMachine.OnDestroy",
            () => On.RoR2.EntityStateMachine.OnDestroy += HookStateMachineDestroy,
            () => On.RoR2.EntityStateMachine.OnDestroy -= HookStateMachineDestroy);

        private static void HookStateMachineUpdate(On.RoR2.EntityStateMachine.orig_ManagedUpdate orig, EntityStateMachine self)
        {
            if (!CanRunHook(stateMachineUpdateHook) || !ShouldTrackStateSearchContext())
            {
                orig(self);
                return;
            }

            // Nested state execution must restore the previous character afterwards
            stateMachineUpdateHook.StartProfiling();
            CharacterBody previous = currentStateCharacter;
            EntityStateMachine previousMachine = currentStateMachine;
            try
            {
                currentStateCharacter = self.commonComponents.characterBody;
                currentStateMachine = self;

                // Measure our context changes without including the original method
                stateMachineUpdateHook.StopProfiling();
                try
                {
                    orig(self);
                }
                finally
                {
                    stateMachineUpdateHook.StartProfiling();
                }
            }
            finally
            {
                currentStateCharacter = previous;
                currentStateMachine = previousMachine;
                stateMachineUpdateHook.StopProfiling();
            }
        }

        private static void HookStateMachineFixedUpdate(On.RoR2.EntityStateMachine.orig_ManagedFixedUpdate orig, EntityStateMachine self, float deltaTime)
        {
            if (!CanRunHook(stateMachineFixedUpdateHook) || !ShouldTrackStateSearchContext())
            {
                orig(self, deltaTime);
                return;
            }

            stateMachineFixedUpdateHook.StartProfiling();
            CharacterBody previous = currentStateCharacter;
            EntityStateMachine previousMachine = currentStateMachine;
            try
            {
                currentStateCharacter = self.commonComponents.characterBody;
                currentStateMachine = self;

                // Measure our context changes without including the original method
                stateMachineFixedUpdateHook.StopProfiling();
                try
                {
                    orig(self, deltaTime);
                }
                finally
                {
                    stateMachineFixedUpdateHook.StartProfiling();
                }
            }
            finally
            {
                currentStateCharacter = previous;
                currentStateMachine = previousMachine;
                stateMachineFixedUpdateHook.StopProfiling();
            }
        }

        private static void HookStateMachineSetState(On.RoR2.EntityStateMachine.orig_SetState orig, EntityStateMachine self, EntityStates.EntityState newState)
        {
            if (!CanRunHook(stateMachineSetStateHook) || !ShouldTrackStateSearchContext())
            {
                orig(self, newState);
                return;
            }

            // SetState runs both the old state's OnExit and the new state's OnEnter
            stateMachineSetStateHook.StartProfiling();
            CharacterBody previous = currentStateCharacter;
            EntityStateMachine previousMachine = currentStateMachine;
            try
            {
                currentStateCharacter = self.commonComponents.characterBody;
                currentStateMachine = self;

                // Measure our context changes without including the original method
                stateMachineSetStateHook.StopProfiling();
                try
                {
                    orig(self, newState);
                }
                finally
                {
                    stateMachineSetStateHook.StartProfiling();
                }
            }
            finally
            {
                currentStateCharacter = previous;
                currentStateMachine = previousMachine;
                stateMachineSetStateHook.StopProfiling();
            }
        }

        private static void HookStateMachineDestroy(On.RoR2.EntityStateMachine.orig_OnDestroy orig, EntityStateMachine self)
        {
            if (!CanRunHook(stateMachineDestroyHook) || !ShouldTrackStateSearchContext())
            {
                orig(self);
                return;
            }

            // Destruction can also execute the current state's OnExit
            stateMachineDestroyHook.StartProfiling();
            CharacterBody previous = currentStateCharacter;
            EntityStateMachine previousMachine = currentStateMachine;
            try
            {
                currentStateCharacter = self.commonComponents.characterBody;
                currentStateMachine = self;

                // Measure our context changes without including the original method
                stateMachineDestroyHook.StopProfiling();
                try
                {
                    orig(self);
                }
                finally
                {
                    stateMachineDestroyHook.StartProfiling();
                }
            }
            finally
            {
                currentStateCharacter = previous;
                currentStateMachine = previousMachine;
                stateMachineDestroyHook.StopProfiling();
            }
        }

        // Character body destruction
        private static readonly List<CharacterBodyCallback> onBodyDestroyedCallbacks = new();
        private static readonly HookRegistration bodyDestroyedHook = new("CharacterBody.onBodyDestroyGlobal",
            () => CharacterBody.onBodyDestroyGlobal += HookBodyDestroyed,
            () => CharacterBody.onBodyDestroyGlobal -= HookBodyDestroyed);

        public static void AddOnBodyDestroyedCallback(CharacterBodyCallback _callback)
        {
            onBodyDestroyedCallbacks.Add(_callback);
            DebugLog("Added Character Body Destroyed behaviour");
        }

        private static void HookBodyDestroyed(CharacterBody _body)
        {
            if (!CanRunHook(bodyDestroyedHook)) return;

            bodyDestroyedHook.StartProfiling();
            try
            {
                foreach (CharacterBodyCallback callback in onBodyDestroyedCallbacks)
                {
                    callback(_body);
                }
            }
            finally
            {
                bodyDestroyedHook.StopProfiling();
            }
        }

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

        private static readonly List<BodyStatsModCallback> bodyStatsModCallbacks = new List<BodyStatsModCallback>();

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

        // Add character body stats mod
        public static void AddStatsMod(BodyStatsModCallback _callback)
        {
            bodyStatsModCallbacks.Add(_callback);

            DebugLog("Added character body stat mods");
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

                // Cycle through character body stats mod callbacks
                foreach (BodyStatsModCallback callback in bodyStatsModCallbacks)
                {
                    callback(_body, _stats);
                }
            }
            finally
            {
                statsModHook.StopProfiling();
            }
        }

        // Shield additions based on the calculated maximum health
        private static readonly List<ModifyShieldFromHealthCallback> modifyShieldFromHealthCallbacks = new();

        private static readonly HookRegistration shieldFromHealthHook = new("CharacterBody.RecalculateStats (shield from health)",
            () => IL.RoR2.CharacterBody.RecalculateStats += HookShieldFromHealth,
            () => IL.RoR2.CharacterBody.RecalculateStats -= HookShieldFromHealth);

        public static void AddModifyShieldFromHealthCallback(ModifyShieldFromHealthCallback _callback)
        {
            modifyShieldFromHealthCallbacks.Add(_callback);
            DebugLog("Added Modify Shield From Health behaviour");
        }

        private static void HookShieldFromHealth(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Find PSG's shield addition after maximum health has been calculated
            if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld<CharacterBody>("baseMaxShield")) ||
                !cursor.TryGotoNext(MoveType.After,
                    instruction => instruction.MatchLdarg(0),
                    instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_maxHealth"),
                    instruction => instruction.MatchMul(),
                    instruction => instruction.MatchAdd()))
            {
                throw new System.InvalidOperationException("Could not find the health-based shield addition in RecalculateStats.");
            }

            // Modify the shield value before it is stored, ahead of health conversion and shield change handling
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ModifyShieldFromHealth);
        }

        private static float ModifyShieldFromHealth(float _shield, CharacterBody _body)
        {
            if (!CanRunHook(shieldFromHealthHook) || modifyShieldFromHealthCallbacks.Count == 0 || !_body) return _shield;

            shieldFromHealthHook.StartProfiling();
            try
            {
                float maxHealth = _body.maxHealth;
                foreach (ModifyShieldFromHealthCallback callback in modifyShieldFromHealthCallbacks)
                {
                    callback(_body, maxHealth, ref _shield);
                }

                return _shield;
            }
            finally
            {
                shieldFromHealthHook.StopProfiling();
            }
        }

        // Shield delays shared by MoreStats and Faithful
        internal const float MinimumShieldRechargeDelay = 1.0f;
        private static readonly List<ModifyShieldRechargeDelayCallback> modifyShieldRechargeDelayCallbacks = new();

        private static readonly bool useMoreStatsForShieldRechargeDelay = MoreStatsCompatBootstrap.IsAvailable;

        private static readonly HookRegistration shieldRechargeDelayHook = new("Shield recharge delay",
            EnableShieldRechargeDelay,
            DisableShieldRechargeDelay);

        private static void EnableShieldRechargeDelay()
        {
            // Use one route for the shared callbacks
            if (useMoreStatsForShieldRechargeDelay) MoreStatsCompatBootstrap.TryInit();
            else IL.RoR2.HealthComponent.ServerFixedUpdate += HookShieldRechargeDelay;
        }

        private static void DisableShieldRechargeDelay()
        {
            if (useMoreStatsForShieldRechargeDelay) MoreStatsCompatBootstrap.TryShutdown();
            else IL.RoR2.HealthComponent.ServerFixedUpdate -= HookShieldRechargeDelay;
        }

        private static void HookShieldRechargeDelay(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Find the out-of-danger check used for shield recharging
            if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_maxShield")) ||
                !cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_outOfDanger")))
            {
                throw new System.InvalidOperationException("Could not find the shield recharge check in ServerFixedUpdate.");
            }

            // Change only this check, keeping forced regeneration and the shield break guard
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ModifyShieldRechargeReady);
        }

        private static bool ModifyShieldRechargeReady(bool _outOfDanger, HealthComponent _healthComponent)
        {
            if (!CanRunHook(shieldRechargeDelayHook) || modifyShieldRechargeDelayCallbacks.Count == 0) return _outOfDanger;

            CharacterBody body = _healthComponent.body;
            if (!body || _healthComponent.shield >= body.maxShield) return _outOfDanger;

            float baseDelay = CharacterBody.outOfDangerDelay;
            float delay = baseDelay;
            ModifyShieldRechargeDelay(body, MinimumShieldRechargeDelay, ref delay);
            if (delay == baseDelay) return _outOfDanger;

            // Reuse the damage stopwatch without changing the shared out-of-danger state
            return body.outOfDangerStopwatch >= delay;
        }

        public static void AddModifyShieldRechargeDelayCallback(ModifyShieldRechargeDelayCallback _callback)
        {
            modifyShieldRechargeDelayCallbacks.Add(_callback);
            DebugLog("Added Modify Shield Recharge Delay behaviour");
        }

        internal static void ModifyShieldRechargeDelay(CharacterBody _body, float _minimumDelay, ref float _delay)
        {
            if (!CanRunHook(shieldRechargeDelayHook) || !_body || modifyShieldRechargeDelayCallbacks.Count == 0) return;

            FaithfulRuntimeProfiler.StartScope("Behaviour.ModifyShieldRechargeDelay");
            try
            {
                float originalDelay = _delay;
                foreach (ModifyShieldRechargeDelayCallback callback in modifyShieldRechargeDelayCallbacks)
                {
                    callback(_body, _minimumDelay, ref _delay);
                }
                if (_delay != originalDelay) _delay = Mathf.Max(_minimumDelay, _delay);
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.ModifyShieldRechargeDelay");
            }
        }

        // Shield recharge transitions and the normal out-of-danger sound
        private static readonly List<CharacterBodyCallback> onShieldRechargeStartedCallbacks = new();
        private static readonly List<CanPlayShieldRechargeSoundCallback> canPlayShieldRechargeSoundCallbacks = new();

        private static readonly HookRegistration shieldRechargeHook = new("HealthComponent.ServerFixedUpdate (shield recharge)",
            () => IL.RoR2.HealthComponent.ServerFixedUpdate += HookShieldRecharge,
            () => IL.RoR2.HealthComponent.ServerFixedUpdate -= HookShieldRecharge);

        private static readonly HookRegistration shieldRechargeSoundHook = new("CharacterBody.OnOutOfDangerChanged (shield sound)",
            () => IL.RoR2.CharacterBody.OnOutOfDangerChanged += HookShieldRechargeSound,
            () => IL.RoR2.CharacterBody.OnOutOfDangerChanged -= HookShieldRechargeSound);

        internal static bool shieldRechargeSoundChangesAvailable => CanRunHook(shieldRechargeHook) && CanRunHook(shieldRechargeSoundHook);

        public static void AddOnShieldRechargeStartedCallback(CharacterBodyCallback _callback)
        {
            onShieldRechargeStartedCallbacks.Add(_callback);
            DebugLog("Added On Shield Recharge Started behaviour");
        }

        public static void AddCanPlayShieldRechargeSoundCallback(CanPlayShieldRechargeSoundCallback _callback)
        {
            canPlayShieldRechargeSoundCallbacks.Add(_callback);
            DebugLog("Added Can Play Shield Recharge Sound behaviour");
        }

        private static void HookShieldRecharge(ILContext _il)
        {
            ILCursor cursor = new(_il);
            int shieldVariable = -1;

            // Find the shield value being updated and the end of the recharge block
            if (!cursor.TryGotoNext(MoveType.After,
                    instruction => instruction.MatchLdarg(0),
                    instruction => instruction.MatchLdfld<HealthComponent>("shield"),
                    instruction => instruction.MatchStloc(out shieldVariable)) ||
                !cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld<CharacterBody>("shieldBrokeSinceDangerCheck")) ||
                !cursor.Next.MatchBrtrue(out ILLabel rechargeFinished))
            {
                throw new System.InvalidOperationException("Could not find the shield recharge block in ServerFixedUpdate.");
            }

            // Include skipped recharge paths, before the completion sound and shield assignment
            cursor.GotoLabel(rechargeFinished);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, shieldVariable);
            cursor.EmitDelegate(UpdateShieldRecharge);
        }

        private static void UpdateShieldRecharge(HealthComponent _healthComponent, float _newShield)
        {
            if (!CanRunHook(shieldRechargeHook) || onShieldRechargeStartedCallbacks.Count == 0) return;

            shieldRechargeHook.StartProfiling();
            try
            {
                CharacterBody body = _healthComponent.body;
                FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(body, _warnIfMissing: false);
                if (!helper) return;

                // Only the recharge block has changed this local shield value
                bool recharging = _newShield > _healthComponent.shield;
                if (helper.shieldRecharging == recharging) return;

                helper.shieldRecharging = recharging;
                if (!recharging) return;

                foreach (CharacterBodyCallback callback in onShieldRechargeStartedCallbacks)
                {
                    callback(body);
                }
            }
            finally
            {
                shieldRechargeHook.StopProfiling();
            }
        }

        private static void HookShieldRechargeSound(ILContext _il)
        {
            ILCursor cursor = new(_il);
            if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_outOfDanger")))
            {
                throw new System.InvalidOperationException("Could not find the shield sound check in OnOutOfDangerChanged.");
            }

            // Change only the sound condition, leaving stat updates in place
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CanPlayShieldRechargeSound);
        }

        private static bool CanPlayShieldRechargeSound(bool _canPlay, CharacterBody _body)
        {
            if (!_canPlay || !CanRunHook(shieldRechargeSoundHook)) return _canPlay;

            shieldRechargeSoundHook.StartProfiling();
            try
            {
                foreach (CanPlayShieldRechargeSoundCallback callback in canPlayShieldRechargeSoundCallbacks)
                {
                    if (!callback(_body)) return false;
                }
                return true;
            }
            finally
            {
                shieldRechargeSoundHook.StopProfiling();
            }
        }

        // Shared void shield choices for the HUD, model, item displays and break effects
        private static readonly List<ModifyVoidShieldVisualsCallback> modifyVoidShieldVisualsCallbacks = new();

        private static readonly HookRegistration voidShieldOverlayHook = new("CharacterModel.UpdateOverlays (void shields)",
            () => IL.RoR2.CharacterModel.UpdateOverlays += HookVoidShieldOverlay,
            () => IL.RoR2.CharacterModel.UpdateOverlays -= HookVoidShieldOverlay);

        private static readonly HookRegistration voidShieldDisplayHook = new("ShieldBoosterBodyBehavior.FixedUpdate (void shields)",
            () => IL.RoR2.Items.ShieldBoosterBodyBehavior.FixedUpdate += HookVoidShieldDisplay,
            () => IL.RoR2.Items.ShieldBoosterBodyBehavior.FixedUpdate -= HookVoidShieldDisplay);

        private static readonly HookRegistration voidShieldBreakHook = new("HealthComponent.TakeDamageProcess (void shields)",
            () => IL.RoR2.HealthComponent.TakeDamageProcess += HookVoidShieldBreak,
            () => IL.RoR2.HealthComponent.TakeDamageProcess -= HookVoidShieldBreak);

        private static readonly HookRegistration voidShieldSetHook = new("HealthComponent.SetShield (void shields)",
            () => IL.RoR2.HealthComponent.SetShield += HookVoidShieldSet,
            () => IL.RoR2.HealthComponent.SetShield -= HookVoidShieldSet);

        public static void AddModifyVoidShieldVisualsCallback(ModifyVoidShieldVisualsCallback _callback)
        {
            modifyVoidShieldVisualsCallbacks.Add(_callback);
            DebugLog("Added Modify Void Shield Visuals behaviour");
        }

        private static void HookVoidShieldOverlay(ILContext _il)
        {
            PatchVoidShieldVisualCheck<CharacterModel>(_il, voidShieldOverlayHook, _model => _model.body);
        }

        private static void HookVoidShieldDisplay(ILContext _il)
        {
            PatchVoidShieldVisualCheck<RoR2.Items.ShieldBoosterBodyBehavior>(_il, voidShieldDisplayHook, _behaviour => _behaviour.body);
        }

        private static void HookVoidShieldBreak(ILContext _il)
        {
            PatchVoidShieldVisualCheck<HealthComponent>(_il, voidShieldBreakHook, _healthComponent => _healthComponent.body);
        }

        private static void HookVoidShieldSet(ILContext _il)
        {
            PatchVoidShieldVisualCheck<HealthComponent>(_il, voidShieldSetHook, _healthComponent => _healthComponent.body);
        }

        private static void PatchVoidShieldVisualCheck<T>(ILContext _il, HookRegistration _hook, System.Func<T, CharacterBody> _getBody)
        {
            ILCursor cursor = new(_il);

            // Extend only the visual choice made by the Plasma Shrimp check
            if (!cursor.TryGotoNext(MoveType.After,
                    instruction => instruction.MatchLdsfld(typeof(DLC1Content.Items), nameof(DLC1Content.Items.MissileVoid)),
                    instruction => instruction.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCountEffective)),
                    instruction => instruction.MatchLdcI4(0),
                    instruction => instruction.MatchCgt()))
            {
                throw new System.InvalidOperationException($"Could not find the void shield visual check in {_il.Method.Name}.");
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<System.Func<bool, T, bool>>((hasVoidShields, source) => CanRunHook(_hook) ? ModifyVoidShieldVisuals(_getBody(source), hasVoidShields) : hasVoidShields);
        }

        private static bool ModifyVoidShieldVisuals(CharacterBody _body, bool _hasVoidShields)
        {
            if (!enabled || !Faithful.InitializationSucceeded || !_body || modifyVoidShieldVisualsCallbacks.Count == 0) return _hasVoidShields;

            FaithfulRuntimeProfiler.StartScope("Behaviour.ModifyVoidShieldVisuals");
            try
            {
                foreach (ModifyVoidShieldVisualsCallback callback in modifyVoidShieldVisualsCallbacks)
                {
                    callback(_body, ref _hasVoidShields);
                }
                return _hasVoidShields;
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.ModifyVoidShieldVisuals");
            }
        }

        // Health bar display values
        private static readonly List<ModifyHealthBarValuesCallback> modifyHealthBarValuesCallbacks = new();

        private static readonly HookRegistration healthBarValuesHook = new("HealthComponent.GetHealthBarValues",
            () => On.RoR2.HealthComponent.GetHealthBarValues += HookHealthBarValues,
            () => On.RoR2.HealthComponent.GetHealthBarValues -= HookHealthBarValues);

        public static void AddModifyHealthBarValuesCallback(ModifyHealthBarValuesCallback _callback)
        {
            modifyHealthBarValuesCallbacks.Add(_callback);
            DebugLog("Added Modify Health Bar Values behaviour");
        }

        private static HealthComponent.HealthBarValues HookHealthBarValues(On.RoR2.HealthComponent.orig_GetHealthBarValues orig, HealthComponent self)
        {
            HealthComponent.HealthBarValues values = orig(self);
            if (!CanRunHook(healthBarValuesHook)) return values;

            healthBarValuesHook.StartProfiling();
            try
            {
                values.hasVoidShields = ModifyVoidShieldVisuals(self.body, values.hasVoidShields);

                foreach (ModifyHealthBarValuesCallback callback in modifyHealthBarValuesCallbacks)
                {
                    callback(self, ref values);
                }

                return values;
            }
            finally
            {
                healthBarValuesHook.StopProfiling();
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

        public static void RemoveOnRecalculateStatsCallback(CharacterBodyCallback _callback)
        {
            onRecalculateStatsCallbacks.Remove(_callback);
            DebugLog("Removed On Recalculate Stats behaviour");
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
