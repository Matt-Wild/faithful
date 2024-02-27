using RoR2;
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
            displaySettings.AddCharacterDisplay("Commando", "HandL", new Vector3(0.00635F, 0.0875F, 0.0875F), new Vector3(355F, 15F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Huntress", "HandL", new Vector3(-0.005F, 0.079F, 0.065F), new Vector3(5F, 355F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Bandit", "MainWeapon", new Vector3(-0.05F, 0.25F, -0.0425F), new Vector3(0F, 180F, 180F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("MUL-T", "LowerArmL", new Vector3(0.135F, 3.2F, 1.5F), new Vector3(0F, 0F, 55F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Engineer", "HandL", new Vector3(0.005F, 0.11F, 0.09F), new Vector3(0F, 345F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0F, 0.1775F, -0.1325F), new Vector3(25F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "HandL", new Vector3(-0.005F, 0.124F, 0.097F), new Vector3(12.5F, 345F, 10F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, -0.087F, 0.559F), new Vector3(10F, 0F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Loader", "MechHandL", new Vector3(-0.075F, 0.15F, 0.18F), new Vector3(5F, 325F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "HandL", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 120F), new Vector3(0.8F, 0.8F, 0.8F));
            //displaySettings.AddCharacterDisplay("Acrid", "HandR", new Vector3(0F, 0F, 0F), new Vector3(45F, 0F, 0F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0F, 0.125F, -0.064F), new Vector3(0F, 95F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Railgunner", "TopRail", new Vector3(0F, 0.6525F, 0.0585F), new Vector3(0F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ElbowR", new Vector3(0.01F, -0.0125F, 0.005F), new Vector3(345F, 90F, 60F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0F, 18.25F, 0F), new Vector3(280F, 330F, 90F), new Vector3(2.5F, 2.5F, 2.5F));
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
