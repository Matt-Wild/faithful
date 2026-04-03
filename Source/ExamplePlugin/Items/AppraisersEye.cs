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

        // Store item settings
        Setting<bool> hiddenFromLogbookSetting;

        // Store item stats
        bool hiddenFromLogbook;

        // Constructor
        public AppraisersEye(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("appraiserseyedisplaymesh");

            // Create item and buff
            appraisersEyeItem = Items.AddItem("APPRAISERS_EYE", "Appraisers Eye", [ItemTag.Damage, ItemTag.Technology, ItemTag.WorldUnique], "texappraiserseyeicon", "appraiserseyemesh", ItemTier.VoidTier3, _displaySettings: displaySettings, _namePrefix: "Collectors Vision", _hiddenFromLogbook: true);
            scrutinizedBuff = Buffs.AddBuff("SCRUTINIZED", "Scrutinized", "texBuffScrutinizedEye", Color.white, _isDebuff: true);

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
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0.0925F, 0.25F, 0.13375F), new Vector3(355F, 37.5F, 352.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.1621F, 0.12725F), new Vector3(344F, 0F, 0F), new Vector3(0.0825F, 0.0825F, 0.0825F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.055F, 0.12375F), new Vector3(0F, 0F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(-0.93F, 3.3725F, -0.46F), new Vector3(303.75F, 180F, 180F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0F, 0.375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.5745F, -0.26F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.79125F, 0.6F), new Vector3(0F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.099F, 0.1025F), new Vector3(338.5F, 0F, 0F), new Vector3(0.0475F, 0.0475F, 0.0475F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0.0541F, 0.14375F, 0.12125F), new Vector3(345.75F, 37.5F, 358.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "Eye", new Vector3(0F, 0.8375F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0.05375F, 0.121F, 0.112F), new Vector3(345F, 31.25F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 2.145F, 1.1125F), new Vector3(350F, 0F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "Head", new Vector3(0F, 0.0675F, 0.1325F), new Vector3(355F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Head", new Vector3(0F, 0.125F, 0.1075F), new Vector3(337.5F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Hand", new Vector3(-0.019F, 0.09F, 0.0145F), new Vector3(350F, 270F, 180F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.1615F, 0.1215F), new Vector3(351.25F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "HandR", new Vector3(0.01525F, 0.144F, -0.0404F), new Vector3(348.75F, 191.25F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.2065F, 0.1195F, -0.08675F), new Vector3(287.5F, 180F, 180F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.12555F, -0.1748F, 0.05195F), new Vector3(67.5F, 313.75F, 317.5F), new Vector3(0.05125F, 0.05125F, 0.05125F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.121F, 0.1885F, -0.06F), new Vector3(290F, 202.5F, 158.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "ChestEye", new Vector3(0F, 0F, 0F), new Vector3(270F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            hiddenFromLogbookSetting = appraisersEyeItem.CreateSetting("HIDDEN_FROM_LOGBOOK", "Hide From Logbook?", true, "Should this item be hidden from the logbook?", false, _canRandomise: false, _restartRequired: true);
        }

        public override void FetchSettings()
        {
            // Get item settings
            hiddenFromLogbook = hiddenFromLogbookSetting.Value;

            // Update if hidden in logbook
            appraisersEyeItem.hiddenFromLogbook = hiddenFromLogbook;

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
