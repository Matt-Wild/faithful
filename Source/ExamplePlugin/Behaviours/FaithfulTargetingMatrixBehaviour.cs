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
        string[] blacklisted = ["AffixEarthHealerBody"];
        BodyIndex[] blacklistedIndexes;

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

            // Track if targets changed
            bool targetsChanged = false;

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
                    targetsChanged = true;
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
                    targetsChanged = true;
                }
            }

            // Check if targets changed
            if (targetsChanged)
            {
                // Sync target state
                CmdSyncTarget();

                // Attempt to find new target
                SearchForTarget();
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

        void SearchForTarget()
        {
            SearchForTarget(int.MaxValue);
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
            targetTargetingMatrixBehaviour?.SetNotTargeted();

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
            if (Utils.localPlayerBody != _targeter) return;

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
                LocalCameraEffect component = gameObject.GetComponent<LocalCameraEffect>();
                if (component)
                {
                    component.targetCharacter = base.gameObject;
                }
            }
        }

        public void SetNotTargeted()
        {
            // Check for visual effect
            if (visualEffect == null) return;

            // Remove visual effect
            visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
        }

        [Command]
        public void CmdSyncTarget()
        {
            // Clear targets on clients first
            RpcSyncNoTarget();

            // Sync all targets on clients
            foreach (CharacterBody currentTarget in targets)
            {
                // Validate target
                if (currentTarget == null) continue;

                // Get network identity
                NetworkIdentity networkIdentity = currentTarget.GetComponent<NetworkIdentity>();
                if (networkIdentity == null) continue;

                // Sync target on all clients
                RpcAddSyncTarget(networkIdentity.netId);
            }
        }

        [ClientRpc]
        private void RpcAddSyncTarget(NetworkInstanceId _targetNetID)
        {
            // Unnecessary for host
            if (Utils.hosting) return;

            // Attempt to find target character body
            CharacterBody targetBody = ClientScene.FindLocalObject(_targetNetID)?.GetComponent<CharacterBody>();

            // Check for target body
            if (targetBody == null)
            {
                // Warn and return
                Log.Warning($"[TARGETING MATRIX] | Could not find target body with net ID {_targetNetID} - Sync unsuccessful.");
                return;
            }

            // Get faithful behaviour for target
            FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(targetBody);
            if (targetCharacterBehaviour == null) return;

            // Get targeting matrix behaviour for target
            FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
            if (targetTargetingMatrixBehaviour == null) return;

            // Add target
            AddTarget(targetBody, targetTargetingMatrixBehaviour);
        }

        [ClientRpc]
        private void RpcSyncNoTarget()
        {
            // Unnecessary for host
            if (Utils.hosting) return;

            // Remove targets
            RemoveTarget(false);
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
