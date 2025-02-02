using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class LeadersPennon : ItemBase
    {
        // Store item and buff
        Buff leadersPennonBuff;
        Item leadersPennonItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> enableRadiusIndicatorSetting;
        Setting<bool> enableBuffEffectSetting;
        Setting<float> radiusSetting;
        Setting<float> radiusStackingSetting;
        Setting<float> attackSpeedSetting;
        Setting<float> regenSetting;
        Setting<float> regenPerLevelSetting;
        Setting<float> regenMultSetting;
        Setting<float> buffDurationSetting;

        // Store item stats
        bool enableRadiusIndicator;
        bool enableBuffEffect;
        float baseRadius;
        float radiusStacking;
        float attackSpeed;
        float regen;
        float regenPerLevel;
        float regenMult;
        float buffDuration;

        // Constructor
        public LeadersPennon(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("leaderspennondisplaymesh");

            // Create Leader's Pennon item and buff
            leadersPennonBuff = Buffs.AddBuff("LEADERS_PENNON", "texbuffleaderarea", Color.white, false);
            leadersPennonItem = Items.AddItem("LEADERS_PENNON", [ItemTag.Utility, ItemTag.AIBlacklist], "texleaderspennonicon", "leaderspennonmesh", ItemTier.VoidTier1, _corruptToken: "ITEM_WARDONLEVEL_NAME", _displaySettings: displaySettings);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add ally to ally behaviour
            Behaviour.AddAllyToAllyCallback(leadersPennonItem, AllyWithItemToAlly);

            // Add update visual effects callback
            Behaviour.AddOnUpdateVisualEffectsCallback(UpdateVisualEffects);

            // Add stats modification
            Behaviour.AddStatsMod(leadersPennonBuff, LeadersPennonStatsMod);
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
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            enableRadiusIndicatorSetting = leadersPennonItem.CreateSetting("ENABLE_RADIUS_INDICATOR", "Enable Radius Indicator?", true, "Should this item have a radius indicator visual effect?", false, true);
            enableBuffEffectSetting = leadersPennonItem.CreateSetting("ENABLE_BUFF_EFFECT", "Enable Buff Visual Effect?", true, "Should this item's buff have a visual effect?", false, true);
            radiusSetting = leadersPennonItem.CreateSetting("RADIUS", "Radius", 15.0f, "How big should the base radius be of this item's effect? (15.0 = 15 meters)");
            radiusStackingSetting = leadersPennonItem.CreateSetting("RADIUS_STACKING", "Radius Stacking", 7.5f, "How much should the radius of this item's effect increase per stack? (7.5 = 7.5 meters)");
            attackSpeedSetting = leadersPennonItem.CreateSetting("ATTACK_SPEED", "Attack Speed", 30.0f, "How much should this item increase ally's attack speed? (30.0 = 30% increase)");
            regenSetting = leadersPennonItem.CreateSetting("REGEN", "Regen", 5.0f, "How much should this item increase ally's base regen? (5.0 = 5 hp/s)");
            regenPerLevelSetting = leadersPennonItem.CreateSetting("REGEN_PER_LEVEL", "Regen Per Level", 1.0f, "How much should this item increase ally's regen per level? (1.0 = 1 hp/s)");
            regenMultSetting = leadersPennonItem.CreateSetting("REGEN_MULT", "Regen Multiplier", 30.0f, "How much should this item increase ally's regen multiplicatively? (30.0 = 30% increase)", _canRandomise: false);
            buffDurationSetting = leadersPennonItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the radius of this item's effect? (1.0 = 1 second)", _minValue: 0.1f, _canRandomise: false);
        }

        public override void FetchSettings()
        {
            // Get item settings
            enableRadiusIndicator = enableRadiusIndicatorSetting.Value;
            enableBuffEffect = enableBuffEffectSetting.Value;
            baseRadius = radiusSetting.Value;
            radiusStacking = radiusStackingSetting.Value;
            attackSpeed = attackSpeedSetting.Value / 100.0f;
            regen = regenSetting.Value;
            regenPerLevel = regenPerLevelSetting.Value;
            regenMult = regenMultSetting.Value / 100.0f;
            buffDuration = buffDurationSetting.Value;

            // Update item texts with new settings
            leadersPennonItem.UpdateItemTexts();
        }

        void LeadersPennonStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify attack speed
            _stats.attackSpeedMultAdd += attackSpeed;

            // Modify regen speed
            _stats.baseRegenAdd += regen;
            _stats.levelRegenAdd += regenPerLevel;
            _stats.regenMultAdd += regenMult;
        }

        void AllyWithItemToAlly(int _count, CharacterMaster _holder, CharacterMaster _other)
        {
            // Calculate effect radius
            float radius = baseRadius + (_count - 1) * radiusStacking;     // REMEMBER TO SYNC THIS WITH FaithfulLeadersPennonBehaviour

            // Other ally in radius
            if ((_holder.GetBodyObject().transform.position - _other.GetBodyObject().transform.position).magnitude <= radius)
            {
                // Get body of other
                CharacterBody body = _other.GetBody();

                // If other ally doesn't have buff already
                if (body.GetBuffCount(leadersPennonBuff.buffDef) == 0)
                {
                    // Grant buff
                    body.AddTimedBuff(leadersPennonBuff.buffDef, buffDuration);
                }
                else
                {
                    // Refresh Leader's Pennon buffs on other ally
                    Utils.RefreshTimedBuffs(body, leadersPennonBuff.buffDef, buffDuration);
                }
            }
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
