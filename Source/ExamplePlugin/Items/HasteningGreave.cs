using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class HasteningGreave
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item hasteningGreaveItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public HasteningGreave(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("hasteninggreavedisplaymesh");

            // Create Longshot Geode item
            hasteningGreaveItem = toolbox.items.AddItem("HASTENING_GREAVE", [ItemTag.Utility], "texhasteninggreaveicon", "hasteninggreavemesh", ItemTier.Lunar, _displaySettings: displaySettings);

            // Add On Recalculate Stats behaviour
            toolbox.behaviour.AddOnRecalculateStatsCallback(OnRecalculateStats);
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
            displaySettings.AddCharacterDisplay("Commando", "CalfL", new Vector3(0.0025F, 0.225F, 0.0235F), new Vector3(2F, 175F, 182.5F), new Vector3(0.19F, 0.19F, 0.19F));
            displaySettings.AddCharacterDisplay("Huntress", "CalfL", new Vector3(-0.02F, 0.265F, 0.0215F), new Vector3(2.5F, 190F, 181F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Bandit", "CalfL", new Vector3(0F, 0.225F, 0.01215F), new Vector3(0F, 180F, 180F), new Vector3(0.16F, 0.16F, 0.16F));
            displaySettings.AddCharacterDisplay("MUL-T", "CalfL", new Vector3(0F, 2.25F, 0F), new Vector3(0F, 0F, 180F), new Vector3(1.75F, 1.75F, 1.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CalfL", new Vector3(-0.003F, 0.1875F, 0.02F), new Vector3(2.5F, 180F, 182.5F), new Vector3(0.225F, 0.2F, 0.24F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.0375F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.5F, 0.25F, 0.5F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfL", new Vector3(-0.0055F, 0.4045F, 0.01F), new Vector3(5F, 185F, 183.5F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Mercenary", "CalfL", new Vector3(0.0032F, 0.245F, 0.022F), new Vector3(5F, 185F, 181F), new Vector3(0.14F, 0.14F, 0.1475F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0F, 0.85F, 0F), new Vector3(0F, 180F, 180F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "CalfL", new Vector3(-0.009F, 0.25F, 0.0135F), new Vector3(5F, 175F, 180F), new Vector3(0.225F, 0.2F, 0.225F));
            displaySettings.AddCharacterDisplay("Acrid", "CalfL", new Vector3(-0.065F, 3.8F, 0F), new Vector3(2F, 0F, 180F), new Vector3(0.85F, 0.8F, 0.85F));
            displaySettings.AddCharacterDisplay("Captain", "CalfL", new Vector3(0F, 0.3F, 0.0125F), new Vector3(5F, 195F, 180F), new Vector3(0.155F, 0.125F, 0.15F));
            displaySettings.AddCharacterDisplay("Railgunner", "CalfL", new Vector3(0F, 0.325F, 0.00825F), new Vector3(0F, 85F, 180F), new Vector3(0.195F, 0.15F, 0.195F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(0.0025F, 0.3775F, -0.00021F), new Vector3(5F, 100F, 183.5F), new Vector3(0.14F, 0.125F, 0.14F));
            displaySettings.AddCharacterDisplay("Scavenger", "CalfL", new Vector3(0F, 1.55F, -0.145F), new Vector3(8.5F, 180F, 180F), new Vector3(1.7F, 0.75F, 1.7F));
        }

        void OnRecalculateStats(CharacterBody _body)
        {
            // Check for Character Body
            if (_body == null)
            {
                return;
            }

            // Check for inventory
            if (!_body.inventory)
            {
                return;
            }

            // Get item count
            int count = _body.inventory.GetItemCount(hasteningGreaveItem.itemDef);

            // Modify stats multiplicatively
            _body.attackSpeed *= Mathf.Pow(2.0f, count);
            _body.damage *= Mathf.Pow(0.5f, count);
        }
    }
}
