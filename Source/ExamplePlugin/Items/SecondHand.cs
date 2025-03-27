using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class SecondHand : ItemBase
    {
        // Store item and buff
        Item secondHandItem;
        Buff secondHandBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> attackSpeedSetting;
        Setting<float> attackSpeedStackingSetting;
        Setting<float> speedSetting;
        Setting<float> speedStackingSetting;

        // Store item stats
        float attackSpeed;
        float attackSpeedStacking;
        float speed;
        float speedStacking;

        // Constructor
        public SecondHand(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("secondhanddisplaymesh");

            // Create Second Hand item and buff
            secondHandItem = Items.AddItem("SECOND_HAND", [ItemTag.Damage, ItemTag.Utility], "texsecondhandicon", "secondhandmesh", _tier: ItemTier.Tier2, _displaySettings: displaySettings);
            secondHandBuff = Buffs.AddBuff("SECOND_HAND", "texbuffsecondhand", Color.white);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmR", new Vector3(-0.005F, 0.25F, -0.064F), new Vector3(0F, 0F, 180F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Huntress", "BowHinge2L", new Vector3(-0.0302F, 0.05125F, 0.00413F), new Vector3(0F, 90F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "BowHinge2R", new Vector3(-0.0302F, 0.05125F, -0.00413F), new Vector3(0F, 90F, 180F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighL", new Vector3(0.06113F, 0.3375F, 0.08F), new Vector3(359.25F, 36.95F, 172.5F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.875F, 0.7125F, 0.25F), new Vector3(0F, 90F, 270F), new Vector3(1.5F, 1.5F, 1.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(-0.195F, 0.375F, 0.045F), new Vector3(0F, 90F, 180F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Turret", "LegBar1", new Vector3(0.2675F, 1.05F, -0.07F), new Vector3(0F, 270F, 180F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.1375F, 0.09825F), new Vector3(338.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmR", new Vector3(-0.02425F, 0.152F, -0.10675F), new Vector3(3.5F, 18.25F, 177.75F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "CalfFrontR", new Vector3(0.05425F, 0.5F, -0.0325F), new Vector3(0F, 90F, 0F), new Vector3(0.375F, 0.375F, 0.375F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmR", new Vector3(-0.045F, 0.26F, -0.0825F), new Vector3(0F, 1.5F, 175.75F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Acrid", "MouthMuzzle", new Vector3(0F, -0.745F, 1.675F), new Vector3(49.5F, 0F, 0F), new Vector3(1.5F, 1.5F, 2.5F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(0.095F, 0F, -0.0855F), new Vector3(0F, 90F, 91.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(0F, 0.2375F, 0.3125F), new Vector3(90F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(0F, 0.175F, 0.1125F), new Vector3(325F, 0F, 180F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.25F, 0.101F), new Vector3(334F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "HandL", new Vector3(0.74F, 0.41825F, 0.0625F), new Vector3(293.75F, 264.85F, 184.95F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Chef", "Cleaver", new Vector3(-0.01425F, 0.445F, -0.00125F), new Vector3(0F, 0F, 0F), new Vector3(0.15F, 0.15F, 1F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(-0.01425F, 0.445F, -0.00125F), new Vector3(0F, 0F, 0F), new Vector3(0.15F, 0.15F, 1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            attackSpeedSetting = secondHandItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 20.0f, "How much should this item increase attack speed while touching the ground? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            attackSpeedStackingSetting = secondHandItem.CreateSetting("ATTACK_SPEED_STACKING", "Attack Speed Stacking", 20.0f, "How much should further stacks of this item increase attack speed while touching the ground? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            speedSetting = secondHandItem.CreateSetting("SPEED", "Movement Speed", 30.0f, "How much should this item increase movement speed while touching the ground? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingSetting = secondHandItem.CreateSetting("SPEED_STACKING", "Movement Speed Stacking", 30.0f, "How much should further stacks of this item increase movement speed while touching the ground? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            attackSpeedStacking = attackSpeedStackingSetting.Value / 100.0f;
            speed = speedSetting.Value / 100.0f;
            speedStacking = speedStackingSetting.Value / 100.0f;

            // Update item texts with new settings
            secondHandItem.UpdateItemTexts();
        }

        void SecondHandStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += _count > 1 ? attackSpeed + (attackSpeedStacking * (_count - 1)) : attackSpeed;
            _stats.moveSpeedMultAdd += _count > 1 ? speed + (speedStacking * (_count - 1)) : speed;
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (characterBody && inventory)
            {
                // Get target Second Hand buff amount
                int targetSecondHandCount = _character.isGrounded || _character.characterMotor == null ? inventory.GetItemCount(secondHandItem.itemDef) : 0;

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
