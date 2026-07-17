using R2API;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class LeadersPennon : ItemBase
    {
        // Store buffs
        Buff leadersPennonBuff;
        Buff leadersPennonVisualBuff;

        // Quality buffs
        Buff leadersPennonSpeedBuff;
        Buff leadersPennonSpeedUncommonBuff;
        Buff leadersPennonSpeedRareBuff;
        Buff leadersPennonSpeedEpicBuff;
        Buff leadersPennonSpeedLegendaryBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> enableRadiusIndicatorSetting;
        Setting<bool> enableBuffEffectSetting;
        Setting<float> radiusSetting;
        Setting<float> radiusStackingSetting;
        Setting<float> attackSpeedSetting;
        Setting<float> attackSpeedStackingSetting;
        Setting<float> regenSetting;
        Setting<float> regenStackingSetting;
        Setting<float> regenPerLevelSetting;
        Setting<float> regenMultSetting;
        Setting<float> buffDurationSetting;

        // Store item stats
        bool enableRadiusIndicator;
        bool enableBuffEffect;
        float baseRadius;
        float radiusStacking;
        float attackSpeed;
        float attackSpeedStacking;
        float regen;
        float regenStacking;
        float regenPerLevel;
        float regenMult;
        float buffDuration;

        // Store additional quality settings
        QualitySetting<float> speedQualitySetting;
        QualitySetting<float> durationQualitySetting;
        QualitySetting<float> durationStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> speedQualityValues = new();
        QualityValues<float> durationQualityValues = new();
        QualityValues<float> durationStackingQualityValues = new();

        // Constructor
        public LeadersPennon(Toolbox _toolbox) : base(_toolbox, "LEADERS_PENNON")
        {
            // Create display settings
            CreateDisplaySettings("leaderspennondisplaymesh");

            // Create Leader's Pennon item and buff
            leadersPennonBuff = Buffs.AddBuff("LEADERS_PENNON", "Leaders Pennon", "texbuffleaderarea", Color.white, true, _isHidden: true, _hasConfig: false);
            leadersPennonVisualBuff = Buffs.AddBuff("LEADERS_PENNON_VISUAL", "Leaders Pennon", "texbuffleaderarea", Color.white, false, _langTokenOverride: "LEADERS_PENNON");
            MainItem = Items.AddItem(token, "Leaders Pennon", [ItemTag.Utility, ItemTag.AIBlacklist], "texleaderspennonicon", "leaderspennonmesh", ItemTier.VoidTier1, _corruptToken: "ITEM_WARDONLEVEL_NAME", _supportsQuality: true, _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add ally to ally behaviour
            Behaviour.AddAllyToAllyCallback(MainItem, AllyWithItemToAlly);

            // Add update visual effects callback
            Behaviour.AddOnUpdateVisualEffectsCallback(UpdateVisualEffects);

            // Add stats modification
            Behaviour.AddStatsMod(leadersPennonBuff, LeadersPennonStatsMod);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            leadersPennonSpeedBuff = Buffs.AddBuff("LEADERS_PENNON_SPEED", "Leaders Pennon Speed", "texBuffLeaderSpeed", Color.white, false, _qualityBuff: true);
            leadersPennonSpeedUncommonBuff = Buffs.AddBuff("LEADERS_PENNON_SPEED_UNCOMMON", "Leaders Pennon Speed", "texBuffLeaderSpeed", Color.white, false, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "LEADERS_PENNON_SPEED");
            leadersPennonSpeedRareBuff = Buffs.AddBuff("LEADERS_PENNON_SPEED_RARE", "Leaders Pennon Speed", "texBuffLeaderSpeed", Color.white, false, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "LEADERS_PENNON_SPEED");
            leadersPennonSpeedEpicBuff = Buffs.AddBuff("LEADERS_PENNON_SPEED_EPIC", "Leaders Pennon Speed", "texBuffLeaderSpeed", Color.white, false, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "LEADERS_PENNON_SPEED");
            leadersPennonSpeedLegendaryBuff = Buffs.AddBuff("LEADERS_PENNON_SPEED_LEGENDARY", "Leaders Pennon Speed", "texBuffLeaderSpeed", Color.white, false, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "LEADERS_PENNON_SPEED");

            // Link On Purchase Interaction Begin behaviour
            Behaviour.AddOnPurchaseInteractionBeginCallback(OnPurchaseInteractionBegin_Quality);

            // Link character tick behaviour for holder self-buffing
            Behaviour.AddOnCharacterBodyTickCallback(3.0f, OnCharacterBodyTick_Quality);

            // Add stats mods for speed buffs
            Behaviour.AddStatsMod(leadersPennonSpeedUncommonBuff, UncommonStatsMod_Quality);
            Behaviour.AddStatsMod(leadersPennonSpeedRareBuff, RareStatsMod_Quality);
            Behaviour.AddStatsMod(leadersPennonSpeedEpicBuff, EpicStatsMod_Quality);
            Behaviour.AddStatsMod(leadersPennonSpeedLegendaryBuff, LegendaryStatsMod_Quality);
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
            displaySettings.AddCharacterDisplay("Commando", "Chest", new Vector3(0F, 0.3775F, -0.21275F), new Vector3(1F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Huntress", "Chest", new Vector3(0F, 0.125F, -0.1275F), new Vector3(5F, 180F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Bandit", "MainWeapon", new Vector3(-0.06125F, 0.5415F, -0.0255F), new Vector3(1F, 172.75F, 0F), new Vector3(0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("MUL-T", "Chest", new Vector3(0F, 4F, -1.725F), new Vector3(0F, 180F, 0F), new Vector3(1F, 0.8F, 1F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0.003F, 0.47825F, -0.2905F), new Vector3(0F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.325F, -0.20525F), new Vector3(0F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Neck", new Vector3(0F, 0.325F, -0.20525F), new Vector3(0F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Artificer", "Chest", new Vector3(0.1165F, 0.448F, -0.275F), new Vector3(0F, 180F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("Mercenary", "UpperArmR", new Vector3(-0.16175F, 0.005F, -0.03925F), new Vector3(3.50988F, 257.5009F, 173.5443F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("REX", "PlatformBase", new Vector3(0F, 0.4F, -0.3725F), new Vector3(0F, 180F, 0F), new Vector3(0.12F, 0.12F, 0.12F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(0.00098F, 0.06F, -0.1515F), new Vector3(359F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(0F, 0.94F, 1.51255F), new Vector3(348.9F, 0F, 180F), new Vector3(1.35F, 1.35F, 1.35F));
            displaySettings.AddCharacterDisplay("Captain", "Chest", new Vector3(0F, 0.32F, -0.235F), new Vector3(3.5F, 180F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Railgunner", "Backpack", new Vector3(0.055F, -0.035F, -0.11F), new Vector3(0F, 178F, 14F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Chest", new Vector3(-0.005F, 0.345F, -0.205F), new Vector3(5F, 180F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Seeker", "Pack", new Vector3(-0.153F, 0.05F, -0.39275F), new Vector3(0F, 180F, 315F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("False Son", "Chest", new Vector3(-0.2405F, 0.4255F, 0.19575F), new Vector3(329.5F, 0.5F, 55.75F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "Chest", new Vector3(-0.3335F, -0.2505F, 0.0195F), new Vector3(90F, 270F, 0F), new Vector3(0.11F, 0.1F, 0.11F));
            displaySettings.AddCharacterDisplay("Technician", "Backpack", new Vector3(0F, 0.265F, 0.0875F), new Vector3(355F, 180F, 0F), new Vector3(0.125F, 0.1125F, 0.125F));
            displaySettings.AddCharacterDisplay("Operator", "Backpack", new Vector3(0F, 0.4475F, -0.2165F), new Vector3(7.5F, 180F, 0F), new Vector3(0.15F, 0.125F, 0.15F));
            displaySettings.AddCharacterDisplay("Drifter", "Chest", new Vector3(-0.1625F, -0.2875F, 0F), new Vector3(75F, 90F, 180F), new Vector3(0.2F, 0.2F, 0.2F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            enableRadiusIndicatorSetting = MainItem.CreateSetting("ENABLE_RADIUS_INDICATOR", "Enable Radius Indicator?", true, "Should this item have a radius indicator visual effect?", false, true, _canRandomise: false);
            enableBuffEffectSetting = MainItem.CreateSetting("ENABLE_BUFF_EFFECT", "Enable Buff Visual Effect?", true, "Should this item's buff have a visual effect?", false, true, _canRandomise: false);
            radiusSetting = MainItem.CreateSetting("RADIUS", "Radius", 15.0f, "How big should the base radius be of this item's effect? (15.0 = 15 meters)", _valueFormatting: "{0:0.0}m");
            radiusStackingSetting = MainItem.CreateSetting("RADIUS_STACKING", "Radius Stacking", 7.5f, "How much should the radius of this item's effect increase per stack? (7.5 = 7.5 meters)", _valueFormatting: "{0:0.00}m");
            attackSpeedSetting = MainItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 30.0f, "How much should this item increase ally's attack speed? (30.0 = 30% increase)", _valueFormatting: "{0:0.0}%");
            attackSpeedStackingSetting = MainItem.CreateSetting("ATTACK_SPEED_STACKING", "Attack Speed Stacking", 0.0f, "How much should additional stacks of this item increase ally's attack speed? (10.0 = 10% increase)", _valueFormatting: "{0:0.0}%");
            regenSetting = MainItem.CreateSetting("REGEN", "Regen", 5.0f, "How much should this item increase ally's base regen? (5.0 = 5 hp/s)", _valueFormatting: "{0:0.00}hp/s");
            regenStackingSetting = MainItem.CreateSetting("REGEN_STACKING", "Regen Stacking", 0.0f, "How much should additional stacks of this item increase ally's base regen? (10.0 = 10 hp/s)", _valueFormatting: "{0:0.00}hp/s");
            regenPerLevelSetting = MainItem.CreateSetting("REGEN_PER_LEVEL", "Regen Per Level", 1.0f, "How much should this item increase ally's regen per level? (1.0 = 1 hp/s)", _valueFormatting: "{0:0.00}hp/s");
            regenMultSetting = MainItem.CreateSetting("REGEN_MULT", "Regen Multiplier", 30.0f, "How much should this item increase ally's regen multiplicatively? (30.0 = 30% increase)", _canRandomise: false, _valueFormatting: "{0:0.0}%");
            buffDurationSetting = MainItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the radius of this item's effect? (1.0 = 1 second)", _minValue: 0.1f, _canRandomise: false, _valueFormatting: "{0:0.00}s");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            speedQualitySetting = MainItem.CreateQualitySetting("SPEED", "Movement Speed", 20.0f, 40.0f, 70.0f, 100.0f, "How much movement speed should this quality item grant nearby allies after activating an interactable? (20.0 = 20% movement speed)", _isStat: false, _minValue: 0.0f, _valueFormatting: "{0:0.0}%");
            durationQualitySetting = MainItem.CreateQualitySetting("DURATION", "Buff Duration", 5.0f, 10.0f, 15.0f, 20.0f, "How long should this quality item increase nearby ally movement speed after activating an interactable? (5.0 = 5 seconds)", _minValue: 0.0f, _valueFormatting: "{0:0.0}s");
            durationStackingQualitySetting = MainItem.CreateQualitySetting("DURATION_STACKING", "Buff Duration Stacking", 5.0f, 10.0f, 15.0f, 20.0f, "How much longer should further stacks of this quality item increase nearby ally movement speed after activating an interactable? (5.0 = 5 seconds)", _minValue: 0.0f, _valueFormatting: "{0:0.0}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            enableRadiusIndicator = enableRadiusIndicatorSetting.Value;
            enableBuffEffect = enableBuffEffectSetting.Value;
            baseRadius = radiusSetting.Value;
            radiusStacking = radiusStackingSetting.Value;
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            attackSpeedStacking = attackSpeedStackingSetting.Value / 100.0f;
            regen = regenSetting.Value;
            regenStacking = regenStackingSetting.Value;
            regenPerLevel = regenPerLevelSetting.Value;
            regenMult = regenMultSetting.Value / 100.0f;
            buffDuration = buffDurationSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            speedQualityValues.UpdateValues(speedQualitySetting, 0.01f);
            durationQualityValues.UpdateValues(durationQualitySetting);
            durationStackingQualityValues.UpdateValues(durationStackingQualitySetting);
        }

        public override Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            return new Dictionary<string, string>
            {
                { "RADIUS", radiusSetting.Value.ToString() },
                { "RADIUS_STACKING", radiusStackingSetting.Value.ToString() },
                { "ATTACK_SPEED", attackSpeedSetting.Value.ToString() },
                { "ATTACK_SPEED_STACKING", attackSpeedStackingSetting.Value.ToString() },
                { "REGEN", regenSetting.Value.ToString() },
                { "REGEN_STACKING", regenStackingSetting.Value.ToString() }
            };
        }

        void LeadersPennonStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += Utils.CalculateStackingValue(_count, attackSpeed, attackSpeedStacking);

            // Modify regen speed
            _stats.baseRegenAdd += Utils.CalculateStackingValue(_count, regen, regenStacking);
            _stats.levelRegenAdd += regenPerLevel;
            _stats.regenMultAdd += regenMult;
        }

        void AllyWithItemToAlly(int _count, CharacterMaster _holder, CharacterMaster _other)
        {
            // Calculate effect radius
            float radius = Utils.CalculateStackingValue(_count, baseRadius, radiusStacking);     // REMEMBER TO SYNC THIS WITH FaithfulLeadersPennonBehaviour

            // Other ally in radius
            if ((_holder.GetBodyObject().transform.position - _other.GetBodyObject().transform.position).magnitude <= radius)
            {
                // Get body of other
                CharacterBody body = _other.GetBody();

                // Grant Leader's Pennon buffs
                GrantLeadersPennonBuffs(body, _count);
            }
        }

        private void GrantLeadersPennonBuffs(CharacterBody _body, int _count)
        {
            // Validate input
            if (_body == null || _count <= 0) return;

            // If ally doesn't have buff already
            if (_body.GetBuffCount(leadersPennonVisualBuff.buffDef) == 0)
            {
                // Grant buff
                _body.AddTimedBuff(leadersPennonVisualBuff.buffDef, buffDuration);

                // Get needed amount of buffs
                int needed = _count - _body.GetBuffCount(leadersPennonBuff.buffDef);

                // Catch up buff count
                for (int i = 0; i < needed; i++) _body.AddTimedBuff(leadersPennonBuff.buffDef, buffDuration);
            }
            else
            {
                // Refresh Leader's Pennon buffs
                Utils.RefreshTimedBuffs(_body, leadersPennonVisualBuff.buffDef, buffDuration);

                // Refresh needed amount of pennon buffs
                Utils.RefreshTimedBuffs(_body, leadersPennonBuff.buffDef, buffDuration, _count);

                // Add additional buffs if needed
                for (int i = 0; i < _count - _body.GetBuffCount(leadersPennonBuff.buffDef); i++) _body.AddTimedBuff(leadersPennonBuff.buffDef, buffDuration);
            }
        }

        private void OnCharacterBodyTick_Quality(CharacterBody _body)
        {
            // Validate input
            if (_body == null) return;

            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory == null) return;

            // Get Leader's Pennon amount
            int itemCount = inventory.GetItemCountEffective(MainItem.itemDef);
            if (itemCount <= 0) return;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(inventory, MainItem);
            if (qualityCounts.Total <= 0) return;

            // Possessing any quality variant lets the holder receive their own Leader's Pennon buffs
            GrantLeadersPennonBuffs(_body, itemCount);
        }

        private void OnPurchaseInteractionBegin_Quality(PurchaseInteraction _shop, CharacterMaster _activator)
        {
            // Validate input
            if (_shop == null || _activator == null || !_activator.hasBody) return;

            // Check for activator body and inventory
            CharacterBody activatorBody = _activator.GetBody();
            Inventory inventory = _activator.inventory;
            if (activatorBody == null || inventory == null) return;

            // Get Leader's Pennon amount
            int itemCount = inventory.GetItemCountEffective(MainItem.itemDef);
            if (itemCount <= 0) return;

            // Get quality item counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(inventory, MainItem);
            if (qualityCounts.Total <= 0) return;

            // Calculate effect values
            Quality effectiveQuality = qualityCounts.GetHighestQuality();
            float duration = GetSpeedBuffDuration_Quality(qualityCounts);
            float radius = Utils.CalculateStackingValue(itemCount, baseRadius, radiusStacking);
            if (duration <= 0.0f || radius <= 0.0f) return;

            // Get holder team
            TeamIndex teamIndex = activatorBody.teamComponent ? activatorBody.teamComponent.teamIndex : _activator.teamIndex;

            // Apply speed buff to nearby allies and the holder
            foreach (CharacterMaster ally in Utils.GetCharactersForTeam(teamIndex))
            {
                // Check for ally body
                CharacterBody allyBody = ally.GetBody();
                if (allyBody == null || allyBody.healthComponent == null || !allyBody.healthComponent.alive) continue;

                // Check if ally is holder or within the regular Leader's Pennon radius
                bool inRadius = ally == _activator || (activatorBody.corePosition - allyBody.corePosition).sqrMagnitude <= radius * radius;
                if (!inRadius) continue;

                // Apply speed buff
                ApplySpeedBuff_Quality(allyBody, effectiveQuality, duration);
            }
        }

        private float GetSpeedBuffDuration_Quality(QualityCounts _qualityCounts)
        {
            // Add up duration from all quality stacks
            float duration = 0.0f;
            duration += Utils.CalculateStackingValue(_qualityCounts.UNCOMMON, durationQualityValues.UNCOMMON, durationStackingQualityValues.UNCOMMON);
            duration += Utils.CalculateStackingValue(_qualityCounts.RARE, durationQualityValues.RARE, durationStackingQualityValues.RARE);
            duration += Utils.CalculateStackingValue(_qualityCounts.EPIC, durationQualityValues.EPIC, durationStackingQualityValues.EPIC);
            duration += Utils.CalculateStackingValue(_qualityCounts.LEGENDARY, durationQualityValues.LEGENDARY, durationStackingQualityValues.LEGENDARY);

            return duration;
        }

        private void ApplySpeedBuff_Quality(CharacterBody _body, Quality _quality, float _duration)
        {
            // Validate input
            if (_body == null || _duration <= 0.0f) return;

            // Refresh or apply visual speed buff
            if (_body.GetBuffCount(leadersPennonSpeedBuff.buffDef) > 0) Utils.RefreshTimedBuffs(_body, leadersPennonSpeedBuff.buffDef, _duration);
            else _body.AddTimedBuff(leadersPennonSpeedBuff.buffDef, _duration);

            // Get quality speed buff
            Buff speedBuff = GetSpeedBuff_Quality(_quality);
            if (speedBuff == null) return;

            // Refresh or apply quality speed buff
            if (_body.GetBuffCount(speedBuff.buffDef) > 0) Utils.RefreshTimedBuffs(_body, speedBuff.buffDef, _duration);
            else _body.AddTimedBuff(speedBuff.buffDef, _duration);
        }

        private Buff GetSpeedBuff_Quality(Quality _quality)
        {
            return _quality switch
            {
                Quality.UNCOMMON => leadersPennonSpeedUncommonBuff,
                Quality.RARE => leadersPennonSpeedRareBuff,
                Quality.EPIC => leadersPennonSpeedEpicBuff,
                Quality.LEGENDARY => leadersPennonSpeedLegendaryBuff,
                _ => null
            };
        }

        private void UncommonStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += speedQualityValues.UNCOMMON * _count;
        }

        private void RareStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += speedQualityValues.RARE * _count;
        }

        private void EpicStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += speedQualityValues.EPIC * _count;
        }

        private void LegendaryStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify movement speed
            _stats.moveSpeedMultAdd += speedQualityValues.LEGENDARY * _count;
        }

        void UpdateVisualEffects(CharacterBody _body)
        {
            // Check if buff effect is enabled
            if (!enableBuffEffect) return;

            // Check for character body
            if (_body == null) return;

            // Check for faithful character body behaviour
            FaithfulCharacterBodyBehaviour faithfulBehaviour = Utils.FindCharacterBodyHelper(_body);
            if (faithfulBehaviour == null) return;

            // Tell faithful leader's pennon behaviour to update it's visual effect
            faithfulBehaviour.leadersPennon.UpdateVisualEffect(_body.HasBuff(leadersPennonBuff.buffDef));
        }
    }
}
