﻿using R2API;
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

            // Attempt to get attacker body
            CharacterBody attackerBody = _inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
            if (attackerBody == null)
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