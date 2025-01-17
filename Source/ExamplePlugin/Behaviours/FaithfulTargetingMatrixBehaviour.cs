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
                }

                // Check for valid character bodies
                if (filteredCharacterBodies.Count == 0) return;

                // Get targeter position
                Vector3 targeterPos = character.corePosition;

                // Select random character body weighted on distance so closer character bodies are more likely to be chosen

                // Total weight and weight array
                float totalWeight = 0f;
                float[] weights = new float[filteredCharacterBodies.Count];

                // Cycle through character bodies
                for (int i = 0; i < filteredCharacterBodies.Count; i++)
                {
                    // Get character body and distance to targeter
                    CharacterBody body = filteredCharacterBodies[i];
                    float distance = Vector3.Distance(body.corePosition, targeterPos);

                    // Avoid division by zero, assign a very high weight to objects at the exact position
                    float weight = distance > 0f ? 1f / distance : float.MaxValue;

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

                        Log.Debug("TARGET ASSIGNED");

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
    }
}
