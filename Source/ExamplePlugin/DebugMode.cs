using RoR2;
using UnityEngine.Networking;
using UnityEngine;

namespace Faithful
{
    internal class DebugMode
    {
        // Toolbox
        protected Toolbox toolbox;

        // Constructor
        public DebugMode(Toolbox _toolbox)
        {
            toolbox = _toolbox;
        }

        public void Update()
        {
            if (!toolbox.utils.debugMode)
            {
                return; // Update contains only debug functionality
            }

            // Is F1 key down - Item spawning
            if (Input.GetKey(KeyCode.F1))
            {
                bool spawn = false;
                PickupIndex index = PickupCatalog.FindPickupIndex(ItemTier.Tier1);

                // Item tier picking behaviour
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    spawn = true;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Tier3);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Boss);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.Lunar);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidTier3);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(ItemTier.VoidBoss);
                }

                if (spawn)  // Spawn command essense of selected tier
                {
                    Transform transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                    Log.Debug($"Spawning item at coordinates {transform.position}");
                    GameObject essence = GameObject.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CommandCube"), transform.position, transform.rotation);
                    essence.GetComponent<PickupIndexNetworker>().NetworkpickupIndex = index;
                    essence.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(index);
                    NetworkServer.Spawn(essence);
                }
            }
        }
    }
}
