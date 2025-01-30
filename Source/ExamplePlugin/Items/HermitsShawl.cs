using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class HermitsShawl : ItemBase
    {
        // Store item and buff
        Buff buff;
        Item item;

        // Store reference to buff behaviour
        Patience buffBehaviour;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<int> maxBuffsStackingSetting;
        Setting<float> buffCooldownSetting;
        Setting<float> damageSetting;

        // Constructor
        public HermitsShawl(Toolbox _toolbox, Patience _patience) : base(_toolbox)
        {
            // Assign vengeance behaviour
            buffBehaviour = _patience;

            // Get buff
            buff = Buffs.GetBuff("PATIENCE");

            // Create display settings
            CreateDisplaySettings("HermitShawlDisplayMesh");

            // Create item
            item = Items.AddItem("HERMITS_SHAWL", [ItemTag.Damage], "texHermitShawlIcon", "HermitShawlMesh", ItemTier.Tier2, _displaySettings: displaySettings, _debugOnly: true);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Damage Dealt behaviour
            Behaviour.AddOnDamageDealtCallback(OnDamageDealt);
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
            displaySettings.AddCharacterDisplay("Commando", "ThighR", new Vector3(-0.1738F, 0.0778F, 0.0148F), new Vector3(0F, 6F, 90F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "ThighR", new Vector3(-0.105F, -0.065F, 0.02785F), new Vector3(347.5F, 334F, 117.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "ThighR", new Vector3(-0.09275F, 0.4F, 0.0715F), new Vector3(0F, 35F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(-1.75F, 2.875F, -1.31F), new Vector3(0F, 0F, 0F), new Vector3(0.75F, 0.75F, 0.75F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.055F, 0.24F, 0.2575F), new Vector3(270F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            //displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.3F, 0F), new Vector3(0F, 270F, 0F), new Vector3(0.3F, 0.3F, 0.3F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(-0.239F, 0.05525F, -0.2225F), new Vector3(9.5F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "ThighL", new Vector3(0.11575F, 0.13425F, -0.115F), new Vector3(2F, 45F, 260F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "Chest", new Vector3(0.48F, 0.425F, 0F), new Vector3(0F, 0F, 270F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0535F, 0.1525F, -0.1415F), new Vector3(352F, 90F, 270F), new Vector3(0.115F, 0.115F, 0.115F));
            displaySettings.AddCharacterDisplay("Acrid", "Chest", new Vector3(-1.83F, 3.05F, 3.025F), new Vector3(317.2F, 186.25F, 321F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "UpperArmL", new Vector3(0.0575F, 0.1375F, -0.1255F), new Vector3(0F, 61F, 265.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.1315F, 0.461F, -0.02325F), new Vector3(0F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ForeArmR", new Vector3(0.082F, 0.27F, -0.191F), new Vector3(8F, 280F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.3675F, 0.012F, -0.21F), new Vector3(5F, 180F, 250F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(0.20668F, 0.2025F, 0.021F), new Vector3(4.5F, 40.5F, 15.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(0.125F, -0.4F, -0.15F), new Vector3(85F, 12.5F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            maxBuffsStackingSetting = item.CreateSetting("MAX_BUFFS_STACKING", "Max Buffs", 4, "How many stacks of patience should the player receive per stack of this item? (4 = 4 stacks)");
            buffCooldownSetting = item.CreateSetting("BUFF_RECHARGE", "Buff Recharge Time", 10.0f, "After leaving combat how long does it take to receive the maximum amount of patience? (10.0 = 10 seconds)");
            damageSetting = item.CreateSetting("DAMAGE", "Damage", 25.0f, "How much should each stack of patience increase damage? (25.0 = 25% increase)");
        }

        public override void FetchSettings()
        {
            // Apply damage to buff
            buffBehaviour.damage = damageSetting.Value / 100.0f;

            // Update item texts with new settings
            item.UpdateItemTexts();
        }

        void OnDamageDealt(DamageReport _report)
        {
            // Ignore DoTs
            if (_report.dotType != DotController.DotIndex.None) return;

            // Check for attacker body
            CharacterBody attacker = _report.attackerBody;
            if (attacker != null)
            {
                // Get patience buff count
                int buffCount = attacker.GetBuffCount(buff.buffDef);
                
                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    attacker.SetBuffCount(buff.buffDef.buffIndex, 0);

                    // Get faithful helper
                    FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(attacker);
                    if (helper != null)
                    {
                        // Get hermit's shawl behaviour
                        FaithfulHermitsShawlBehaviour behaviour = helper.hermitsShawl;
                        if (behaviour != null)
                        {
                            // Force attacker into combat
                            behaviour.ForceIntoCombat();
                        }
                    }
                }
            }

            // Check for victim body
            CharacterBody victim = _report.victimBody;
            if (victim != null)
            {
                // Get patience buff count
                int buffCount = victim.GetBuffCount(buff.buffDef);

                // Check for buff
                if (buffCount > 0)
                {
                    // Remove patience buff
                    victim.SetBuffCount(buff.buffDef.buffIndex, 0);
                }
            }
        }
    }
}
