﻿using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine.Networking;

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

        // Store if currently targeting something
        private bool targeting = false;

        // Reference to display mesh
        private GameObject m_displayMesh;

        // Reference to display bone
        private GameObject m_display;

        public FaithfulTargetingMatrixBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Setup display model behaviour relay
            DisplayModelBehaviourRelay relay = character.modelLocator.modelTransform.gameObject.AddComponent<DisplayModelBehaviourRelay>();
            relay.Init(this);

            // Attempt to update display model
            UpdateDisplayObject();
        }

        public void FetchSettings()
        {
            
        }

        void FixedUpdate()
        {
            // Check if no longer targeting something
            if (targeting && target == null)
            {
                // Set as no longer targeting
                targeting = false;

                // Update display object
                UpdateDisplayObject();
            }

            // Check if starting to target something
            if (!targeting && target != null)
            {
                // Set as targeting
                targeting = true;

                // Update display object
                UpdateDisplayObject();
            }

            // Check if hosting
            if (!Utils.hosting) return;

            // BELOW IS HOST ONLY BEHAVIOUR

            // Check for target
            if (target == null) return;

            // Get position of target
            targetPos = target.corePosition;

            // Check if target is out of range
            if (Vector3.Distance(targetPos, character.corePosition) > 300.0f)
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
            if (outOfRangeTimer > 15.0f)
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

            // Check if targeting
            if (targeting)
            {
                // Set display bone position
                display.transform.localPosition = displayPos;
            }

            // Not targeting
            else
            {
                // Set display bone position
                display.transform.localPosition = displayPos + displayTargetingMovement;
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
            // Check if hosting and there is no current target (or the current target just died)
            if (Utils.hosting && (target == null || target == _killed))
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

                    // Check if character just got killed
                    if (_killed == characterBody) continue;

                    // Get distance to target
                    float targetDistance = Vector3.Distance(targeterPos, characterBody.corePosition);

                    // Check if character body is too far from player
                    if (targetDistance > 300.0f) continue;

                    // Valid target
                    filteredCharacterBodies.Add(characterBody);

                    // Check if target is "close" to target
                    if (targetDistance <= 120.0f) closeCharacterBodies.Add(characterBody);
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
                    float weight = distance > 0.0f ? (distance > 30.0f ? 1.0f / distance : 2.0f / distance) : float.MaxValue;

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
                        if (targetCharacterBehaviour == null) return;

                        // Get targeting matrix behaviour for chosen target
                        FaithfulTargetingMatrixBehaviour targetTargetingMatrixBehaviour = targetCharacterBehaviour.targetingMatrix;
                        if (targetTargetingMatrixBehaviour == null) return;

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

            // Fetch target to sync
            if (NetworkServer.objects.TryGetValue(_targetNetID, out NetworkIdentity networkIdentity))
            {
                // Attempt to get target character body
                CharacterBody targetBody = networkIdentity.gameObject.GetComponent<CharacterBody>();
                if (targetBody == null) return;

                // Attempt to get target targeting matrix behaviour
                FaithfulTargetingMatrixBehaviour matrixBehaviour = networkIdentity.gameObject.GetComponent<FaithfulTargetingMatrixBehaviour>();
                if (matrixBehaviour == null) return;

                // Set target
                matrixBehaviour.SetTargeted(character);
                target = targetBody;
            }
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
                // Check for display mesh
                if (m_displayMesh == null)
                {
                    // Attempt to find display mesh
                    m_displayMesh = Utils.FindChildByName(character.modelLocator.modelTransform, "TargetingMatrixDisplayMesh(Clone)");
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
