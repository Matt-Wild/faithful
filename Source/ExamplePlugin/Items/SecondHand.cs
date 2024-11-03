using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class SecondHand
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item and buff
        Item secondHandItem;
        Buff secondHandBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public SecondHand(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("secondhanddisplaymesh");

            // Create Second Hand item and buff
            secondHandItem = Items.AddItem("SECOND_HAND", [ItemTag.Damage, ItemTag.Utility], "texsecondhandicon", "secondhandmesh", _tier: ItemTier.Tier2, _displaySettings: displaySettings);
            secondHandBuff = Buffs.AddBuff("SECOND_HAND", "texbuffsecondhand", Color.white);

            // Add stats modification
            Behaviour.AddStatsMod(secondHandBuff, SecondHandStatsMod);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.01898F, 0.26776F, 0.00182F), new Vector3(7.69423F, 1.2381F, 2.28152F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Huntress", "Muzzle", new Vector3(0F, -0.02925F, -0.02537F), new Vector3(75F, 270F, 90F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(0F, 0.1F, 0F), new Vector3(355F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.39384F, 2.95909F, -1.01878F), new Vector3(0F, 270F, 35F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.141F, 0.382F, 0.1435F), new Vector3(45F, 135F, 270F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.00525F, 0.08556F, 0.03226F), new Vector3(10F, 0F, 355F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(0.0085F, 0.152F, -0.0075F), new Vector3(0F, 18.25F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0F, -0.034F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(-0.0025F, 0.64F, -0.0055F), new Vector3(356F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(0F, 3.725F, 0F), new Vector3(355F, 180F, 0F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0F, 3.725F, 0F), new Vector3(359.765F, 267.319F, 355.005F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(-0.00353F, -0.00525F, -0.06F), new Vector3(0F, 90F, 90F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(-0.075F, -0.1475F, 0.2855F), new Vector3(0F, 90F, 270F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(-0.0025F, 0.385F, 0.0025F), new Vector3(4.5F, 0F, 350F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(5.13896F, 3.45994F, 0.08489F), new Vector3(338.495F, 358.5428F, 334.8337F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(0F, 0.3275F, -0.075F), new Vector3(0F, 315F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.01725F, 0.2145F, 0.0045F), new Vector3(358.75F, 0.0375F, 358.95F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(0.02F, 0.36575F, 0.0011F), new Vector3(1.61F, 149.31F, 0.265F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, 0.12975F, 0F), new Vector3(0F, 2.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(0F, -0.12975F, 0F), new Vector3(0F, 122.5F, 0F), new Vector3(0.05F, 0.1F, 0.05F));
        }

        void SecondHandStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += 0.20f * _count;
            _stats.moveSpeedMultAdd += 0.30f * _count;
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (characterBody && inventory)
            {
                // Get target Second Hand buff amount
                int targetSecondHandCount = _character.isGrounded ? inventory.GetItemCount(secondHandItem.itemDef) : 0;

                // Get current amount of Second Hand buffs
                int currentSecondHandCount = characterBody.GetBuffCount(secondHandBuff.buffDef);

                // Check if character has the wrong amount of buffs
                if (targetSecondHandCount != currentSecondHandCount)
                {
                    // Update Second Hand buff count
                    characterBody.SetBuffCount(secondHandBuff.buffDef.buffIndex, targetSecondHandCount);
                }
            }
        }
    }
}
