using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class MeltingWarbler
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item meltingWarblerItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public MeltingWarbler(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("meltingwarblerdisplaymesh");

            // Create Melting Warbler item
            meltingWarblerItem = Items.AddItem("MELTING_WARBLER", [ItemTag.Utility], "texmeltingwarblericon", "meltingwarblermesh", ItemTier.VoidTier2, _corruptToken: "ITEM_JUMPBOOST_NAME", _displaySettings: displaySettings);

            // Add stats modification
            Behaviour.AddStatsMod(meltingWarblerItem, MeltingWarblerStatsMod);
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

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0F, 0.46155F, 0.00827F), new Vector3(15F, 0F, 0F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.3575F, -0.025F), new Vector3(10F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.275F, 0.02F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0F, 3F, 2.15F), new Vector3(305F, 180F, 0F), new Vector3(0.6F, 0.6F, 0.6F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.7715F, 0.0975F), new Vector3(15F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            //displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.25F, -0.0375F), new Vector3(25F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0F, 0.325F, 0.07F), new Vector3(12.5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "HandL", new Vector3(0F, 0.3F, 0.125F), new Vector3(280F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0F, 0.3F, 0.0375F), new Vector3(5F, 0F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 1.4325F, 2.15F), new Vector3(280F, 180F, 0F), new Vector3(1.35F, 1.35F, 1.35F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0.3415F, 0.5145F, -0.045F), new Vector3(5F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunStock", new Vector3(-0.001F, -0.015F, 0.09F), new Vector3(85.00005F, 180F, 180F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(0F, 0.25F, -0.065F), new Vector3(345F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Scavenger", "Chest", new Vector3(0F, 7.25F, 1.4F), new Vector3(350F, 180F, 0F), new Vector3(2.4F, 2.4F, 2.4F));
        }

        void MeltingWarblerStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify jump power
            _stats.baseJumpPowerAdd += 1.8f * _count;
        }
    }
}
