using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class CauterizingGreave : ItemBase
    {
        // Store item
        Item cauterizingGreaveItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> maxHealthBuffSetting;
        Setting<float> maxHealthBuffStackingSetting;
        Setting<float> healingNerfSetting;
        Setting<float> healingNerfStackingSetting;

        // Store item stats
        float maxHealthBuff;
        float maxHealthBuffStacking;
        float healingNerf;
        float healingNerfStacking;

        // Constructor
        public CauterizingGreave(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("cauterizinggreavedisplaymesh");

            // Create Longshot Geode item
            cauterizingGreaveItem = Items.AddItem("CAUTERIZING_GREAVE", [ItemTag.Utility], "texcauterizinggreaveicon", "cauterizinggreavemesh", ItemTier.Lunar, _displaySettings: displaySettings, _modifyItemModelPrefabCallback: ModifyModelPrefab, _modifyItemDisplayPrefabCallback: ModifyModelPrefab);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

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
            displaySettings.AddCharacterDisplay("Seeker", "CalfL", new Vector3(-0.00875F, 0.3325F, -0.01375F), new Vector3(348.5723F, 270.4153F, 357.9048F), new Vector3(0.175F, 0.15F, 0.175F));
            displaySettings.AddCharacterDisplay("False Son", "CalfL", new Vector3(-0.0315F, 0.31835F, -0.0125F), new Vector3(12.5F, 90F, 0F), new Vector3(0.275F, 0.2F, 0.275F));
            displaySettings.AddCharacterDisplay("Chef", "Base", new Vector3(0.1925F, 0F, 0.54F), new Vector3(0F, 0F, 90F), new Vector3(0.1F, 0.075F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "Chest", new Vector3(0.1925F, 0F, 0.54F), new Vector3(0F, 0F, 90F), new Vector3(0.1F, 0.075F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            maxHealthBuffSetting = cauterizingGreaveItem.CreateSetting("MAX_HEALTH_BUFF", "Max Health Increase", 100.0f, "How much should this item increase the max health of the player? (100.0 = 100% increase)", _minValue: 0.1f, _valueFormatting: "{0:0}%");
            maxHealthBuffStackingSetting = cauterizingGreaveItem.CreateSetting("MAX_HEALTH_BUFF_STACKING", "Max Health Increase Stacking", 100.0f, "How much should further stacks of this item increase the max health of the player? (100.0 = 100% increase)", _minValue: 0.1f, _valueFormatting: "{0:0}%");
            healingNerfSetting = cauterizingGreaveItem.CreateSetting("HEALING_NERF", "Healing Decrease", 50.0f, "How much should this item decrease the received healing of the player? (50.0 = 50% decrease)", _randomiserMin: 0.0f, _randomiserMax: 90.0f, _minValue: 0.1f, _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");
            healingNerfStackingSetting = cauterizingGreaveItem.CreateSetting("HEALING_NERF_STACKING", "Healing Decrease Stacking", 50.0f, "How much should further stacks of this item decrease the received healing of the player? (50.0 = 50% decrease)", _randomiserMin: 0.0f, _randomiserMax: 90.0f, _minValue: 0.1f, _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            maxHealthBuff = maxHealthBuffSetting.Value / 100.0f;
            maxHealthBuffStacking = maxHealthBuffStackingSetting.Value / 100.0f;
            healingNerf = 1.0f - healingNerfSetting.Value / 100.0f;
            healingNerfStacking = 1.0f - healingNerfStackingSetting.Value / 100.0f;

            // Update item texts with new settings
            cauterizingGreaveItem.UpdateItemTexts();
        }

        void ModifyModelPrefab(GameObject _prefab)
        {
            // Get ring
            GameObject ring = Utils.FindChildByName(_prefab.transform, "Ring");

            // Add rotator
            FaithfulRotatorBehaviour ringRotator = ring.AddComponent<FaithfulRotatorBehaviour>();

            // Initialise rotator
            ringRotator.Init(new Vector3(0.0f, 1.0f, 0.0f), 45.0f);
        }

        void StatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for item
            if (_count == 0) return;

            // Increase max health
            _stats.healthMultAdd += maxHealthBuff + (maxHealthBuffStacking * (_count - 1));
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
                    // Decrease received healing
                    _amount *= healingNerf * Mathf.Pow(healingNerfStacking, count - 1);
                }
            }
        }
    }
}
