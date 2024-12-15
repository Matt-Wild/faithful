using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulLeadersPennonBehaviour : MonoBehaviour, ICharacterBehaviour
    {
        // Store reference to Character Body and Character Inventory
        public CharacterBody character;

        // Store reference to visual effect
        private TemporaryVisualEffect visualEffect;

        // Store previous registered item count
        int count = 0;

        // Store leader's pennon stats
        bool enableRadiusIndicator;
        float baseRadius;
        float radiusStacking;

        // Store reference to radius indicator
        FaithfulRadiusIndicatorBehaviour radiusIndicator;

        public FaithfulLeadersPennonBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Fetch leader's pennon settings
            FetchSettings();

            // Check for inventory
            Inventory inventory = character.inventory;
            if (inventory != null)
            {
                // Update item count with current item amount
                UpdateItemCount(inventory.GetItemCount(Items.GetItem("LEADERS_PENNON").itemDef));
            }

            // Hook behaviour
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);

            // Update temporary visual effects
            character.UpdateAllTemporaryVisualEffects();
        }

        public void FetchSettings()
        {
            // Get leaders pennon item
            Item leadersPennonItem = Items.GetItem("LEADERS_PENNON");

            // Update stats
            enableRadiusIndicator = leadersPennonItem.FetchSetting<bool>("ENABLE_RADIUS_INDICATOR").Value;
            baseRadius = leadersPennonItem.FetchSetting<float>("RADIUS").Value;
            radiusStacking = leadersPennonItem.FetchSetting<float>("RADIUS_STACKING").Value;
        }

        private void OnDestroy()
        {
            // Unregister with utils
            Utils.UnregisterCharacterBehaviour(this);

            // Check for radius indicator exists
            if (radiusIndicator != null)
            {
                // Destroy radius indicator
                Destroy(radiusIndicator.gameObject);
            }

            // Check for visual effect
            if (visualEffect != null)
            {
                // Remove visual effect
                visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
            }

            // Unhook behaviour
            Behaviour.RemoveOnInventoryChangedCallback(OnInventoryChanged);
        }

        protected void OnInventoryChanged(Inventory _inventory)
        {
            // Attempt to get Character Body
            CharacterBody body = Utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Check if for this character body
            if (body != character) return;

            // Get new item count
            int newCount = _inventory.GetItemCount(Items.GetItem("LEADERS_PENNON").itemDef);

            // Update item count
            UpdateItemCount(newCount);
        }

        protected void UpdateItemCount(int _newCount)
        {
            // Check if the same as previous count
            if (_newCount == count) return;

            // Update count
            count = _newCount;

            // Skip adjusting radius indicator if it is disabled
            if (!enableRadiusIndicator) return;

            // Check if radius indicator exists
            if (radiusIndicator != null)
            {
                // Check if needs to destroy radius indicator
                if (count == 0)
                {
                    // Destroy radius indicator
                    Destroy(radiusIndicator.gameObject);
                    return;
                }

                // Calculate radius indicator radius
                float radius = baseRadius + (count - 1) * radiusStacking;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Set radius indicator target size
                radiusIndicator.SetTargetSize(radius);
            }

            // Radius indicator doesn't exist
            else
            {
                // Calculate effect radius
                float radius = baseRadius + (count - 1) * radiusStacking;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Create radius indicator
                radiusIndicator = Utils.CreateRadiusIndicator(character, 0.0f, radius, new Color(0.58039215f, 0.22745098f, 0.71764705f));
            }
        }

        public void UpdateVisualEffect(bool _active)
        {
            // Check for necessary asset
            if (Assets.pennonEffectPrefab == null) return;

            // Check if visual effect should be active
            if (_active)
            {
                // Check if already has visual effect
                if (visualEffect != null) return;

                // Create visual effect
                GameObject gameObject = Instantiate(Assets.pennonEffectPrefab, character.corePosition, Quaternion.identity);
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

                // Done
                return;
            }

            // Visual effect not supposed to be active

            // Check if visual effect is already not active
            if (visualEffect == null) return;

            // Remove visual effect
            visualEffect.visualState = TemporaryVisualEffect.VisualState.Exit;
        }
    }
}
