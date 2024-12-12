using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class LongshotGeode
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store item
        Item longshotGeodeItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> distanceSetting;

        // Store item stats
        float damage;
        float damageStacking;
        float distanceThreshold;

        // Constructor
        public LongshotGeode(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Create display settings
            CreateDisplaySettings("longshotgeodedisplaymesh");

            // Create Longshot Geode item
            longshotGeodeItem = Items.AddItem("LONGSHOT_GEODE", [ItemTag.Damage], "texlongshotgeodeicon", "longshotgeodemesh", ItemTier.VoidTier1, _corruptToken: "ITEM_NEARBYDAMAGEBONUS_NAME", _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add On Incoming Damage behaviour
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
            displaySettings.AddCharacterDisplay("Acrid", "HandR", new Vector3(0F, 0F, 0F), new Vector3(45F, 0F, 0F), new Vector3(0.8F, 0.8F, 0.8F));
            displaySettings.AddCharacterDisplay("Captain", "HandL", new Vector3(0F, 0.125F, -0.064F), new Vector3(0F, 95F, 0F), new Vector3(0.08F, 0.08F, 0.08F));
            displaySettings.AddCharacterDisplay("Railgunner", "TopRail", new Vector3(0F, 0.6525F, 0.0585F), new Vector3(0F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "ElbowR", new Vector3(0.01F, -0.0125F, 0.005F), new Vector3(345F, 90F, 60F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Scavenger", "Weapon", new Vector3(0F, 18.25F, 0F), new Vector3(280F, 330F, 90F), new Vector3(2.5F, 2.5F, 2.5F));
            displaySettings.AddCharacterDisplay("Seeker", "HandL", new Vector3(0.0023F, -0.1F, 0.025F), new Vector3(7.5F, 2.15F, 141.5F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "HandL", new Vector3(0.01213F, 0.1175F, -0.03225F), new Vector3(0F, 180F, 180F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Chef", "HandL", new Vector3(0.00315F, 0.01855F, -0.0625F), new Vector3(0F, 180F, 180F), new Vector3(0.05F, 0.05F, 0.05F));
        }

        private void CreateSettings()
        {
            // Create settings specific to this item
            damageSetting = longshotGeodeItem.CreateSetting("DAMAGE", "Damage", 15.0f, "How much should this item increase damage while the target is beyond the distance threshold? (15.0 = 15% increase)");
            damageStackingSetting = longshotGeodeItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 15.0f, "How much should further stacks of this item increase damage while the target is beyond the distance threshold? (15.0 = 15% increase)");
            distanceSetting = longshotGeodeItem.CreateSetting("DISTANCE", "Distance", 50.0f, "How far should the target need to be for the damage bonus to be applied? (50.0 = 50 meters)");

            // Update item texts with new settings
            longshotGeodeItem.UpdateItemTexts();
        }

        private void FetchSettings()
        {
            // Get item settings
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            distanceThreshold = distanceSetting.Value;
        }

        void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check for attacker and victim
            if (_attacker == null || _victim == null)
            {
                return;
            }

            // Check for attacker and victim bodies
            if (!_attacker.hasBody || !_victim.hasBody)
            {
                return;
            }

            // Get attacker and victim bodies
            CharacterBody attackerBody = _attacker.GetBody();
            CharacterBody victimBody = _victim.GetBody();

            // Check for attacker inventory
            if (!attackerBody.inventory)
            {
                return;
            }

            // Get item count
            int count = attackerBody.inventory.GetItemCount(longshotGeodeItem.itemDef);

            // Has item?
            if (count > 0)
            {
                // Get distance between bodies
                float distance = (attackerBody.transform.position - victimBody.transform.position).magnitude;

                // Check if distance is greater or equal to 50 metres
                if (distance >= distanceThreshold)
                {
                    // Apply damage bonus and colour index
                    _report.damageColorIndex = DamageColorIndex.Nearby;
                    _report.damage *= 1.0f + damage + (damageStacking * (count - 1));
                }
            }
        }
    }
}
