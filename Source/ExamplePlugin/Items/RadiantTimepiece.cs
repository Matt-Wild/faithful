using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class RadiantTimepiece : ItemBase
    {
        // Store item
        Item radiantTimepieceItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> durationSetting;
        Setting<float> durationStackingSetting;

        // Store item stats
        float duration;
        float durationStacking;

        // Constructor
        public RadiantTimepiece(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("RadiantTimepieceDisplayMesh");

            // Create Second Hand item and buff
            radiantTimepieceItem = Items.AddItem("RADIANT_TIMEPIECE", "Radiant Timepiece", [ItemTag.Utility, ItemTag.Technology, ItemTag.AIBlacklist], "texRadiantTimepieceIcon", "RadiantTimepieceMesh", _tier: ItemTier.Tier1, _displaySettings: displaySettings, _modifyItemDisplayPrefabCallback: ModifyDisplayPrefab);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Hook behaviour
            Behaviour.AddOnAddTimedBuffCallback(OnAddTimedBuff);
            Behaviour.AddOnAddTimedBuffMaxStacksCallback(OnAddTimedBuffMaxStacks);
            Behaviour.AddOnExtendTimedBuffsCallback(OnExtendTimedBuffs);
            Behaviour.AddOnSetTimedBuffDurationCallback(OnSetTimedBuffDuration);
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
            displaySettings.AddCharacterDisplay("Commando", "Stomach", new Vector3(-0.08F, 0F, -0.12975F), new Vector3(352.5F, 185F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0.1685F, 0.0205F, 0.0575F), new Vector3(0F, 90F, 348.75F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Bandit", "Stomach", new Vector3(0.035F, -0.0275F, -0.17F), new Vector3(340F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(1.53F, 0.45F, -1.825F), new Vector3(0F, 180F, 0F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(-0.0745F, -0.105F, -0.3336F), new Vector3(345F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.42F, 0.15F, -1.4375F), new Vector3(0F, 215F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(-0.63F, 0.92F, -1.595F), new Vector3(0F, 180F, 0F), new Vector3(0.4F, 0.4F, 0.4F));
            displaySettings.AddCharacterDisplay("Artificer", "Pelvis", new Vector3(0.195F, -0.0075F, -0.0125F), new Vector3(17.25F, 75.5F, 195F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Mercenary", "Pelvis", new Vector3(0.1125F, 0.104F, 0.079F), new Vector3(7.5F, 0F, 180F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(-0.375F, 0.025F, -0.5475F), new Vector3(0F, 195F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0.1705F, 0.315F, -0.175F), new Vector3(0F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "SpineChest3", new Vector3(1.5425F, 1.475F, -0.05F), new Vector3(335F, 90F, 270F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "Stomach", new Vector3(0.185F, 0.071F, 0.12F), new Vector3(353.75F, 30F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Railgunner", "Pelvis", new Vector3(0.07F, 0.12F, 0.135F), new Vector3(22.5F, 20F, 182.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(0.15125F, -0.07375F, -0.15F), new Vector3(17.5F, 159.25F, 339.25F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Seeker", "Pelvis", new Vector3(-0.18F, -0.0175F, -0.1425F), new Vector3(335F, 216.25F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Pelvis", new Vector3(-0.15875F, 0.024F, -0.2425F), new Vector3(355F, 185F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.1375F, -0.245F, -0.22F), new Vector3(90F, 270F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Operator", "Stomach", new Vector3(-0.017F, 0.0925F, -0.0705F), new Vector3(292F, 212F, 237.5F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(0.25675F, -0.09575F, -0.3425F), new Vector3(60F, 207.5F, 257.5F), new Vector3(0.12F, 0.12F, 0.12F));
            displaySettings.AddCharacterDisplay("Technician", "Pelvis", new Vector3(-0.11625F, 0.0475F, -0.194F), new Vector3(355F, 180F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            durationSetting = radiantTimepieceItem.CreateSetting("DURATION", "Duration", 1.0f, "How much should this item increase the duration of temporary buffs? (1.0 = 1 second)", _valueFormatting: "{0:0.0}%");
            durationStackingSetting = radiantTimepieceItem.CreateSetting("DURATION_STACKING", "Duration Stacking", 1.0f, "How much should further stacks of this item increase the duration of temporary buffs? (1.0 = 1 second)", _valueFormatting: "{0:0.0}%");
        }

        public override void FetchSettings()
        {
            // Get item settings
            duration = durationSetting.Value;
            durationStacking = durationStackingSetting.Value;

            // Update item texts with new settings
            radiantTimepieceItem.UpdateItemTexts();
        }

        void ModifyDisplayPrefab(GameObject _prefab)
        {
            // Get first timepiece object
            GameObject chain = Utils.FindChildByName(_prefab.transform, "Chain.001");

            // Add dynamic bone behaviour
            DynamicBone dynamicBone = chain.AddComponent<DynamicBone>();

            // Set up dynamic bone config
            DynamicBoneConfig dynamicBoneConfig = new DynamicBoneConfig();
            dynamicBoneConfig.Damping = 0.375f;
            dynamicBoneConfig.Elasticity = 0.02f;
            dynamicBoneConfig.Stiffness = 0.5f;
            dynamicBoneConfig.Gravity = new Vector3(0.0f, -1.0f, 0.0f);
            dynamicBoneConfig.LocalGravity = new Vector3(0.0f, -0.1f, -0.5f);
            dynamicBoneConfig.Exclusions = new List<Transform>
            {
                Utils.FindChildByName(_prefab.transform, "BigHand").transform,
                Utils.FindChildByName(_prefab.transform, "SmallHand").transform
            };

            // Set up dynamic bone
            Utils.ConfigureDynamicBone(dynamicBone, dynamicBoneConfig);
        }

        private void OnAddTimedBuff(BuffDef _buff, ref float _duration, CharacterBody _character)
        {
            // Add additional duration to temporary buffs if we have the item
            _duration += GetAdditionalTimedBuffDuration(_buff, _character);
        }

        private void OnAddTimedBuffMaxStacks(BuffDef _buff, ref float _duration, ref int _maxStacks, CharacterBody _character)
        {
            // Add additional duration to temporary buffs if we have the item
            _duration += GetAdditionalTimedBuffDuration(_buff, _character);
        }

        private void OnExtendTimedBuffs(BuffDef _buff, ref float _duration, ref float _max, CharacterBody _character)
        {
            // Get additional duration to temporary buffs if we have the item
            float additionalDuration = GetAdditionalTimedBuffDuration(_buff, _character);

            // Apply to extension
            _duration += additionalDuration;
            _max += additionalDuration;
        }

        private void OnSetTimedBuffDuration(BuffDef _buff, ref float _duration, bool _allStacks, CharacterBody _character)
        {
            // Add additional duration to temporary buffs if we have the item
            _duration += GetAdditionalTimedBuffDuration(_buff, _character);
        }

        public float GetAdditionalTimedBuffDuration(BuffDef _buff, CharacterBody _character)
        {
            // Check for valid buff and character body
            if (_buff == null || _character == null) return 0.0f;

            // Check for valid inventory
            Inventory inventory = _character.inventory;
            if (inventory == null) return 0.0f;

            // Get item count
            int itemCount = inventory.GetItemCountEffective(radiantTimepieceItem.itemDef);

            // Apply effect to duration if we have at least 1 item
            if (itemCount == 0) return 0.0f;
            return duration + durationStacking * (itemCount - 1);
        }
    }
}
