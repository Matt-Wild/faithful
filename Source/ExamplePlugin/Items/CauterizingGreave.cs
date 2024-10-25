using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class CauterizingGreave
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item cauterizingGreaveItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public CauterizingGreave(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("cauterizinggreavedisplaymesh");

            // Create Longshot Geode item
            cauterizingGreaveItem = Items.AddItem("CAUTERIZING_GREAVE", [ItemTag.Utility], "texcauterizinggreaveicon", "cauterizinggreavemesh", ItemTier.Lunar, _displaySettings: displaySettings);

            // Add stats modification
            Behaviour.AddStatsMod(cauterizingGreaveItem, StatsMod);

            // Add On Heal behaviour
            Behaviour.AddOnHealCallback(OnHeal);
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
            displaySettings.AddCharacterDisplay("Commando", "CalfR", new Vector3(-0.0025F, 0.225F, 0.0235F), new Vector3(2F, 175F, 177.5F), new Vector3(0.19F, 0.19F, 0.19F));
            displaySettings.AddCharacterDisplay("Huntress", "CalfR", new Vector3(0.02F, 0.26F, 0.0215F), new Vector3(1.5F, 170F, 179F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Bandit", "CalfR", new Vector3(0F, 0.225F, 0.01215F), new Vector3(0F, 175.5F, 180F), new Vector3(0.16F, 0.16F, 0.16F));
            displaySettings.AddCharacterDisplay("MUL-T", "CalfR", new Vector3(0F, 2.25F, 0F), new Vector3(0F, 0F, 180F), new Vector3(1.75F, 1.75F, 1.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CalfR", new Vector3(0.003F, 0.1875F, 0.02F), new Vector3(2.5F, 180F, 177.5F), new Vector3(0.225F, 0.2F, 0.24F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.295F, 0F), new Vector3(0F, 180F, 0F), new Vector3(0.5F, 0.25F, 0.5F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.0055F, 0.4045F, 0.01F), new Vector3(5F, 175F, 176.5F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Mercenary", "CalfR", new Vector3(-0.0032F, 0.245F, 0.022F), new Vector3(5F, 175F, 179F), new Vector3(0.14F, 0.14F, 0.1475F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontR", new Vector3(0F, 0.85F, 0F), new Vector3(0F, 180F, 180F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "CalfR", new Vector3(0.009F, 0.25F, 0.015F), new Vector3(2.5F, 185F, 180F), new Vector3(0.225F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Acrid", "CalfR", new Vector3(0.065F, 3.8F, -0.0075F), new Vector3(2F, 0F, 180F), new Vector3(0.85F, 0.8F, 0.85F));
            displaySettings.AddCharacterDisplay("Captain", "CalfR", new Vector3(0F, 0.3F, 0.0125F), new Vector3(5F, 165F, 180F), new Vector3(0.155F, 0.125F, 0.15F));
            displaySettings.AddCharacterDisplay("Railgunner", "CalfR", new Vector3(0F, 0.325F, 0.00825F), new Vector3(0F, 280F, 180F), new Vector3(0.195F, 0.15F, 0.195F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfR", new Vector3(0.00335F, 0.378F, 0F), new Vector3(5F, 260F, 176.5F), new Vector3(0.13F, 0.125F, 0.14F));
            displaySettings.AddCharacterDisplay("Scavenger", "CalfR", new Vector3(0.025F, 1.55F, -0.16F), new Vector3(8.5F, 180F, 180F), new Vector3(1.7F, 0.75F, 1.7F));
            displaySettings.AddCharacterDisplay("Seeker", "Pelvis", new Vector3(-0.096F, -0.17875F, 0.1775F), new Vector3(316F, 305F, 160F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(0.0146F, 0.3535F, -0.03525F), new Vector3(10F, 275F, 95F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "Pelvis", new Vector3(5.19109F, 2.45005F, -2.17756F), new Vector3(342.2899F, 335.8934F, 339.8638F), new Vector3(1F, 1F, 1F));
        }

        void StatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Increase max health
            _stats.healthMultAdd += 1.0f * _count;
        }

        void OnHeal(HealthComponent _healthComponent, ref float _amount, ref ProcChainMask _procChainMask, ref bool _nonRegen)
        {
            // This is not regen behaviour
            if (!_nonRegen)
            {
                return;
            }

            // Attempt to get Character Body
            CharacterBody body = _healthComponent.gameObject.GetComponent<CharacterBody>();
            if (body != null)
            {
                // Check for inventory
                if (!body.inventory)
                {
                    return;
                }

                // Get item count
                int count = body.inventory.GetItemCount(cauterizingGreaveItem.itemDef);

                // Has item?
                if (count > 0)
                {
                    // Increase max health on top of items
                    _amount *= Mathf.Pow(0.5f, count);
                }
            }
        }
    }
}
