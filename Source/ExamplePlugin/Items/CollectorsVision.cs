using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class CollectorsVision
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Buff inspirationBuff;
        Item collectorsVisionItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public CollectorsVision(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Get Vengeance buff
            inspirationBuff = toolbox.buffs.GetBuff("INSPIRATION");

            // Create display settings
            CreateDisplaySettings("collectorsvisiondisplaymesh");

            // Create Collector's Vision item
            collectorsVisionItem = toolbox.items.AddItem("COLLECTORS_VISION", [ItemTag.Damage], "texcollectorsvisionicon", "collectorsvisionmesh", ItemTier.VoidTier3, _corruptToken: "ITEM_CRITDAMAGE_NAME", _displaySettings: displaySettings);

            // Link On Give Item behaviour
            toolbox.behaviour.AddOnGiveItemCallback(OnGiveItem);
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
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.1738F, 0.0778F, 0.0148F), new Vector3(0F, 6F, 90F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "ThighR", new Vector3(-0.105F, -0.065F, 0.02785F), new Vector3(347.5F, 334F, 117.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighR", new Vector3(-0.09275F, 0.4F, 0.0715F), new Vector3(0F, 35F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.75F, 2.875F, -1.31F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.055F, 0.24F, 0.2575F), new Vector3(270F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            //displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.239F, 0.05525F, -0.2225F), new Vector3(9.5F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.11575F, 0.13425F, -0.115F), new Vector3(2F, 45F, 260F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0.48F, 0.425F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0535F, 0.1525F, -0.1415F), new Vector3(352F, 90F, 270F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-1.83F, 3.05F, 3.025F), new Vector3(317.2F, 186.25F, 321F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "UpperArmL", new Vector3(0.0575F, 0.1375F, -0.1255F), new Vector3(0F, 61F, 265.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.1315F, 0.461F, -0.02325F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.082F, 0.27F, -0.191F), new Vector3(8F, 280F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        void OnGiveItem(Inventory _inventory, ItemIndex _index, int _count)
        {
            // Check for valid call
            if (_inventory == null || _index == ItemIndex.None || _count <= 0)
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

            // Get item count
            int count = _inventory.GetItemCount(collectorsVisionItem.itemDef);
            if (count > 0)
            {
                // Get Collector's Vision index
                ItemIndex collectorsIndex = ItemCatalog.FindItemIndex("FAITHFUL_COLLECTORS_VISION_NAME");

                // Ensure it's not Collector's Vision
                if (_index == collectorsIndex)
                {
                    return;
                }

                // Check flag to ensure item is a first pickup for the stage
                if (helper.stageFlags.Get($"CS_{_index}_FFS"))
                {
                    return;
                }

                // Set flag
                helper.stageFlags.Set($"CS_{_index}_FFS");

                // Grant buff amount equal to item count
                for (int i = 0; i < count; i++)
                {
                    body.AddBuff(inspirationBuff.buffDef);
                }
            }
        }
    }
}
