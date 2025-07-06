using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulLongshotGeodeBehaviour : MonoBehaviour, ICharacterBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

        // Does longshot geode have a radius indicator?
        private bool enableRadiusIndicator;
        private float effectRadius;

        // Store reference to radius indicator
        FaithfulRadiusIndicatorBehaviour radiusIndicator;

        public FaithfulLongshotGeodeBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Fetch settings
            FetchSettings();

            // Check for inventory
            Inventory inventory = character.inventory;
            if (inventory != null)
            {
                // Update if radius indicator should be present
                UpdateRadiusIndicator(inventory.GetItemCount(Items.GetItem("LONGSHOT_GEODE").itemDef) != 0);
            }

            // Hook behaviour
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);
        }

        public void FetchSettings()
        {
            // Get item
            Item item = Items.GetItem("LONGSHOT_GEODE");

            // Update settings
            enableRadiusIndicator = item.FetchSetting<bool>("ENABLE_RADIUS_INDICATOR").Value;
            effectRadius = item.FetchSetting<float>("DISTANCE").Value;
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

            // Unhook behaviour
            Behaviour.RemoveOnInventoryChangedCallback(OnInventoryChanged);
        }

        protected void OnInventoryChanged(Inventory _inventory)
        {
            // Ignore if radius indicator not enabled
            if (!enableRadiusIndicator) return;

            // Attempt to get Character Body
            CharacterBody body = Utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Check if for this character body
            if (body != character) return;

            // Update radius indicator
            UpdateRadiusIndicator(_inventory.GetItemCount(Items.GetItem("LONGSHOT_GEODE").itemDef) != 0);
        }

        protected void UpdateRadiusIndicator(bool _enabled)
        {
            // Ignore if radius indicator not enabled
            if (!enableRadiusIndicator) return;

            // Check if radius indicator exists
            if (radiusIndicator != null)
            {
                // Check if needs to destroy radius indicator
                if (!_enabled)
                {
                    // Destroy radius indicator
                    Destroy(radiusIndicator.gameObject);
                    return;
                }
            }

            // Radius indicator doesn't exist but should
            else if (_enabled)
            {
                // Create radius indicator
                radiusIndicator = Utils.CreateRadiusIndicator(character, 0.0f, effectRadius, new Color(0.780392157f, 0.149019608f, 0.505882353f));
            }
        }
    }
}
