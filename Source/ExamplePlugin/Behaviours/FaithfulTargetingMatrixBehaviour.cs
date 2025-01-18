using RoR2;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Faithful
{
    internal class FaithfulTargetingMatrixBehaviour : MonoBehaviour, ICharacterBehaviour
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

        public FaithfulTargetingMatrixBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;
        }

        public void FetchSettings()
        {
            
        }

        void FixedUpdate()
        {
            // Check if hosting
            if (!Utils.hosting) return;

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
            if (outOfRangeTimer > 30.0f)
            {
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

            // Unhook behaviour
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

                    // Valid target
                    filteredCharacterBodies.Add(characterBody);

                    // Check if target is "close" to target
                    if (Vector3.Distance(targeterPos, characterBody.corePosition) <= 120.0f) closeCharacterBodies.Add(characterBody);
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

                        // Done
                        return;
                    }
                }
            }
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
    }
}
