using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class FaithfulLeadersPennonBehaviour : MonoBehaviour
    {
        // Store reference to Character Body and Character Inventory
        public CharacterBody character;

        // Store previous registered item count
        int count = 0;

        // Store reference to radius indicator
        FaithfulRadiusIndicatorBehaviour radiusIndicator;

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Check for inventory
            Inventory inventory = character.inventory;
            if (inventory != null)
            {
                // Update item count with current item amount
                UpdateItemCount(inventory.GetItemCount(Items.GetItem("LEADERS_PENNON").itemDef));
            }

            // Hook behaviour
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);
        }

        private void OnDestroy()
        {
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
            if (_newCount == count)
            {
                return;
            }

            // Update count
            count = _newCount;

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
                float radius = 15.0f + (count - 1) * 5.0f;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Set radius indicator target size
                radiusIndicator.SetTargetSize(radius);
            }

            // Radius indicator doesn't exist
            else
            {
                // Calculate effect radius
                float radius = 15.0f + (count - 1) * 5.0f;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Create radius indicator
                radiusIndicator = Utils.CreateRadiusIndicator(character, 0.0f, radius, new Color(0.58039215f, 0.22745098f, 0.71764705f));
            }
        }
    }
}
