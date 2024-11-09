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

        // Store reference to effect
        GameObject effect;

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

            // Check if effect exists
            if (effect != null)
            {
                // Check if needs to destroy effect
                if (count == 0)
                {
                    // Destroy effect
                    Destroy(effect);
                    return;
                }

                // Calculate effect radius
                float radius = 15.0f + (count - 1) * 5.0f;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Calculate scale multiplier
                float scaleMult = radius / 13.0f;

                // Remember parent
                Transform parent = effect.transform.parent;

                // Set scale
                effect.transform.parent = null;
                effect.transform.localScale = new Vector3(scaleMult, scaleMult, scaleMult);
                effect.transform.parent = parent;
            }

            // Effect doesn't exist
            else
            {
                // Create effect
                GameObject prefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/NearbyDamageBonusIndicator");
                effect = Instantiate(prefab, character.corePosition, Quaternion.identity);

                // Delete unneeded network components
                Destroy(effect.GetComponent<NetworkedBodyAttachment>());
                Destroy(effect.GetComponent<NetworkIdentity>());

                // Create color
                Color effectColour = new Color(0.58039215f, 0.22745098f, 0.71764705f);

                // Set material colours
                effect.transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_Color", effectColour);
                effect.transform.Find("Donut").GetComponent<MeshRenderer>().material.SetColor("_TintColor", effectColour);
                effect.transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_Color", effectColour);
                effect.transform.Find("Radius, Spherical").GetComponent<MeshRenderer>().material.SetColor("_TintColor", effectColour);

                // Set parent
                effect.transform.parent = character.modelLocator.modelTransform;

                // Calculate effect radius
                float radius = 15.0f + (count - 1) * 5.0f;  // REMEMBER TO SYNC THIS WITH LeadersPennon

                // Calculate scale multiplier
                float scaleMult = radius / 13.0f;

                // Remember parent
                Transform parent = effect.transform.parent;

                // Set scale
                effect.transform.parent = null;
                effect.transform.localScale = new Vector3(scaleMult, scaleMult, scaleMult);
                effect.transform.parent = parent;
            }
        }
    }
}
