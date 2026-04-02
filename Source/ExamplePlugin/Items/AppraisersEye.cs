using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class AppraisersEye : ItemBase
    {
        // Store item and buff
        Item appraisersEyeItem;
        Buff scrutinizedBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Constructor
        public AppraisersEye(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            //CreateDisplaySettings("TemporalCubeMesh");

            // Create item and buff
            appraisersEyeItem = Items.AddItem("APPRAISERS_EYE", "Appraisers Eye", [ItemTag.Damage, ItemTag.Technology, ItemTag.AIBlacklist, ItemTag.WorldUnique], "texTemporalCubeIcon", "TemporalCubeMesh", ItemTier.VoidTier3, _corruptToken: "ITEM_CRITDAMAGE_NAME", _displaySettings: displaySettings, _WIP: true, _namePrefix: "Collectors Vision", _hiddenFromLogbook: true);
            scrutinizedBuff = Buffs.AddBuff("SCRUTINIZED", "Scrutinized", "texTemporalCubeIcon", Color.white, _isDebuff: true);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);
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
            displaySettings.AddCharacterDisplay("Commando", "GunL", new Vector3(0.205F, 0F, 0F), new Vector3(0F, 90F, 180F), new Vector3(0.0525F, 0.0525F, 0.0525F));
            displaySettings.AddCharacterDisplay("Commando", "GunR", new Vector3(-0.205F, 0F, 0F), new Vector3(0F, 270F, 180F), new Vector3(0.0525F, 0.0525F, 0.0525F));
            displaySettings.AddCharacterDisplay("Huntress", "BowBase", new Vector3(0F, -0.0145F, -0.0775F), new Vector3(90F, 180F, 0F), new Vector3(0.07F, 0.07F, 0.07F));
            displaySettings.AddCharacterDisplay("Bandit", "SideWeapon", new Vector3(0F, -0.1165F, 0.1925F), new Vector3(90F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(2.275F, 2.475F, -0.35F), new Vector3(300F, 180F, 90F), new Vector3(0.725F, 0.725F, 0.725F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.11475F, 0.395F, 0.22F), new Vector3(270F, 225F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(-0.11475F, 0.395F, 0.22F), new Vector3(270F, 135F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0.805F, 0.54618F, 1.8275F), new Vector3(0F, 0F, 270F), new Vector3(0.1125F, 0.1125F, 0.1125F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0.615F, 0.7805F, 0.71F), new Vector3(0F, 0F, 270F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmL", new Vector3(0F, 0.22075F, -0.20375F), new Vector3(270F, 2.5F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Artificer", "LowerArmR", new Vector3(0.0225F, 0.2225F, 0.2045F), new Vector3(270F, 182.5F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0.1865F, 0.0715F, 0.0675F), new Vector3(2.5F, 355F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "AimOriginSyringe", new Vector3(0.2825F, 0.09425F, -0.1575F), new Vector3(0F, 0F, 270F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0.181F, 0.1635F, 0.02625F), new Vector3(0F, 0F, 282.5F), new Vector3(0.065F, 0.065F, 0.065F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(-1.0431F, 4.035F, -1.631F), new Vector3(275F, 35F, 358F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0.1525F, 0.0625F, -0.0075F), new Vector3(271F, 314.5F, 318.5F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(0F, 0.32F, 0.08F), new Vector3(0F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Hand", new Vector3(0.0965F, 0.1135F, 0.01375F), new Vector3(275F, 270F, 2F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Seeker", "HandL", new Vector3(-0.071F, -0.016F, -0.1245F), new Vector3(295.5F, 357.5F, 40F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "HandR", new Vector3(-0.01875F, -0.045F, 0.1425F), new Vector3(286.25F, 207.25F, 317.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(-0.399F, 0.2925F, 0.1575F), new Vector3(0F, 0F, 165F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Cleaver", new Vector3(0.08025F, 0.5105F, 0F), new Vector3(270F, 270F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Chef", "PizzaCutter", new Vector3(-0.18175F, 0.4075F, -0.00025F), new Vector3(270F, 90F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "HandL", new Vector3(-0.094F, 0.11F, -0.0165F), new Vector3(270F, 90F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Technician", "HandR", new Vector3(0.094F, 0.11F, -0.0165F), new Vector3(270F, 270F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Operator", "MuzzleGun", new Vector3(0F, 0.105F, 0.0348F), new Vector3(2.5F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(-0.275F, -0.023F, -0.295F), new Vector3(275F, 315F, 120F), new Vector3(0.125F, 0.125F, 0.125F));
        }

        protected override void CreateSettings()
        {
        }

        public override void FetchSettings()
        {
            // Update item texts with new settings
            appraisersEyeItem.UpdateItemTexts();
        }

        private void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check if crit
            if (_report.crit == false) return;

            // Check for attacker and victim
            if (_attacker == null || _victim == null) return;

            // Attempt to get attacker and victim bodies
            CharacterBody attackerBody = _attacker.GetBody();
            CharacterBody victimBody = _victim.GetBody();
            if (attackerBody == null || victimBody == null) return;

            // Attempt to get attacker inventory
            Inventory attackerInventory = attackerBody.inventory;
            if (attackerInventory == null) return;

            // Get attacker item count and victim debuff count
            int attackerItemCount = attackerInventory.GetItemCountEffective(appraisersEyeItem.itemDef.itemIndex);
            int victimDebuffCount = victimBody.GetBuffCount(scrutinizedBuff.buffDef.buffIndex);

            // Check if can add debuff
            if (attackerItemCount > victimDebuffCount)
            {
                // Add debuff to victim
                victimBody.AddBuff(scrutinizedBuff.buffDef.buffIndex);
            }

            // Increase damage based on debuff count
            _report.damage *= 1.0f + (0.2f * victimDebuffCount);
        }
    }
}
