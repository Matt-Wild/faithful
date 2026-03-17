using RoR2;
using UnityEngine;

namespace Faithful
{
    internal delegate void ItemDisplayCallback(ItemDisplaySettings _displaySettings);

    internal class WIPItem : ItemBase
    {
        // Store item
        Item item;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Display settings callback
        ItemDisplayCallback displaySettingsCallback;

        // Constructor
        public WIPItem(bool _createDisplaySettings = true, string _name = "WIP ITEM", ItemTag[] _tags = default, string _iconDir = "texTemporalCubeIcon",
            string _modelDir = "temporalcubemesh", string _displayDir = "temporalcubemesh", ItemTier _tier = ItemTier.Tier1,
            bool _simulacrumBanned = false, bool _canRemove = true, bool _hidden = false, string _corruptToken = null, 
            ItemDisplayCallback _displaySettingsCallback = null, ModifyPrefabCallback _modifyItemModelPrefabCallback = null,
            ModifyPrefabCallback _modifyItemDisplayPrefabCallback = null, bool _canNeverBeTemporary = false, bool _debugOnly = false,
            string _pickup = null, string _description = null, string _lore = null) : base(Utils.toolbox)
        {
            // Don't create item is WIP content is disabled
            if (!Utils.debugWIPContent) return;

            // Assign display settings callback
            displaySettingsCallback = _displaySettingsCallback;

            // Create display settings if asked to
            if (_createDisplaySettings) CreateDisplaySettings(_displayDir);

            // Create Copper Gear item and buff
            item = Items.AddItem(Utils.GetXMLSafeString(_name).ToUpper(), _name, _tags ?? [], _iconDir, _modelDir, _tier, _simulacrumBanned, _canRemove, 
                _hidden, _corruptToken, displaySettings, _modifyItemModelPrefabCallback, _modifyItemDisplayPrefabCallback, _canNeverBeTemporary, 
                _debugOnly, true, _name, _pickup, _description, _lore);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // If display settings callback exists, call it
            if (displaySettingsCallback != null)
            {
                displaySettingsCallback(displaySettings);
            }

            // Otherwise apply default display settings
            else
            {
                // Add character display settings
                displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Bandit", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F), "EngiTurretBody");
                displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F), "EngiWalkerTurretBody");
                displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Mercenary", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Loader", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Railgunner", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Operator", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
                displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(0F, 0F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            }
        }

        protected override void CreateSettings()
        {
        }

        public override void FetchSettings()
        {
            // Update item texts with new settings
            item.UpdateItemTexts();
        }
    }
}
