using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System.Linq;

namespace Faithful
{
    internal class FaithfulTargetingMatrixBehaviour : NetworkBehaviour, ICharacterBehaviour, IDisplayModelBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

        // Character targeted character body
        public CharacterBody target;

        // Store reference to visual effect
        private TemporaryVisualEffect visualEffect;

        // Store time since being out of range of targeting
        private float outOfRangeTimer;

        // Store last known position of target
        private Vector3 targetPos = Vector3.zero;

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
        float maxDistance;
        float closeDistance;
        float preferredDistance;
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
            maxDistance = item.FetchSetting<float>("MAX_DISTANCE").Value;
            closeDistance = item.FetchSetting<float>("CLOSE_DISTANCE").Value;
            preferredDistance = item.FetchSetting<float>("PREFERRED_DISTANCE").Value;
            outOfRangeTime = item.FetchSetting<float>("OUT_OF_RANGE_TIME").Value;
        }

        void FixedUpdate()
        {
            // Check if no longer targeting something
            if (targeting && target == null)
            {
                // Set as no longer targeting
                targeting = false;

                // Stop all coroutines
                StopAllCoroutines();

                // Close display
                StartCoroutine(CloseDisplay());
            }

            // Check if starting to target something
            if (!targeting && target != null)
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

            // Check for target
            if (target == null) return;

            // Get position of target
            targetPos = target.corePosition;

            // Check if target is out of range
            if (Vector3.Distance(targetPos, character.corePosition) > maxDistance)
            {
                // Add to out of range timer
                outOfRangeTimer += Time.fixedDeltaTime;
            }

            // Target in range
            else
            {
                // Reset out of range timer
                outOfRangeTimer = 0.0f;
            }

            // Check if out of range for too long
            if (outOfRangeTimer > outOfRangeTime)
            {
                // Remove target
                RemoveTarget();

                // Sync target with clients
                CmdSyncTarget();
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
            if (target != null && target == _killed)
            {
                // Remove target
                RemoveTarget();
            }

            Invoke("SearchForTarget", 0.1f);
        }

        void SearchForTarget()
        {
            // Check if hosting and there is no current target
            if (Utils.hosting && target == null)
            {
                // Get character bodies in scene
                ReadOnlyCollection<CharacterBody> characterBodies = CharacterBody.readOnlyInstancesList;
                if (characterBodies == null) return;

                // Initialise filtered list of character bodies
                List<CharacterBody> filteredCharacterBodies = new List<CharacterBody>();

                // Initialise list of "close" character bodies
                List<CharacterBody> closeCharacterBodies = new List<CharacterBody>();

                // Get targeter position
                Vector3 targeterPos = character.corePosition;

                // Cycle through character bodies in scene
                foreach (CharacterBody characterBody in characterBodies)
                {
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

                    // Check if target is "close" to target
                    if (targetDistance <= closeDistance) closeCharacterBodies.Add(characterBody);
                }

                // Check if close targets were found
                if (closeCharacterBodies.Count > 0)
                {
                    // Use only close characters
                    filteredCharacterBodies = closeCharacterBodies;
                }

                // Otherwise check for valid character bodies
                else if (filteredCharacterBodies.Count == 0) return;

                // Select random character body weighted on distance so closer character bodies are more likely to be chosen

                // Total weight and weight array
                float totalWeight = 0f;
                float[] weights = new float[filteredCharacterBodies.Count];

                // Cycle through character bodies
                for (int i = 0; i < filteredCharacterBodies.Count; i++)
                {
                    // Get character body and distance to previous target or targeter
                    CharacterBody body = filteredCharacterBodies[i];
                    float distance = targetPos == Vector3.zero ? Vector3.Distance(body.corePosition, targeterPos) : Vector3.Distance(body.corePosition, targetPos);

                    // Avoid division by zero, assign a very high weight to objects at the exact position
                    // Assign more favourable weights to character bodies within a small distance
                    float weight = distance > 0.0f ? (distance > preferredDistance ? 1.0f / distance : 2.0f / distance) : float.MaxValue;

                    // Add to total weight and weights array
                    totalWeight += weight;
                    weights[i] = weight;
                }

                // Select a random value
                float randomValue = Random.Range(0f, totalWeight);

                // Cycle through character bodies and accumulate weights until random value is reached
                float cumulativeWeight = 0f;
                for (int i = 0; i < filteredCharacterBodies.Count; i++)
                {
                    // Accumulate weight
                    cumulativeWeight += weights[i];

                    // Check if random value has been reached
                    if (randomValue <= cumulativeWeight)
                    {
                        // Get chosen target character body
                        CharacterBody chosenTarget = filteredCharacterBodies[i];

                        // Get faithful behaviour for chosen target
                        FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(chosenTarget);
                        if (targetCharacterBehaviour == null) continue;

                        // Get targeting matrix behaviour for chosen target
                        FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
                        if (targetTargetingMatrixBehaviour == null) continue;

                        // Set target
                        targetTargetingMatrixBehaviour.SetTargeted(character);
                        target = chosenTarget;

                        // Sync target with clients
                        CmdSyncTarget();

                        // Done
                        return;
                    }
                }
            }
        }

        void RemoveTarget()
        {
            // Check for target
            if (target == null) return;

            // Get faithful behaviour for target
            FaithfulCharacterBodyBehaviour targetCharacterBehaviour = Utils.FindCharacterBodyHelper(target);
            if (targetCharacterBehaviour == null) return;

            // Get targeting matrix behaviour for target
            FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
            if (targetTargetingMatrixBehaviour == null) return;

            // Tell target targeting matrix behaviour that it's no longer being targeted
            targetTargetingMatrixBehaviour.SetNotTargeted();

            // Remove target
            target = null;
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
                GameObject gameObject = Instantiate(Assets.matrixEffectPrefab, character.corePosition, Quaternion.identity);
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
            // Check for target
            if (target != null)
            {
                // Sync target on all clients
                RpcSyncTarget(target.GetComponent<NetworkIdentity>().netId);
            }

            // No target
            else
            {
                // Sync no target on all clients
                RpcSyncNoTarget();
            }
        }

        [ClientRpc]
        private void RpcSyncTarget(NetworkInstanceId _targetNetID)
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

            // Set target
            targetTargetingMatrixBehaviour.SetTargeted(character);
            target = targetBody;
        }

        [ClientRpc]
        private void RpcSyncNoTarget()
        {
            // Unnecessary for host
            if (Utils.hosting) return;

            // Remove target
            RemoveTarget();
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
