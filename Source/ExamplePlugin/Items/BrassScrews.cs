using EntityStates;
using R2API;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class BrassScrews : ItemBase
    {
        // Store buff
        Buff brassScrewsBuff;
        Buff brassScrewsEffectBuff;

        // Quality buffs
        Buff brassScrewsQualityBuff;
        Buff brassScrewsQualityUncommonBuff;
        Buff brassScrewsQualityRareBuff;
        Buff brassScrewsQualityEpicBuff;
        Buff brassScrewsQualityLegendaryBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> buffDurationSetting;

        // Store item stats
        float damage;
        float damageStacking;
        float buffDuration;

        // Store additional quality settings
        QualitySetting<float> damageQualitySetting;
        QualitySetting<float> durationQualitySetting;
        QualitySetting<float> durationStackingQualitySetting;

        // Store quality item stats
        QualityValues<float> damageQualityValues = new();
        QualityValues<float> durationQualityValues = new();
        QualityValues<float> durationStackingQualityValues = new();

        // Constructor
        public BrassScrews(Toolbox _toolbox) : base(_toolbox, "BRASS_SCREWS")
        {
            // Create display settings
            CreateDisplaySettings("brassscrewsdisplaymesh");

            // Create Brass Screws item and buff
            MainItem = Items.AddItem(token, "Brass Screws", [ItemTag.Damage, ItemTag.Technology, ItemTag.HoldoutZoneRelated], "texbrassscrewsicon", "brassscrewsmesh", ItemTier.VoidTier1, _simulacrumBanned: true, _corruptToken: "FAITHFUL_ITEM_COPPER_GEAR_NAME", _supportsQuality: true, _displaySettings: displaySettings);
            brassScrewsBuff = Buffs.AddBuff("BRASS_SCREWS", "Brass Screws", "texbuffteleporterscrew", Color.white, false);
            brassScrewsEffectBuff = Buffs.AddBuff("BRASS_SCREWS_EFFECT", "Brass Screws", "texbuffteleporterscrew", Color.white, _isHidden: true, _hasConfig: false, _langTokenOverride: "BRASS_SCREWS");

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Add stats modification
            Behaviour.AddStatsMod(brassScrewsEffectBuff, BrassScrewsStatsMod);

            // Link Holdout Zone behaviour
            Behaviour.AddInHoldoutZoneCallback(InHoldoutZone);

            // Link Generic Character Fixed Update behaviour
            Behaviour.AddGenericCharacterFixedUpdateCallback(GenericCharacterFixedUpdate);
        }

        public override void QualityConstructor()
        {
            // Create Quality stuff
            brassScrewsQualityBuff = Buffs.AddBuff("BRASS_SCREWS_QUALITY", "Brass Kill", "texBuffDeathScrew", Color.white, _qualityBuff: true);
            brassScrewsQualityUncommonBuff = Buffs.AddBuff("BRASS_SCREWS_QUALITY_UNCOMMON", "Brass Kill", "texBuffDeathScrew", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "BRASS_SCREWS_QUALITY");
            brassScrewsQualityRareBuff = Buffs.AddBuff("BRASS_SCREWS_QUALITY_RARE", "Brass Kill", "texBuffDeathScrew", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "BRASS_SCREWS_QUALITY");
            brassScrewsQualityEpicBuff = Buffs.AddBuff("BRASS_SCREWS_QUALITY_EPIC", "Brass Kill", "texBuffDeathScrew", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "BRASS_SCREWS_QUALITY");
            brassScrewsQualityLegendaryBuff = Buffs.AddBuff("BRASS_SCREWS_QUALITY_LEGENDARY", "Brass Kill", "texBuffDeathScrew", Color.white, _isHidden: true, _hasConfig: false, _qualityBuff: true, _langTokenOverride: "BRASS_SCREWS_QUALITY");

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath_Quality);

            // Add stats mods for buffs
            Behaviour.AddStatsMod(brassScrewsQualityUncommonBuff, UncommonStatsMod_Quality);
            Behaviour.AddStatsMod(brassScrewsQualityRareBuff, RareStatsMod_Quality);
            Behaviour.AddStatsMod(brassScrewsQualityEpicBuff, EpicStatsMod_Quality);
            Behaviour.AddStatsMod(brassScrewsQualityLegendaryBuff, LegendaryStatsMod_Quality);
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
            displaySettings.AddCharacterDisplay("Commando", "LowerArmL", new Vector3(0.02264F, 0.16791F, -0.03632F), new Vector3(335.4802F, 67.51034F, 272.0254F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "LowerArmL", new Vector3(0.00565F, 0.10005F, -0.02699F), new Vector3(325.6556F, 67.49967F, 283.802F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Bandit", "LowerArmL", new Vector3(-0.0025F, 0.0835F, -0.0475F), new Vector3(340F, 90F, 270F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(1.75763F, 3.26321F, 0.01843F), new Vector3(315F, 220F, 60F), new Vector3(0.45F, 0.45F, 0.45F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadL", new Vector3(0.11935F, 0.38362F, 0.162F), new Vector3(326.5692F, 291.2021F, 269.7665F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, -0.43863F, -0.20092F), new Vector3(290F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Neck", new Vector3(0F, -0.43863F, -0.20092F), new Vector3(290F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
            displaySettings.AddCharacterDisplay("Artificer", "CalfR", new Vector3(0.02813F, 0.05546F, 0.01644F), new Vector3(11.75F, 168.75F, 88F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Mercenary", "LowerArmL", new Vector3(-0.02268F, 0.12083F, -0.031F), new Vector3(338.8F, 144.5F, 267.8F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("REX", "FootFrontL", new Vector3(0.04454F, 0.20025F, -0.03737F), new Vector3(340F, 40F, 270F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Loader", "MechLowerArmL", new Vector3(0.0042F, 0.53245F, -0.11887F), new Vector3(48.68909F, 51.01041F, 227.7835F), new Vector3(0.095F, 0.095F, 0.095F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmL", new Vector3(-0.793F, 3.70431F, 0.14379F), new Vector3(51.71926F, 146.3149F, 227.2395F), new Vector3(0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Acrid", "LowerArmR", new Vector3(0.7955F, 3.535F, 0.26625F), new Vector3(63.0825F, 231.25F, 159.35F), new Vector3(0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(-0.01064F, 0.01558F, -0.08823F), new Vector3(18.29989F, 139.6017F, 345.7164F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(-0.096F, -0.17875F, 0.1775F), new Vector3(316F, 305F, 160F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Void Fiend", "CalfL", new Vector3(0.0146F, 0.3535F, -0.03525F), new Vector3(10F, 275F, 95F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(5.19109F, 2.45005F, -2.17756F), new Vector3(342.2899F, 335.8934F, 339.8638F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Seeker", "Chest", new Vector3(-0.03825F, 0.3525F, -0.0625F), new Vector3(5.5F, 196.5F, 250F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("False Son", "LowerArmL", new Vector3(-0.1165F, 0.3125F, -0.052F), new Vector3(340F, 0.5F, 70F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Chef", "OvenDoor", new Vector3(-0.04625F, 0.10875F, 0.03775F), new Vector3(65F, 190F, 150F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Technician", "Shin.L", new Vector3(-0.03375F, 0.525F, 0.00185F), new Vector3(300F, 47.5F, 30F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Operator", "LowerArmR", new Vector3(0.08F, -0.045F, 0.045F), new Vector3(340F, 30F, 160F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Drifter", "BagFrontPocket", new Vector3(-0.09F, 0.5F, 0F), new Vector3(20F, 245F, 355F), new Vector3(0.075F, 0.075F, 0.075F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            damageSetting = MainItem.CreateSetting("DAMAGE", "Damage", 20.0f, "How much should this item increase damage while within the teleporter radius? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            damageStackingSetting = MainItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 20.0f, "How much should further stacks of this item increase damage while within the teleporter radius? (20.0 = 20% increase)", _valueFormatting: "{0:0.0}%");
            buffDurationSetting = MainItem.CreateSetting("BUFF_DURATION", "Buff Duration", 1.0f, "How long should the buff be retained after leaving the teleporter radius? (1.0 = 1 second)", _minValue: 0.1f, _canRandomise: false, _valueFormatting: "{0:0.00}s");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            damageQualitySetting = MainItem.CreateQualitySetting("DAMAGE", "Damage", 5.0f, 10.0f, 20.0f, 30.0f, "How much should each kill increase damage while within the teleporter radius? (5.0 = 5% increase)", _valueFormatting: "{0:0.0}%");
            durationQualitySetting = MainItem.CreateQualitySetting("DURATION", "Buff Duration", 2.5f, 5.0f, 10.0f, 15.0f, "How long should the damage buff last after each kill while within the teleporter radius? (2.5 = 2.5 seconds)", _valueFormatting: "{0:0.0}s");
            durationStackingQualitySetting = MainItem.CreateQualitySetting("DURATION_STACKING", "Buff Duration Stacking", 2.5f, 5.0f, 10.0f, 15.0f, "How much longer should further stacks of this item make the damage buff last after each kill while within the teleporter radius? (2.5 = 2.5 seconds)", _valueFormatting: "{0:0.0}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            buffDuration = buffDurationSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            damageQualityValues.UpdateValues(damageQualitySetting, 0.01f);
            durationQualityValues.UpdateValues(durationQualitySetting);
            durationStackingQualityValues.UpdateValues(durationStackingQualitySetting);
        }

        void BrassScrewsStatsMod(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Check for buff
            if (_count == 0) return;

            // Modify damage
            _stats.damageMultAdd += damage + damageStacking * (_count - 1);
        }

        void InHoldoutZone(CharacterBody _body, HoldoutZoneController _zone)
        {
            // Check for inventory
            Inventory inventory = _body.inventory;
            if (inventory)
            {
                // Get Brass Screws amount
                int brassScrewsCount = inventory.GetItemCount(MainItem.itemDef.itemIndex);

                // Has Brass Screws?
                if (brassScrewsCount > 0)
                {
                    // Refresh Brass Screws buffs
                    Utils.RefreshTimedBuffs(_body, brassScrewsEffectBuff.buffDef, buffDuration);

                    // Get needed amount of buffs
                    int needed = brassScrewsCount - _body.GetBuffCount(brassScrewsEffectBuff.buffDef);

                    // Check if there are too many buffs (can happen if the player loses items while in the zone)
                    if (needed < 0)
                    {
                        // Remove excess buffs
                        for (int i = 0; i < -needed; i++)
                        {
                            // Remove Brass Screws buff
                            _body.RemoveOldestTimedBuff(brassScrewsEffectBuff.buffDef);
                        }
                    }

                    // Either has enough buffs or needs more
                    else
                    {
                        // Catch up buff count
                        for (int i = 0; i < needed; i++)
                        {
                            // Add Brass Screws buff
                            _body.AddTimedBuff(brassScrewsEffectBuff.buffDef, buffDuration);
                        }
                    }
                }
            }
        }

        void GenericCharacterFixedUpdate(GenericCharacterMain _character)
        {
            // Check for character body and inventory
            CharacterBody characterBody = _character.characterBody;
            if (characterBody)
            {
                // Update visual buff
                characterBody.SetBuffCount(brassScrewsBuff.buffDef.buffIndex, characterBody.GetBuffCount(brassScrewsEffectBuff.buffDef.buffIndex) > 0 ? 1 : 0);
            }
        }

        private void UncommonStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage
            _stats.damageMultAdd += damageQualityValues.UNCOMMON * _count;
        }

        private void RareStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage
            _stats.damageMultAdd += damageQualityValues.RARE * _count;
        }

        private void EpicStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage
            _stats.damageMultAdd += damageQualityValues.EPIC * _count;
        }

        private void LegendaryStatsMod_Quality(int _count, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Modify damage
            _stats.damageMultAdd += damageQualityValues.LEGENDARY * _count;
        }

        private void OnCharacterDeath_Quality(DamageReport _report)
        {
            // Attempt to fetch attacker inventory and body
            if (_report == null) return;
            CharacterMaster master = _report.attackerMaster;
            if (master == null) return;
            Inventory inventory = master.inventory;
            if (inventory == null) return;
            CharacterBody body = master.GetBody();
            if (body == null) return;

            // Check if character is in a holdout zone
            if (Utils.GetHoldoutZonesContainingCharacter(master).Count <= 0) return;

            // Get item counts
            QualityCounts counts = QualityCompat.GetItemCountsEffective(inventory, MainItem);

            // Effective quality should be highest quality the attacker possesses
            Quality effectiveQuality = Quality.UNCOMMON;
            if (counts.LEGENDARY > 0) effectiveQuality = Quality.LEGENDARY;
            else if (counts.EPIC > 0) effectiveQuality = Quality.EPIC;
            else if (counts.RARE > 0) effectiveQuality = Quality.RARE;
            else if (counts.UNCOMMON <= 0) return;

            // Calculate buff duration
            float buffDuration = counts.UNCOMMON == 0 ? 0.0f : durationQualityValues.UNCOMMON + (counts.UNCOMMON - 1) * durationStackingQualityValues.UNCOMMON;
            buffDuration += counts.RARE == 0 ? 0.0f : durationQualityValues.RARE + (counts.RARE - 1) * durationStackingQualityValues.RARE;
            buffDuration += counts.EPIC == 0 ? 0.0f : durationQualityValues.EPIC + (counts.EPIC - 1) * durationStackingQualityValues.EPIC;
            buffDuration += counts.LEGENDARY == 0 ? 0.0f : durationQualityValues.LEGENDARY + (counts.LEGENDARY - 1) * durationStackingQualityValues.LEGENDARY;

            // Give buff
            switch (effectiveQuality)
            {
                case Quality.UNCOMMON:
                    body.AddTimedBuff(brassScrewsQualityUncommonBuff.buffDef, buffDuration);
                    break;

                case Quality.RARE:
                    body.AddTimedBuff(brassScrewsQualityRareBuff.buffDef, buffDuration);
                    break;

                case Quality.EPIC:
                    body.AddTimedBuff(brassScrewsQualityEpicBuff.buffDef, buffDuration);
                    break;

                case Quality.LEGENDARY:
                    body.AddTimedBuff(brassScrewsQualityLegendaryBuff.buffDef, buffDuration);
                    break;
            }

            // Give visual buff
            body.AddTimedBuff(brassScrewsQualityBuff.buffDef, buffDuration);
        }
    }
}
