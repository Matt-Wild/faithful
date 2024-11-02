using RoR2;
using System.Reflection;
using UnityEngine;
using static Facepunch.Steamworks.Inventory.Item;

namespace Faithful
{
    internal class TJetpack
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item item;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public TJetpack(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("4t0njetpackdisplaymesh");

            // Create item
            item = Items.AddItem("4T0N_JETPACK", [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist], "tex4t0njetpackicon", "4t0njetpackmesh", ItemTier.Tier3, _displaySettings: displaySettings, _debugOnly: true);

            // Inject on transfer item behaviours
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);

            // Inject character body behaviours
            Behaviour.AddOnCharacterBodyStartCallback(OnCharacterBodyStart);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName, _useHopooShader: false);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3F, -0.235F), new Vector3(345F, 180F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.1125F, 0.15F, -0.135F), new Vector3(0F, 135F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0.007F, 0.1295F, -0.225F), new Vector3(0F, 172.5F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 1.1F, -2.1F), new Vector3(0F, 180F, 0F), new Vector3(1.85F, 1.85F, 1.85F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.2475F, -0.325F), new Vector3(345F, 180F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.00125F, 0.1025F, -0.25F), new Vector3(350F, 180F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.1875F, -0.265F), new Vector3(345F, 180F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.15F, -0.95F), new Vector3(0F, 180F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0F, 0.125F, -0.205F), new Vector3(350F, 179.6F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Acrid", "SpineStomach1", new Vector3(0F, 1.25F, 1F), new Vector3(270F, 0F, 0F), new Vector3(3F, 3F, 3F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.125F, -0.3075F), new Vector3(0F, 180F, 0F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(-0.00375F, -0.2875F, 0.02425F), new Vector3(0F, 178.75F, 0F), new Vector3(0.3725F, 0.3725F, 0.3725F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Stomach", new Vector3(-0.001F, 0F, -0.2F), new Vector3(5F, 179F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Seeker", "Pelvis", new Vector3(0F, 0.1575F, -0.21F), new Vector3(345F, 180F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0.00375F, 0F, -0.46F), new Vector3(22.5F, 180.25F, 0.25F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.0375F, -0.3F, 0F), new Vector3(65F, 270F, 0F), new Vector3(0.375F, 0.375F, 0.375F));
        }

        void OnInventoryChanged(Inventory _inventory)
        {
            // Attempt to get Character Body
            CharacterBody body = Utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Attempt to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(body);
            if (helper == null)
            {
                return;
            }

            // Get new item count
            int count = _inventory.GetItemCount(item.itemDef);

            // Update TJetpack item count
            helper.tJetpack.UpdateItemCount(count);
        }

        void OnCharacterBodyStart(CharacterBody _character)
        {
            // Check if valid
            if (_character == null || _character.inventory == null)
            {
                return;
            }

            // Get item count
            int count = _character.inventory.GetItemCount(item.itemDef);

            // Has item?
            if (count > 0)
            {
                // Attempt to get Faithful behaviour
                FaithfulCharacterBodyBehaviour helper = _character.gameObject.transform.Find("Faithful Helper").GetComponent<FaithfulCharacterBodyBehaviour>();
                if (helper == null)
                {
                    return;
                }

                // Update TJetpack item count
                helper.tJetpack.UpdateItemCount(count);
            }
        }
    }
}
