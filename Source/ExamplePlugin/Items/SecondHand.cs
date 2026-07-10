using EntityStates;
using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class SecondHand : ItemBase
    {
        // Store buffs
        Buff secondHandBuff;
        Buff secondHandEffectBuff;

        // Quality buffs
        Buff secondHandBoostBuff;
        Buff secondHandBoostUncommonBuff;
        Buff secondHandBoostRareBuff;
        Buff secondHandBoostEpicBuff;
        Buff secondHandBoostLegendaryBuff;

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

        // Store additional quality settings
        QualitySetting<float> boostAttackSpeedQualitySetting;
        QualitySetting<float> boostAttackSpeedStackingQualitySetting;
        QualitySetting<float> boostDelayQualitySetting;
        QualitySetting<float> graceDurationQualitySetting;

        // Store quality item stats
        QualityValues<float> boostAttackSpeedQualityValues = new();
        QualityValues<float> boostAttackSpeedStackingQualityValues = new();
        QualityValues<float> graceDurationQualityValues = new();
        float boostDelay = 3.0f;

        // Store quality buff state
        Dictionary<CharacterBody, SecondHandQualityState> qualityStates = [];

        // Constructor
        public SecondHand(Toolbox _toolbox) : base(_toolbox, "SECOND_HAND")
        {
            // Create display settings
            CreateDisplaySettings("secondhanddisplaymesh");

            // Create Second Hand item and buff
            MainItem = Items.AddItem(token, "Second Hand", [ItemTag.Damage, ItemTag.Utility, ItemTag.Technology, ItemTag.MobilityRelated], "texsecondhandicon", "secondhandmesh", _tier: ItemTier.Tier2, _supportsQuality: true, _displaySettings: displaySettings);
            secondHandBuff = Buffs.AddBuff("SECOND_HAND", "Second Hand", "texbuffsecondhand", Color.white, false);
            secondHandEffectBuff = Buffs.AddBuff("SECOND_HAND_EFFECT", "Second Hand", "texbuffsecondhand", Color.white, _isHidden: true, _hasConfig: false, _langTokenOverride: "SECOND_HAND");

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(secondHandEffectBuff, SecondHandStatsMod);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            secondHandBoostBuff = Buffs.AddBuff("SECOND_HAND_BOOST", "Second Hand Boost", "texBuffSecondHandBoost", Color.white, false, _qualityBuff: true);
            secondHandBoostUncommonBuff = Buffs.AddBuff("SECOND_HAND_BOOST_UNCOMMON", "Second Hand Boost", "texBuffSecondHandBoost", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "SECOND_HAND_BOOST");
            secondHandBoostRareBuff = Buffs.AddBuff("SECOND_HAND_BOOST_RARE", "Second Hand Boost", "texBuffSecondHandBoost", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "SECOND_HAND_BOOST");
            secondHandBoostEpicBuff = Buffs.AddBuff("SECOND_HAND_BOOST_EPIC", "Second Hand Boost", "texBuffSecondHandBoost", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "SECOND_HAND_BOOST");
            secondHandBoostLegendaryBuff = Buffs.AddBuff("SECOND_HAND_BOOST_LEGENDARY", "Second Hand Boost", "texBuffSecondHandBoost", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "SECOND_HAND_BOOST");

            // Add stats mods for quality boost buffs
            Behaviour.AddStatsMod(secondHandBoostUncommonBuff, UncommonStatsMod_Quality);
            Behaviour.AddStatsMod(secondHandBoostRareBuff, RareStatsMod_Quality);
            Behaviour.AddStatsMod(secondHandBoostEpicBuff, EpicStatsMod_Quality);
            Behaviour.AddStatsMod(secondHandBoostLegendaryBuff, LegendaryStatsMod_Quality);

            // Clear stored quality state on scene exit
            Behaviour.AddOnPreSceneExitCallback((_exitController) =>
            {
                qualityStates.Clear();
            });
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
            displaySettings.AddCharacterDisplay("Walker Turret", "LegBar1", new Vector3(0.2675F, 1.05F, -0.07F), new Vector3(0F, 270F, 180F), new Vector3(0.5F, 0.5F, 0.5F));
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
            displaySettings.AddCharacterDisplay("Technician", "Head", new Vector3(0F, 0.275F, 0.10875F), new Vector3(327.5F, 0F, 180F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(-0.00125F, 0.27125F, -0.225F), new Vector3(0F, 0F, 90F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Drifter", "LowerArmL", new Vector3(0.2405F, 0.0375F, -0.02375F), new Vector3(57.5F, 318F, 52.5F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Best Buddy", "CableM", new Vector3(0.005F, -0.12F, 0.0025F), new Vector3(35.5F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            attackSpeedSetting = MainItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 20.0f, "How much should this item increase attack speed while touching the ground? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            attackSpeedStackingSetting = MainItem.CreateSetting("ATTACK_SPEED_STACKING", "Attack Speed Stacking", 20.0f, "How much should further stacks of this item increase attack speed while touching the ground? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            speedSetting = MainItem.CreateSetting("SPEED", "Movement Speed", 30.0f, "How much should this item increase movement speed while touching the ground? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
            speedStackingSetting = MainItem.CreateSetting("SPEED_STACKING", "Movement Speed Stacking", 30.0f, "How much should further stacks of this item increase movement speed while touching the ground? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            boostAttackSpeedQualitySetting = MainItem.CreateQualitySetting("BOOST_ATTACK_SPEED", "Boost Attack Speed", 20.0f, 40.0f, 60.0f, 80.0f, "How much attack speed should this item gain after being grounded long enough? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            boostAttackSpeedStackingQualitySetting = MainItem.CreateQualitySetting("BOOST_ATTACK_SPEED_STACKING", "Boost Attack Speed Stacking", 20.0f, 40.0f, 60.0f, 80.0f, "How much additional attack speed should further stacks of this quality item gain after being grounded long enough? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            boostDelayQualitySetting = MainItem.CreateQualitySetting("BOOST_DELAY", "Boost Delay", 3.0f, "How long should the player need to be grounded before gaining the boosted Second Hand buff? (3.0 = 3 seconds)", _isStat: false, _minValue: 0.0f, _canRandomise: false, _valueFormatting: "{0:0.0}s");
            graceDurationQualitySetting = MainItem.CreateQualitySetting("GRACE_DURATION", "Grace Duration", 2.0f, 4.0f, 6.0f, 8.0f, "How long should this quality item keep its grounded buffs after leaving the ground? (2.0 = 2 seconds)", _isStat: false, _minValue: 0.0f, _valueFormatting: "{0:0.0}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            attackSpeedStacking = attackSpeedStackingSetting.Value / 100.0f;
            speed = speedSetting.Value / 100.0f;
            speedStacking = speedStackingSetting.Value / 100.0f;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            boostAttackSpeedQualityValues.UpdateValues(boostAttackSpeedQualitySetting, 0.01f);
            boostAttackSpeedStackingQualityValues.UpdateValues(boostAttackSpeedStackingQualitySetting, 0.01f);
            graceDurationQualityValues.UpdateValues(graceDurationQualitySetting);
            boostDelay = boostDelayQualitySetting.Value;
        }

        void SecondHandStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, attackSpeed, attackSpeedStacking);
            _stats.moveSpeedMultAdd += Utils.CalculateStackingValue(_count, speed, speedStacking);
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            Inventory inventory = characterBody?.inventory;
            if (characterBody && inventory)
            {
                // Get Second Hand item amount
                int secondHandCount = inventory.GetItemCountEffective(MainItem.itemDef);

                // Check if quality behaviour is available
                bool qualityAvailable = MainItem.supportsQuality && Utils.qualityEnabled && secondHandBoostBuff != null;

                // Get quality counts
                QualityCounts qualityCounts = new();
                if (qualityAvailable)
                {
                    qualityCounts = QualityCompat.GetItemCountsEffective(inventory, MainItem);
                }

                // Check for item
                if (secondHandCount <= 0 && qualityCounts.Total <= 0)
                {
                    // Clear Second Hand buffs
                    SetSecondHandBuffs(characterBody, 0, false);

                    // Clear quality buffs and state
                    ClearQualityBuffs(characterBody);
                    qualityStates.Remove(characterBody);

                    // Done
                    return;
                }

                // Check if grounded or character motor is null
                bool grounded = _character.isGrounded || _character.characterMotor == null;

                // Update quality state
                bool baseActive = grounded;
                bool boostActive = false;
                if (qualityAvailable && qualityCounts.Total > 0)
                {
                    SecondHandQualityState state = UpdateQualityState(characterBody, qualityCounts, grounded);
                    baseActive = grounded || state.graceTimer > 0.0f;
                    boostActive = baseActive && state.groundedTimer >= boostDelay;
                }
                else
                {
                    // Clear quality buffs and state
                    ClearQualityBuffs(characterBody);
                    qualityStates.Remove(characterBody);
                }

                // Get target Second Hand buff amount
                int targetSecondHandCount = baseActive ? secondHandCount : 0;

                // Get current amount of Second Hand buffs
                int currentSecondHandCount = characterBody.GetBuffCount(secondHandEffectBuff.buffDef);

                // Check if character has the wrong amount of buffs
                if (targetSecondHandCount != currentSecondHandCount)
                {
                    // Update Second Hand buff count
                    SetSecondHandBuffs(characterBody, targetSecondHandCount, boostActive);
                }
                else
                {
                    // Update visual buff
                    characterBody.SetBuffCount(secondHandBuff.buffDef.buffIndex, targetSecondHandCount > 0 && !boostActive ? 1 : 0);
                    if (qualityAvailable) characterBody.SetBuffCount(secondHandBoostBuff.buffDef.buffIndex, targetSecondHandCount > 0 && boostActive ? 1 : 0);
                }

                // Update quality boost buff counts
                if (qualityAvailable) SetQualityBuffs(characterBody, qualityCounts, boostActive);
            }
        }

        private SecondHandQualityState UpdateQualityState(CharacterBody _body, QualityCounts _counts, bool _grounded)
        {
            // Try get quality state
            if (!qualityStates.TryGetValue(_body, out SecondHandQualityState state))
            {
                // Create quality state
                state = new SecondHandQualityState();
                qualityStates[_body] = state;
            }

            // Get grace duration
            float graceDuration = graceDurationQualityValues.GetValue(_counts.GetHighestQuality());

            // Update grounded timer and grace timer
            if (_grounded)
            {
                state.groundedTimer += Time.fixedDeltaTime;
                state.graceTimer = graceDuration;
            }
            else
            {
                state.graceTimer = Mathf.Max(0.0f, state.graceTimer - Time.fixedDeltaTime);

                // Keep the boost charged during the grace period, otherwise reset the grounded timer
                if (state.groundedTimer >= boostDelay && state.graceTimer > 0.0f) state.groundedTimer = boostDelay;
                else state.groundedTimer = 0.0f;
            }

            // Return quality state
            return state;
        }

        private void SetSecondHandBuffs(CharacterBody _body, int _count, bool _boostActive)
        {
            // Update Second Hand buff count
            _body.SetBuffCount(secondHandEffectBuff.buffDef.buffIndex, _count);

            // Update visual buff
            _body.SetBuffCount(secondHandBuff.buffDef.buffIndex, _count > 0 && !_boostActive ? 1 : 0);
            if (secondHandBoostBuff != null) _body.SetBuffCount(secondHandBoostBuff.buffDef.buffIndex, _count > 0 && _boostActive ? 1 : 0);
        }

        private void SetQualityBuffs(CharacterBody _body, QualityCounts _counts, bool _boostActive)
        {
            // Get boost buff counts
            int uncommonCount = _boostActive ? _counts.UNCOMMON : 0;
            int rareCount = _boostActive ? _counts.RARE : 0;
            int epicCount = _boostActive ? _counts.EPIC : 0;
            int legendaryCount = _boostActive ? _counts.LEGENDARY : 0;

            // Update boost buff counts
            _body.SetBuffCount(secondHandBoostUncommonBuff.buffDef.buffIndex, uncommonCount);
            _body.SetBuffCount(secondHandBoostRareBuff.buffDef.buffIndex, rareCount);
            _body.SetBuffCount(secondHandBoostEpicBuff.buffDef.buffIndex, epicCount);
            _body.SetBuffCount(secondHandBoostLegendaryBuff.buffDef.buffIndex, legendaryCount);
        }

        private void ClearQualityBuffs(CharacterBody _body)
        {
            // Validate input
            if (_body == null || secondHandBoostBuff == null) return;

            // Clear boost buff counts
            _body.SetBuffCount(secondHandBoostBuff.buffDef.buffIndex, 0);
            _body.SetBuffCount(secondHandBoostUncommonBuff.buffDef.buffIndex, 0);
            _body.SetBuffCount(secondHandBoostRareBuff.buffDef.buffIndex, 0);
            _body.SetBuffCount(secondHandBoostEpicBuff.buffDef.buffIndex, 0);
            _body.SetBuffCount(secondHandBoostLegendaryBuff.buffDef.buffIndex, 0);
        }

        private void UncommonStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, boostAttackSpeedQualityValues.UNCOMMON, boostAttackSpeedStackingQualityValues.UNCOMMON);
        }

        private void RareStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, boostAttackSpeedQualityValues.RARE, boostAttackSpeedStackingQualityValues.RARE);
        }

        private void EpicStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, boostAttackSpeedQualityValues.EPIC, boostAttackSpeedStackingQualityValues.EPIC);
        }

        private void LegendaryStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, boostAttackSpeedQualityValues.LEGENDARY, boostAttackSpeedStackingQualityValues.LEGENDARY);
        }

        private class SecondHandQualityState
        {
            public float groundedTimer;
            public float graceTimer;
        }
    }
}
