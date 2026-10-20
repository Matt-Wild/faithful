using Newtonsoft.Json.Utilities;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class FaithfulTargetingMatrixBehaviour : NetworkBehaviour, ICharacterBehaviour, IDisplayModelBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

        // Character targeted character body
        public CharacterBody target;

        // All currently targeted character bodies
        private List<CharacterBody> targets = [];

        // Store time since each target has been out of range
        private Dictionary<CharacterBody, float> targetOutOfRangeTimers = [];

        // Store reference to visual effect
        private TemporaryVisualEffect visualEffect;

        // Store character that owns the visual effect
        private CharacterBody visualEffectTargeter;

        // Store starting local position of display bone
        private Vector3 displayPos;

        // How the display moves when targeting
        private Vector3 displayTargetingMovement = new Vector3(-0.0135f, 0.0f, 0.0f);

        // How the lens move when targeting
        private Vector3 lensMovement = new Vector3(0.0f, 0.00675f, 0.0f);

        // How 'open' the display is
        private float displayOpen = 0.0f;

        // How fast the display opens and closes
        private float displaySpeed = 4.0f;

        // Store if currently targeting something
        private bool targeting = false;

        // Reference to display mesh
        private GameObject m_displayMesh;

        // Reference to display bone
        private GameObject m_display;

        // Lens bones
        private GameObject m_clockwiseLens;
        private GameObject m_antiClockwiseLens;

        // Store reference to model transform
        private Transform m_modelTransform;

        // Lens initial scales
        private Vector3 clockwiseLensScale;
        private Vector3 antiClockwiseLensScale;

        // Lens initial positions
        private Vector3 clockwiseLensPos;
        private Vector3 antiClockwiseLensPos;

        // Store targeting matrix stats
        bool enableTargetEffect;
        bool targetEffectGlobal;
        float maxDistance;
        float outOfRangeTime;

        // Blacklisted characters for targeting
        string[] blacklisted = ["AffixEarthHealerBody", "ArtifactShellBody"];
        BodyIndex[] blacklistedIndexes;

        // Blacklisted buffs for targeting
        string[] blacklistedBuffs = ["bdBrotherThroneShield", "bdHiddenInvincibility", "bdHiddenRejectAllDamage"];
        BuffIndex[] blacklistedBuffIndexes;

        public FaithfulTargetingMatrixBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Get blacklisted indexes
            FetchBlacklistedIndexes();

            // Fetch item settings
            FetchSettings();

            // Check for model transform
            if (modelTransform != null)
            {
                // Setup display model behaviour relay
                DisplayModelBehaviourRelay relay = modelTransform.gameObject.AddComponent<DisplayModelBehaviourRelay>();
                relay.Init(this);
            }

            // Attempt to update display model
            UpdateDisplayObject();
        }

        void FetchBlacklistedIndexes()
        {
            // Create list of blacklisted indexes
            List<BodyIndex> bodyIndexes = new List<BodyIndex>();

            // Cycle through blacklisted characters for targeting
            foreach (string name in blacklisted)
            {
                // Add index to indexes list
                bodyIndexes.Add(BodyCatalog.FindBodyIndex(name));
            }

            // Set blacklisted indexes
            blacklistedIndexes = bodyIndexes.ToArray();

            // Convert buff names once instead of looking them up during target searches
            List<BuffIndex> buffIndexes = new List<BuffIndex>();
            foreach (string name in blacklistedBuffs)
            {
                BuffIndex buffIndex = BuffCatalog.FindBuffIndex(name);
                if (buffIndex != BuffIndex.None) buffIndexes.Add(buffIndex);
            }
            blacklistedBuffIndexes = buffIndexes.ToArray();
        }

        bool HasBlacklistedBuff(CharacterBody _character)
        {
            // Any listed buff makes this character unavailable for targeting
            foreach (BuffIndex buffIndex in blacklistedBuffIndexes)
            {
                if (_character.HasBuff(buffIndex)) return true;
            }
            return false;
        }

        public void FetchSettings()
        {
            // Get targeting matrix item
            Item item = Items.GetItem("TARGETING_MATRIX");

            // Update stats
            enableTargetEffect = item.FetchSetting<bool>("ENABLE_TARGET_EFFECT").Value;
            targetEffectGlobal = item.FetchSetting<bool>("TARGET_EFFECT_GLOBAL").Value;
            maxDistance = item.FetchSetting<float>("MAX_DISTANCE").Value;
            outOfRangeTime = item.FetchSetting<float>("OUT_OF_RANGE_TIME").Value;
        }

        void FixedUpdate()
        {
            FaithfulRuntimeProfiler.StartScope("FaithfulTargetingMatrixBehaviour.FixedUpdate");
            try
            {
                // Check if no longer targeting something
                if (targeting && !HasAnyTarget)
                {
                    // Set as no longer targeting
                    targeting = false;

                    // Stop all coroutines
                    StopAllCoroutines();

                    // Close display
                    StartCoroutine(CloseDisplay());
                }

                // Check if starting to target something
                if (!targeting && HasAnyTarget)
                {
                    // Set as targeting
                    targeting = true;

                    // Stop all coroutines
                    StopAllCoroutines();

                    // Open display
                    StartCoroutine(OpenDisplay());
                }

                // Check if hosting
                if (!Utils.hosting) return;

                // BELOW IS HOST ONLY BEHAVIOUR

                // Check for targets
                if (!HasAnyTarget) return;

                // Track how many targets need replacing
                int targetsRemoved = 0;

                // Cycle through targets backwards so targets can be removed during iteration
                for (int i = targets.Count - 1; i >= 0; i--)
                {
                    // Get current target
                    CharacterBody currentTarget = targets[i];

                    // Check if target is invalid
                    if (currentTarget == null || currentTarget.healthComponent == null || !currentTarget.healthComponent.alive)
                    {
                        // Remove invalid target
                        RemoveTarget(currentTarget, false);
                        targetsRemoved++;
                        continue;
                    }

                    // Get position of target
                    Vector3 targetPos = currentTarget.corePosition;

                    // Check if target is out of range
                    if (Vector3.Distance(targetPos, character.corePosition) > maxDistance)
                    {
                        // Add to out of range timer
                        targetOutOfRangeTimers[currentTarget] = targetOutOfRangeTimers.TryGetValue(currentTarget, out float timer) ? timer + Time.fixedDeltaTime : Time.fixedDeltaTime;
                    }

                    // Target in range
                    else
                    {
                        // Reset out of range timer
                        targetOutOfRangeTimers[currentTarget] = 0.0f;
                    }

                    // Check if out of range for too long
                    if (targetOutOfRangeTimers.TryGetValue(currentTarget, out float outOfRangeTimer) && outOfRangeTimer > outOfRangeTime)
                    {
                        // Remove target
                        RemoveTarget(currentTarget, false);
                        targetsRemoved++;
                    }
                }

                // Check if targets were removed
                if (targetsRemoved > 0)
                {
                    // Sync target state
                    CmdSyncTarget();

                    // Replace only the targets removed
                    SearchForTarget(targetsRemoved);
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("FaithfulTargetingMatrixBehaviour.FixedUpdate");
            }
        }

        void UpdateDisplayObject()
        {
            // Check for display
            if (display == null) return;

            // Set display bone position
            display.transform.localPosition = displayPos + displayTargetingMovement * (1.0f - displayOpen);

            // Set lens positions
            clockwiseLens.transform.localPosition = clockwiseLensPos + lensMovement * (1.0f - displayOpen);
            antiClockwiseLens.transform.localPosition = antiClockwiseLensPos + lensMovement * (1.0f - displayOpen);

            // Set lens scales
            clockwiseLens.transform.localScale = clockwiseLensScale * displayOpen;
            antiClockwiseLens.transform.localScale = antiClockwiseLensScale * displayOpen;
        }

        IEnumerator OpenDisplay()
        {
            // Cycle until fully open
            while (displayOpen < 1.0f)
            {
                // Add to how "open" the display is
                displayOpen = Mathf.Clamp01(displayOpen + Time.deltaTime * displaySpeed);

                // Update display model
                UpdateDisplayObject();

                // Go to next frame
                yield return null;
            }
        }

        IEnumerator CloseDisplay()
        {
            // Cycle until fully closed
            while (displayOpen > 0.0f)
            {
                // Remove from how "open" the display is
                displayOpen = Mathf.Clamp01(displayOpen - Time.deltaTime * displaySpeed);

                // Update display model
                UpdateDisplayObject();

                // Go to next frame
                yield return null;
            }
        }

        private void OnDestroy()
        {
            // Unregister with utils
            Utils.UnregisterCharacterBehaviour(this);

            // Clear targets owned by this character
            RemoveTarget(false);

            // Check for visual effect
            if (visualEffect != null)
            {
                // Remove visual effect
                visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
            }
        }

        public void OnKill(CharacterBody _killed)
        {
            // Check if target needs to be wiped
            if (HasTarget(_killed))
            {
                // Remove target
                RemoveTarget(_killed);
            }

            Invoke(nameof(SearchForTargetForKill), 0.1f);
        }

        void SearchForTargetForKill()
        {
            // Validate character
            if (character == null || character.inventory == null) return;

            // Search only for the amount a single kill should add
            int targetsPerKill = Faithful.targetingMatrix == null ? 0 : Faithful.targetingMatrix.GetTargetsPerKill(character.inventory);
            SearchForTarget(targetsPerKill);
        }

        void SearchForTarget(int _maxTargetsToAdd)
        {
            // Host only behaviour
            if (!Utils.hosting) return;

            // Validate character
            if (character == null || character.inventory == null) return;

            // Get target capacity
            int targetCapacity = Faithful.targetingMatrix == null ? 0 : Faithful.targetingMatrix.GetTargetCapacity(character.inventory);

            // Check if no targets should be held
            if (targetCapacity <= 0)
            {
                RemoveTarget();
                return;
            }

            // Trim targets if capacity was reduced
            if (TrimTargetsToCapacity(targetCapacity))
            {
                CmdSyncTarget();
            }

            // Check if target capacity is already reached
            if (targets.Count >= targetCapacity) return;

            // Check if no targets should be added from this search
            if (_maxTargetsToAdd <= 0) return;

            // Limit how many targets this search can add while still respecting total capacity
            int targetLimit = _maxTargetsToAdd == int.MaxValue ? targetCapacity : Mathf.Min(targetCapacity, targets.Count + _maxTargetsToAdd);

            // Get character bodies in scene
            ReadOnlyCollection<CharacterBody> characterBodies = CharacterBody.readOnlyInstancesList;
            if (characterBodies == null) return;

            // Initialise filtered list of character bodies
            List<CharacterBody> filteredCharacterBodies = new List<CharacterBody>();

            // Get targeter position
            Vector3 targeterPos = character.corePosition;

            // Cycle through character bodies in scene
            foreach (CharacterBody characterBody in characterBodies)
            {
                // Check if target is already held
                if (HasTarget(characterBody)) continue;

                // Check if on the same team as the targeter
                if (characterBody.teamComponent.teamIndex == character.teamComponent.teamIndex) continue;

                // Check if character body has a master
                if (characterBody.master == null) continue;

                // Check if character body has a health component
                if (characterBody.healthComponent == null) continue;

                // Check if character is alive
                if (!characterBody.healthComponent.alive) continue;

                // Get distance to target
                float targetDistance = Vector3.Distance(targeterPos, characterBody.corePosition);

                // Check if character body is too far from player
                if (targetDistance > maxDistance) continue;

                // Check if blacklisted
                if (blacklistedIndexes.Contains(characterBody.bodyIndex)) continue;

                // Skip characters protected by a blacklisted buff
                if (HasBlacklistedBuff(characterBody)) continue;

                // Valid target
                filteredCharacterBodies.Add(characterBody);
            }

            // Track if new targets are added
            bool targetsAdded = false;

            // Cycle until no filtered character bodies remaining or search limit is reached
            while (filteredCharacterBodies.Count > 0 && targets.Count < targetLimit)
            {
                // Get chosen target character body
                CharacterBody chosenTarget = filteredCharacterBodies[Random.Range(0, filteredCharacterBodies.Count)];

                // Remove from filtered list
                filteredCharacterBodies.Remove(chosenTarget);

                // Get faithful behaviour for chosen target
                FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(chosenTarget);
                if (targetCharacterBehaviour == null)
                {
                    continue;
                }

                // Get targeting matrix behaviour for chosen target
                FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
                if (targetTargetingMatrixBehaviour == null)
                {
                    continue;
                }

                // Set target
                AddTarget(chosenTarget, targetTargetingMatrixBehaviour);
                targetsAdded = true;
            }

            // Sync targets with clients
            if (targetsAdded)
            {
                CmdSyncTarget();
            }
        }

        public void RemoveTarget(bool _sync = true)
        {
            // Check for targets
            if (!HasAnyTarget) return;

            // Copy targets so the list can be cleared during iteration
            List<CharacterBody> targetsToRemove = [.. targets];

            // Cycle through targets
            foreach (CharacterBody currentTarget in targetsToRemove)
            {
                // Remove target without syncing each individual removal
                RemoveTarget(currentTarget, false);
            }

            // Clear any null targets that could not be removed normally
            targets.Clear();
            targetOutOfRangeTimers.Clear();

            // Clear primary target
            target = null;

            // Sync if hosting
            if (_sync && Utils.hosting) CmdSyncTarget();
        }

        public bool RemoveTarget(CharacterBody _target, bool _sync = true)
        {
            // Check for target
            if (_target == null)
            {
                // Remove any null entries
                bool removedNull = targets.RemoveAll(targetBody => targetBody == null) > 0;
                UpdatePrimaryTarget();
                return removedNull;
            }

            // Check if target is present
            if (!targets.Contains(_target)) return false;

            // Get faithful behaviour for target
            FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(_target);

            // Get targeting matrix behaviour for target
            FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour == null ? null : targetCharacterBehaviour.targetingMatrix;

            // Tell target targeting matrix behaviour that it's no longer being targeted
            targetTargetingMatrixBehaviour?.SetNotTargeted(character);

            // Remove target
            targets.Remove(_target);
            targetOutOfRangeTimers.Remove(_target);
            UpdatePrimaryTarget();

            // Sync if hosting
            if (_sync && Utils.hosting) CmdSyncTarget();

            return true;
        }

        public void RefreshTargetCapacity()
        {
            // Host only behaviour
            if (!Utils.hosting) return;

            // Validate character
            if (character == null || character.inventory == null) return;

            // Get target capacity
            int targetCapacity = Faithful.targetingMatrix == null ? 0 : Faithful.targetingMatrix.GetTargetCapacity(character.inventory);

            // Remove all targets if no targets should be held
            if (targetCapacity <= 0)
            {
                RemoveTarget();
                return;
            }

            // Trim targets if capacity was reduced
            if (TrimTargetsToCapacity(targetCapacity))
            {
                CmdSyncTarget();
            }
        }

        public bool HasTarget(CharacterBody _target)
        {
            // Check for target
            if (_target == null) return false;

            // Return if target is currently held
            return targets.Contains(_target);
        }

        private void AddTarget(CharacterBody _target, FaithfulTargetingMatrixBehaviour _targetTargetingMatrixBehaviour)
        {
            // Validate target
            if (_target == null || HasTarget(_target)) return;

            // Add target
            targets.Add(_target);
            targetOutOfRangeTimers[_target] = 0.0f;
            UpdatePrimaryTarget();

            // Tell target targeting matrix behaviour that it's being targeted
            _targetTargetingMatrixBehaviour?.SetTargeted(character);
        }

        private void UpdatePrimaryTarget()
        {
            // Remove null targets
            targets.RemoveAll(targetBody => targetBody == null);

            // Update primary target for compatibility with existing code
            target = targets.Count > 0 ? targets[0] : null;
        }

        private bool TrimTargetsToCapacity(int _targetCapacity)
        {
            // Track if targets changed
            bool targetsChanged = false;

            // Remove targets until within capacity
            while (targets.Count > _targetCapacity)
            {
                // Remove last target without syncing each individual removal
                RemoveTarget(targets[targets.Count - 1], false);
                targetsChanged = true;
            }

            return targetsChanged;
        }

        private bool HasAnyTarget
        {
            get
            {
                // Keep target list tidy
                UpdatePrimaryTarget();

                // Return if any target is present
                return target != null;
            }
        }

        public void SetTargeted(CharacterBody _targeter)
        {
            // Check if targeter is the local player
            if (_targeter == null || Utils.localPlayerBody != _targeter) return;

            // Check for visual effect
            if (visualEffect != null) return;

            // Check if should create visual effect
            if (enableTargetEffect)
            {
                // Create visual effect
                GameObject gameObject = Instantiate(targetEffectGlobal ? Assets.matrixEffectPrefab : Assets.matrixEffectLocalPrefab, character.corePosition, Quaternion.identity);
                visualEffect = gameObject.GetComponent<TemporaryVisualEffect>();
                visualEffect.parentTransform = character.coreTransform;
                visualEffect.visualState = TemporaryVisualEffect.VisualState.Enter;
                visualEffect.healthComponent = character.healthComponent;
                visualEffect.radius = character.radius;
                visualEffectTargeter = _targeter;
                LocalCameraEffect component = gameObject.GetComponent<LocalCameraEffect>();
                if (component)
                {
                    component.targetCharacter = base.gameObject;
                }
            }
        }

        public void SetNotTargeted(CharacterBody _targeter)
        {
            // Only the character that owns the visual effect can remove it
            if (visualEffectTargeter != _targeter) return;

            // Remove visual effect
            if (visualEffect != null) visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;

            // Clear references so a new effect can be created while the old one exits
            visualEffect = null;
            visualEffectTargeter = null;
        }

        [Command]
        public void CmdSyncTarget()
        {
            // Collect network IDs for all current targets
            List<NetworkInstanceId> targetNetIDs = [];

            // Cycle through targets
            foreach (CharacterBody currentTarget in targets)
            {
                // Validate target
                if (currentTarget == null) continue;

                // Get network identity
                NetworkIdentity networkIdentity = currentTarget.GetComponent<NetworkIdentity>();
                if (networkIdentity == null) continue;

                // Add target network ID
                targetNetIDs.Add(networkIdentity.netId);
            }

            // Sync the complete target list on all clients
            RpcSyncTargets(targetNetIDs.ToArray());
        }

        [ClientRpc]
        private void RpcSyncTargets(NetworkInstanceId[] _targetNetIDs)
        {
            // Unnecessary for host
            if (Utils.hosting) return;

            // Copy targets so missing targets can be removed during iteration
            List<CharacterBody> previousTargets = [.. targets];

            // Remove only targets no longer present in the synced list
            foreach (CharacterBody currentTarget in previousTargets)
            {
                // Get target network identity
                NetworkIdentity networkIdentity = currentTarget == null ? null : currentTarget.GetComponent<NetworkIdentity>();

                // Remove missing or invalid target without syncing back to the host
                if (networkIdentity == null || !_targetNetIDs.Contains(networkIdentity.netId))
                {
                    RemoveTarget(currentTarget, false);
                }
            }

            // Add new targets while keeping existing indicators active
            foreach (NetworkInstanceId targetNetID in _targetNetIDs)
            {
                // Attempt to find target character body
                CharacterBody targetBody = ClientScene.FindLocalObject(targetNetID)?.GetComponent<CharacterBody>();

                // Check for target body
                if (targetBody == null)
                {
                    // Warn and skip target
                    Log.Warning($"[TARGETING MATRIX] | Could not find target body with net ID {targetNetID} - Sync unsuccessful.");
                    continue;
                }

                // Skip targets already held
                if (HasTarget(targetBody)) continue;

                // Get faithful behaviour for target
                FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(targetBody);
                if (targetCharacterBehaviour == null) continue;

                // Get targeting matrix behaviour for target
                FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
                if (targetTargetingMatrixBehaviour == null) continue;

                // Add target
                AddTarget(targetBody, targetTargetingMatrixBehaviour);
            }
        }

        public void OnDisplayModelCreated()
        {
            // Update item display object
            UpdateDisplayObject();
        }

        GameObject displayMesh
        {
            get
            {
                // Return null if no model transform
                if (modelTransform == null) return null;

                // Check for display mesh
                if (m_displayMesh == null)
                {
                    // Attempt to find display mesh
                    m_displayMesh = Utils.FindChildByName(modelTransform, "TargetingMatrixDisplayMesh(Clone)");
                }

                // Return display mesh
                return m_displayMesh;
            }
        }

        GameObject display
        {
            get
            {
                // Check for display mesh
                if (displayMesh == null) return null;

                // Check for display
                if (m_display == null)
                {
                    // Attempt to find display
                    m_display = Utils.FindChildByName(displayMesh.transform, "Display");

                    // Get initial display position
                    displayPos = m_display.transform.localPosition;
                }

                // Return display
                return m_display;
            }
        }

        GameObject clockwiseLens
        {
            get
            {
                // Check for display mesh
                if (displayMesh == null) return null;

                // Check for lens
                if (m_clockwiseLens == null)
                {
                    // Attempt to find display
                    m_clockwiseLens = Utils.FindChildByName(displayMesh.transform, "Lens_Clock");

                    // Get initial lens scale and position
                    clockwiseLensScale = m_clockwiseLens.transform.localScale;
                    clockwiseLensPos = m_clockwiseLens.transform.localPosition;
                }

                // Return lens
                return m_clockwiseLens;
            }
        }

        GameObject antiClockwiseLens
        {
            get
            {
                // Check for display mesh
                if (displayMesh == null) return null;

                // Check for lens
                if (m_antiClockwiseLens == null)
                {
                    // Attempt to find display
                    m_antiClockwiseLens = Utils.FindChildByName(displayMesh.transform, "Lens_Anti");

                    // Get initial lens scale and position
                    antiClockwiseLensScale = m_antiClockwiseLens.transform.localScale;
                    antiClockwiseLensPos = m_antiClockwiseLens.transform.localPosition;
                }

                // Return lens
                return m_antiClockwiseLens;
            }
        }

        Transform modelTransform
        {
            get
            {
                // Check for model transform
                if (m_modelTransform == null)
                {
                    // Get model transform
                    m_modelTransform = character?.modelLocator?.modelTransform;
                }

                // Return model transform
                return m_modelTransform;
            }
        }

        public string relatedItemToken
        {
            get
            {
                // Related item
                return "TARGETING_MATRIX";
            }
        }
    }
}
