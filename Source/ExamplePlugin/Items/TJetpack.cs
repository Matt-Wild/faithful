﻿using RoR2;
using UnityEngine;

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
            // Velocity = -1? or 32?
            // Acceleration = 60?

            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("4t0njetpackdisplaymesh");

            // Create item
            item = toolbox.items.AddItem("4T0N_JETPACK", [ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.BrotherBlacklist], "tex4t0njetpackicon", "4t0njetpackmesh", ItemTier.Tier3, _displaySettings: displaySettings);

            // Inject on transfer item behaviours
            toolbox.behaviour.AddOnGiveItemCallback(OnGiveItem);
            toolbox.behaviour.AddOnRemoveItemCallback(OnRemoveItem);
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = toolbox.utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!toolbox.assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3F, -0.235F), new Vector3(345F, 180F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.1125F, 0.15F, -0.135F), new Vector3(0F, 135F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0.0025F, 0.1295F, -0.225F), new Vector3(355F, 180F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 1.1F, -2.1F), new Vector3(0F, 180F, 0F), new Vector3(1.85F, 1.85F, 1.85F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.18F, -0.33F), new Vector3(345F, 180F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.0025F, 0.065F, -0.2525F), new Vector3(350F, 180F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0.1875F, -0.265F), new Vector3(345F, 180F, 0F), new Vector3(0.32F, 0.32F, 0.32F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.15F, -0.95F), new Vector3(0F, 180F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0F, 0.125F, -0.205F), new Vector3(350F, 179.6F, 0F), new Vector3(0.35F, 0.35F, 0.35F));
            displaySettings.AddCharacterDisplay("Acrid", "SpineStomach1", new Vector3(0F, 1.25F, 1F), new Vector3(270F, 0F, 0F), new Vector3(3F, 3F, 3F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.125F, -0.3075F), new Vector3(0F, 180F, 0F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0F, -0.275F, 0.0025F), new Vector3(0F, 178.5F, 0F), new Vector3(0.375F, 0.375F, 0.375F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Stomach", new Vector3(-0.001F, 0F, -0.2F), new Vector3(5F, 179F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
        }

        void OnGiveItem(Inventory _inventory, ItemIndex _index, int _count)
        {
            // Check for valid call
            if (_inventory == null || _index == ItemIndex.None || _count <= 0)
            {
                return;
            }

            // Get TJetpack index
            ItemIndex jetpackIndex = ItemCatalog.FindItemIndex("FAITHFUL_4T0N_JETPACK_NAME");

            // Ensure correct index
            if (jetpackIndex != _index)
            {
                return;
            }

            // Attempt to get Character Body
            CharacterBody body = toolbox.utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Attempt to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = body.gameObject.GetComponent<FaithfulCharacterBodyBehaviour>();
            if (helper == null)
            {
                return;
            }

            // Get new item count
            int count = _inventory.GetItemCount(item.itemDef);

            // Update TJetpack item count
            helper.tJetpack.UpdateItemCount(count + _count);
        }

        void OnRemoveItem(Inventory _inventory, ItemIndex _index, int _count)
        {
            // Check for valid call
            if (_inventory == null || _index == ItemIndex.None || _count <= 0)
            {
                return;
            }

            // Get TJetpack index
            ItemIndex jetpackIndex = ItemCatalog.FindItemIndex("FAITHFUL_4T0N_JETPACK_NAME");

            // Ensure correct index
            if (jetpackIndex != _index)
            {
                return;
            }

            // Attempt to get Character Body
            CharacterBody body = toolbox.utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Attempt to get Faithful behaviour
            FaithfulCharacterBodyBehaviour helper = body.gameObject.GetComponent<FaithfulCharacterBodyBehaviour>();
            if (helper == null)
            {
                return;
            }

            // Get new item count
            int count = _inventory.GetItemCount(item.itemDef);

            // Update TJetpack item count
            helper.tJetpack.UpdateItemCount(count - _count);
        }
    }
}