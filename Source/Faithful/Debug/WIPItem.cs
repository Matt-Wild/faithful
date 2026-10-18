using RoR2;
using UnityEngine;

namespace Faithful
{
    internal delegate void ItemDisplayCallback(ItemDisplaySettings _displaySettings);

    internal class WIPItem : ItemBase
    {
        // Store display settings
        ItemDisplaySettings displaySettings;

        // Display settings callback
        ItemDisplayCallback displaySettingsCallback;

        // Constructor
        public WIPItem(WIPItemOptions _options)
            : base(Utils.toolbox, _options.Item.Token)
        {
            // Don't create item is WIP content is disabled
            if (!Utils.WIPContentEnabled) return;

            ItemOptions itemOptions = _options.Item.Copy();

            // Assign display settings callback
            displaySettingsCallback = _options.DisplaySettingsCallback;

            // Create display settings if asked to
            if (_options.CreateDisplaySettings) CreateDisplaySettings(_options.DisplayModelName);

            // Create the WIP item with its generated display settings
            itemOptions.DisplaySettings = displaySettings;
            MainItem = Items.AddItem(itemOptions);

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
            MainItem.UpdateItemTexts();
        }
    }
}
