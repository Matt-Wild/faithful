using RoR2;
using UnityEngine.Networking;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace Faithful
{
    internal class DebugMode
    {
        // Store reference to debug UI
        protected GameObject ui;

        // Constructor
        public DebugMode()
        {
            // Hook behaviours
            HookBehaviours();
        }

        public void Update()
        {
            // Ensure in debug mode
            if (!Utils.debugMode)
            {
                return;
            }

            // Check if F3 pressed
            if (Input.GetKeyDown(KeyCode.F3))
            {
                // Toggle UI
                ToggleUI();
            }

            // Do not allow other debug functionality if not hosting
            if (!Utils.hosting)
            {
                return;
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
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(EquipmentCatalog.equipmentDefs.Where(x => x.nameToken == "EQUIPMENT_COMMANDMISSILE_NAME").FirstOrDefault().equipmentIndex);
                }
                else if (Input.GetKeyDown(KeyCode.Minus))
                {
                    spawn = true;
                    index = PickupCatalog.FindPickupIndex(EquipmentCatalog.equipmentDefs.Where(x => x.nameToken == "EQUIPMENT_METEOR_NAME").FirstOrDefault().equipmentIndex);
                }

                if (spawn)  // Spawn command essense of selected tier
                {
                    Transform transform = PlayerCharacterMasterController.instances[0].master.GetBodyObject().transform;

                    if (Utils.verboseConsole) Log.Debug($"Spawning item at coordinates {transform.position}");
                    GameObject essence = Object.Instantiate(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/CommandCube"), transform.position, transform.rotation);
                    essence.GetComponent<PickupIndexNetworker>().NetworkpickupIndex = index;
                    essence.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(index);
                    NetworkServer.Spawn(essence);
                }
            }
        }

        void HookBehaviours()
        {
            // Do not hook behaviours if not in debug mode
            if (!Utils.debugMode)
            {
                return;
            }

            // Add On Stage Start behaviour
            On.RoR2.Stage.Start += OnStageStart;
        }

        IEnumerator OnStageStart(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            // Create UI
            CreateUI();

            return orig(self); // Run normal processes
        }

        void CreateUI()
        {
            // Create UI
            ui = Object.Instantiate(Assets.GetObject("debugcanvas"));

            // Add Debug Controller component
            DebugController debugController = ui.AddComponent<DebugController>();
            debugController.Init();

            // Disable UI on creation
            ui.SetActive(false);
        }

        void ToggleUI()
        {
            // Toggle UI
            ui.SetActive(!ui.activeInHierarchy);
        }
    }
}
