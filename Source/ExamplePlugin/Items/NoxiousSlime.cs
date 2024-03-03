using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class NoxiousSlime
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item noxiousSlimeItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public NoxiousSlime(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("noxiousslimedisplaymesh");

            // Create Noxious Slime item
            noxiousSlimeItem = toolbox.items.AddItem("NOXIOUS_SLIME", [ItemTag.Damage], "texnoxiousslimeicon", "noxiousslimemesh", ItemTier.Tier3, _displaySettings: displaySettings);

            // Inject DoT behaviour
            toolbox.behaviour.AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRef);
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
            displaySettings.AddCharacterDisplay("Commando", "Pelvis", new Vector3(0.232F, -0.03F, -0.071F), new Vector3(12.5F, 110F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Pelvis", new Vector3(0.22F, 0.02F, -0.017F), new Vector3(20.5F, 82.5F, 163.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "Pelvis", new Vector3(-0.1815F, -0.061F, 0.085F), new Vector3(348F, 130F, 180.5F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("MUL-T", "Hip", new Vector3(0.01F, -0.45F, 1.725F), new Vector3(0F, 0F, 180F), new Vector3(0.65F, 0.65F, 0.65F));
            displaySettings.AddCharacterDisplay("Engineer", "Pelvis", new Vector3(0.278F, -0.006F, 0.009F), new Vector3(350F, 281.25F, 173.5F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Artificer", "Pelvis", new Vector3(0.2F, 0.04F, 0.098F), new Vector3(12F, 66F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "Pelvis", new Vector3(0.255F, 0.128F, 0.022F), new Vector3(350F, 264.25F, 176.5F), new Vector3(0.095F, 0.095F, 0.095F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.505F, 0.3995F, 0F), new Vector3(0F, 0F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Loader", "Pelvis", new Vector3(0.27F, 0.14F, -0.032F), new Vector3(7.5F, 100F, 190F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-2.525F, 2.05F, 3.08F), new Vector3(350F, 304F, 9.25F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "Pelvis", new Vector3(0.2875F, 0.163F, -0.085F), new Vector3(357.5F, 324.5F, 168F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Pelvis", new Vector3(0.165F, 0.066F, 0.005F), new Vector3(20.5F, 80F, 197.25F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Pelvis", new Vector3(0.05275F, 0.045F, 0.1725F), new Vector3(353.5F, 19.8F, 186.25F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Scavenger", "Backpack", new Vector3(-4.0285F, 13.375F, 0.00053F), new Vector3(359.2F, 359.85F, 10.175F), new Vector3(1F, 1F, 1F));
        }

        void OnInflictDamageOverTimeRef(ref InflictDotInfo _inflictDotInfo)
        {
            // Check if hosting
            if (!toolbox.utils.hosting)
            {
                return;
            }

            // Check for victim and attacker
            if (_inflictDotInfo.victimObject == null || _inflictDotInfo.attackerObject == null)
            {
                return;
            }

            // Attempt to get attacker body and inventory
            CharacterBody attackerBody = _inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            if (attackerBody == null || attackerBody.inventory == null)
            {
                return;
            }

            // Get item count
            int count = attackerBody.inventory.GetItemCount(noxiousSlimeItem.itemDef);

            // Has item?
            if (count > 0)
            {
                // Modify DoT
                _inflictDotInfo.damageMultiplier *= 1.0f + (1.0f * count);
            }
        }
    }
}
