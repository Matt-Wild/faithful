using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class CharmingFacade : ItemBase
    {
        // Buff tied to item
        Buff charmedBuff;

        // Server-side charm ownership
        readonly Dictionary<CharacterBody, HashSet<CharacterBody>> charmedVictims = new();
        readonly Dictionary<CharacterBody, CharacterBody> charmOwners = new();

        // Overlay for Charmed debuff
        static readonly Overlays.Overlay charmedOverlay = Overlays.CreateOverlay(new Overlays.OverlaySettings
        {
            MaterialAddress = "RoR2/Base/CritOnUse/matFullCrit.mat",
            Colour = new Color(0.32f, 0.0f, 0.64f, 0.64f)
        });

        // Visual for Charmed debuff
        static readonly BuffOverlayVisuals.BuffOverlayVisual charmedVisual = BuffOverlayVisuals.CreateBuffOverlayVisual(new BuffOverlayVisuals.BuffOverlayVisualSettings
        {
            Sprite = Assets.GetIcon("texCharmingFacadeBuff"),
            EffectName = "faithfulCharmedOverlayVisualEffect",
            Colour = new Color(0.32f, 0.0f, 0.32f)
        });

        // Item display settings
        ItemDisplaySettings displaySettings;

        // Item settings
        Setting<float> chanceSetting;
        Setting<int> limitSetting;
        Setting<int> limitStackingSetting;
        Setting<float> durationSetting;
        Setting<float> durationStackingSetting;

        // Item stats from settings
        float chance;
        int limit;
        int limitStacking;
        float duration;
        float durationStacking;

        // Item Quality settings
        QualitySetting<float> chanceQualitySetting;
        QualitySetting<float> chanceStackingQualitySetting;
        QualitySetting<float> bonusDurationQualitySetting;

        // Item Quality stats from settings
        QualityValues<float> chanceQualityValues = new();
        QualityValues<float> chanceStackingQualityValues = new();
        QualityValues<float> bonusDurationQualityValues = new();

        // Constructor
        public CharmingFacade(Toolbox _toolbox) : base(_toolbox, "CHARMING_FACADE")
        {
            // Create display settings
            CreateDisplaySettings("CharmingFacadeDisplayMesh");

            // Create main item
            MainItem = Items.AddItem(new ItemOptions(
                token: token,
                safeName: "Charming Facade",
                tags: [ItemTag.Damage, ItemTag.Utility, ItemTag.AIBlacklist],
                iconName: "texCharmingFacadeIcon",
                modelName: "CharmingFacadeMesh")
            {
                Tier = ItemTier.VoidTier3,
                CorruptToken = "ITEM_GHOSTONKILL_NAME",
                SupportsQuality = true,
                DisplaySettings = displaySettings,
                ModifyItemDisplayPrefabCallback = ModifyItemDisplayPrefab
            });

            // Create buff tied to item
            charmedBuff = Buffs.AddBuff(token, "Charmed", "texCharmingFacadeBuff", Color.white, false, _overlay: charmedOverlay, _visual: charmedVisual);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();
            
            // Register on hit behaviour
            Behaviour.AddOnHitEnemyLateCallback(OnHitEnemyLate);
            Behaviour.AddOnBuffFirstStackGainedCallback(OnBuffFirstStackGained);
            Behaviour.AddOnBuffFinalStackLostCallback(OnBuffFinalStackLost);
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath);
            Behaviour.AddOnBodyDestroyedCallback(OnBodyDestroyed);

            // Register charmed enemy targeting
            Behaviour.AddModifyAIEnemySearchCallback(ModifyAIEnemySearch);
            Behaviour.AddTrySelectAIEnemyCallback(TrySelectAIEnemy);
        }

        public override void QualityConstructor()
        {
            // CREATE QUALITY CONTENT AND BEHAVIOUR HERE (E.G. BUFFS ONLY USED BY QUALITY AND NEW QUALITY BEHAVIOUR HOOKS)
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
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(0F, 0.225F, 0.1625F), new Vector3(345F, 0F, 0F), new Vector3(0.17F, 0.17F, 0.17F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(0F, 0.1925F, 0.1125F), new Vector3(340F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(0F, 0.0285F, 0.13F), new Vector3(5F, 0F, 0F), new Vector3(0.11F, 0.11F, 0.11F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.395F, 3.05F, -1.15F), new Vector3(300F, 180F, 0F), new Vector3(0.5125F, 0.5125F, 0.5125F));
            displaySettings.AddCharacterDisplay("Engineer", "Chest", new Vector3(0F, 0.5525F, 0.2015F), new Vector3(0F, 0F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(0F, 0.5625F, -0.25F), new Vector3(0F, 0F, 0F), new Vector3(0.2F, 0.2F, 0.2F));
            displaySettings.AddCharacterDisplay("Walker Turret", "Head", new Vector3(0F, 0.75F, 0.6525F), new Vector3(0F, 0F, 0F), new Vector3(0.225F, 0.225F, 0.225F));
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(0F, 0.05F, 0.1234F), new Vector3(355F, 0F, 0F), new Vector3(0.095F, 0.095F, 0.095F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(0F, 0.13F, 0.14125F), new Vector3(350F, 0F, 0F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("REX", "Eye", new Vector3(0F, 0.875F, 0F), new Vector3(275F, 180F, 180F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Loader", "Head", new Vector3(0F, 0.105F, 0.1475F), new Vector3(0F, 0F, 0F), new Vector3(0.14F, 0.14F, 0.14F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(1.65025F, 2.336F, 0.4155F), new Vector3(344F, 72.5F, 96.25F), new Vector3(-0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Acrid", "Head", new Vector3(-1.65025F, 2.336F, 0.4155F), new Vector3(344F, 287.5F, 263.75F), new Vector3(0.95F, 0.95F, 0.95F));
            displaySettings.AddCharacterDisplay("Captain", "Head", new Vector3(0F, 0.0595F, 0.18F), new Vector3(6.25F, 0F, 0F), new Vector3(0.14F, 0.14F, 0.14F));
            displaySettings.AddCharacterDisplay("Railgunner", "Head", new Vector3(-0.08125F, 0.1375F, 0.0405F), new Vector3(330F, 295F, 2.5F), new Vector3(-0.09F, 0.09F, 0.09F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(0F, 0.0813F, 0.173F), new Vector3(342.5F, 0F, 358.75F), new Vector3(0.17F, 0.17F, 0.17F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(0F, 0.07575F, 0.13675F), new Vector3(0F, 0F, 0F), new Vector3(0.12F, 0.12F, 0.12F));
            displaySettings.AddCharacterDisplay("False Son", "Head", new Vector3(0F, 0.1825F, 0.1785F), new Vector3(2.5F, 0F, 0F), new Vector3(0.14F, 0.14F, 0.14F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.2F, 0.1675F, 0F), new Vector3(275F, 90F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.13625F, -0.185F, 0F), new Vector3(70F, 270.75F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.1115F, 0.223F, 0F), new Vector3(270F, 90F, 0F), new Vector3(0.125F, 0.125F, 0.125F));
            displaySettings.AddCharacterDisplay("Technician", "Head", new Vector3(0F, 0.135F, 0.142F), new Vector3(350F, 0F, 0F), new Vector3(0.165F, 0.165F, 0.165F));
            displaySettings.AddCharacterDisplay("Enforcer", "Head", new Vector3(-0.0355F, 0.13F, 0F), new Vector3(275F, 90F, 0F), new Vector3(0.13F, 0.13F, 0.13F));
            displaySettings.AddCharacterDisplay("Acolyte", "Head", new Vector3(-0.0731F, 0.2835F, 0F), new Vector3(275F, 90F, 0F), new Vector3(0.22F, 0.22F, 0.2F));
        }

        private void ModifyItemDisplayPrefab(GameObject _prefab)
        {
            // Get first display object
            GameObject rope = Utils.FindChildByName(_prefab.transform, "Rope.001");

            // Add dynamic bone behaviour
            DynamicBone dynamicBone = rope.AddComponent<DynamicBone>();

            // Set up dynamic bone config
            DynamicBoneConfig dynamicBoneConfig = new DynamicBoneConfig();
            dynamicBoneConfig.Damping = 0.375f;
            dynamicBoneConfig.Elasticity = 0.02f;
            dynamicBoneConfig.Stiffness = 0.5f;
            dynamicBoneConfig.Gravity = new Vector3(0.0f, -1.0f, 0.0f);
            dynamicBoneConfig.LocalGravity = new Vector3(0.5f, -0.1f, 0.0f);

            // Set up dynamic bone
            Utils.ConfigureDynamicBone(dynamicBone, dynamicBoneConfig);
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            chanceSetting = MainItem.CreateSetting("CHANCE", "Charm Chance", 1.0f, "What should the chance be to charm a non-boss enemy on hit? (1.0 = 1% chance)", _maxValue: 100.0f, _valueFormatting: "{0:0.0}%");
            limitSetting = MainItem.CreateSetting("LIMIT", "Charm Limit", 4, "How many enemies should this item be able to charm at one time? (4 = 4 enemies)");
            limitStackingSetting = MainItem.CreateSetting("LIMIT_STACKING", "Charm Limit Stacking", 4, "How many more enemies should each additional stack of this item be able to charm at one time? (4 = 4 enemies)");
            durationSetting = MainItem.CreateSetting("DURATION", "Charm Duration", 30.0f, "How long should each enemy remain charmed? (30.0 = 30 seconds)", _valueFormatting: "{0:0.0}s");
            durationStackingSetting = MainItem.CreateSetting("DURATION_STACKING", "Charm Duration Stacking", 30.0f, "How much longer should each additional stack of this item keep enemies charmed? (30.0 = 30 seconds)", _valueFormatting: "{0:0.0}s");

            // Create quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) CreateQualitySettings();
        }

        protected void CreateQualitySettings()
        {
            // Create quality settings specific to this item
            chanceQualitySetting = MainItem.CreateQualitySetting("CHANCE", "Charm Chance", 5.0f, 10.0f, 15.0f, 20.0f, "What should the chance be to charm a non-boss enemy on hit? (5.0 = 5% chance)", _valueFormatting: "{0:0.0}%");
            chanceStackingQualitySetting = MainItem.CreateQualitySetting("CHANCE_STACKING", "Charm Chance Stacking", 5.0f, 10.0f, 15.0f, 20.0f, "How much additional charm chance should each additional stack of this quality provide? (5.0 = 5% chance)", _valueFormatting: "{0:0.0}%");
            bonusDurationQualitySetting = MainItem.CreateQualitySetting("BONUS_DURATION", "Bonus Charm Duration", 30.0f, "How much longer should the charm last for every full 100% of charm chance? (30.0 = 30 seconds)", _valueFormatting: "{0:0.0}s");
        }

        public override void FetchSettings()
        {
            // Get item settings
            chance = chanceSetting.Value;
            limit = limitSetting.Value;
            limitStacking = limitStackingSetting.Value;
            duration = durationSetting.Value;
            durationStacking = durationStackingSetting.Value;

            // Fetch quality settings for this item if quality is enabled and this item supports quality
            if (MainItem.supportsQuality && Utils.qualityEnabled) FetchQualitySettings();

            // Update item texts with new settings
            MainItem.UpdateItemTexts();
        }

        protected void FetchQualitySettings()
        {
            // Update item quality values
            chanceQualityValues.UpdateValues(chanceQualitySetting);
            chanceStackingQualityValues.UpdateValues(chanceStackingQualitySetting);
            bonusDurationQualityValues.UpdateValues(bonusDurationQualitySetting);
        }

        public override Dictionary<string, string> QualityDescriptionManualTokens(Quality _quality)
        {
            // Use the normal charm limit and duration settings for all qualities
            return new Dictionary<string, string>
            {
                { "LIMIT", limitSetting.Value.ToString() },
                { "LIMIT_STACKING", limitStackingSetting.Value.ToString() },
                { "DURATION", durationSetting.Value.ToString() },
                { "DURATION_STACKING", durationStackingSetting.Value.ToString() }
            };
        }

        private void OnHitEnemyLate(DamageInfo _info, GameObject _victim)
        {
            // Server authoritative behaviour
            if (!Utils.hosting) return;

            // Ignore rejected hits
            if (_info.rejected) return;

            // Require positive proc
            if (_info.procCoefficient <= 0.0f) return;

            // Get victim body
            CharacterBody victimBody = _victim.GetComponent<CharacterBody>();
            if (victimBody == null) return;

            // Only living victims can occupy charm slots
            if (!victimBody.healthComponent || !victimBody.healthComponent.alive) return;

            // Ignore bosses
            if (victimBody.isBoss) return;

            // Ignore if victim already has charmed buff
            if (victimBody.HasBuff(charmedBuff.buffDef)) return;

            // Get attacker body
            if (_info.attacker == null) return;
            CharacterBody attackerBody = _info.attacker.GetComponent<CharacterBody>();
            if (attackerBody == null) return;

            // Get attacker inventory
            Inventory attackerInventory = attackerBody.inventory;
            if (attackerInventory == null) return;

            // Validate item and buff
            if (charmedBuff == null || charmedBuff.buffDef == null) return;
            if (MainItem == null || MainItem.itemDef == null) return;

            // Get item count
            int attackerItemCount = attackerInventory.GetItemCountEffective(MainItem.itemDef.itemIndex);
            if (attackerItemCount <= 0) return;

            // Stop attempting new charms when this character is at its current limit
            int charmLimit = Utils.CalculateStackingValue(attackerItemCount, limit, limitStacking);
            if (charmLimit <= 0) return;
            if (charmedVictims.TryGetValue(attackerBody, out HashSet<CharacterBody> victims) && victims.Count >= charmLimit) return;

            // Calculate charm chance and duration
            float charmChance = 0.0f;
            float charmDuration = 0.0f;

            // Override charm chance and duration with Quality if quality is enabled
            if (Utils.qualityEnabled)
            {
                // Get quality item count
                QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(attackerInventory, MainItem);
                if (qualityCounts.Total > 0)
                {
                    // Add Quality chance
                    charmChance = Utils.CalculateStackingValue(qualityCounts.UNCOMMON, chanceQualityValues.UNCOMMON, chanceStackingQualityValues.UNCOMMON)
                        + Utils.CalculateStackingValue(qualityCounts.RARE, chanceQualityValues.RARE, chanceStackingQualityValues.RARE)
                        + Utils.CalculateStackingValue(qualityCounts.EPIC, chanceQualityValues.EPIC, chanceStackingQualityValues.EPIC)
                        + Utils.CalculateStackingValue(qualityCounts.LEGENDARY, chanceQualityValues.LEGENDARY, chanceStackingQualityValues.LEGENDARY);

                    // Apply proc coefficient to charm chance
                    charmChance *= _info.procCoefficient;

                    // Set base duration
                    charmDuration = Utils.CalculateStackingValue(attackerItemCount - qualityCounts.Total, duration, durationStacking);

                    // Add Quality bonus duration
                    int durationBonuses = Mathf.FloorToInt(charmChance / 100.0f) - 1;
                    if (durationBonuses > 0)
                    {
                        // Get duration bonus to multiply
                        float baseDurationBonus = qualityCounts.LEGENDARY > 0 ? bonusDurationQualityValues.LEGENDARY : 
                            qualityCounts.EPIC > 0 ? bonusDurationQualityValues.EPIC :
                            qualityCounts.RARE > 0 ? bonusDurationQualityValues.RARE :
                            bonusDurationQualityValues.UNCOMMON;

                        // Add bonus duration
                        charmDuration += durationBonuses * baseDurationBonus;
                    }
                }

                // No Quality items
                else
                {
                    charmDuration = Utils.CalculateStackingValue(attackerItemCount, duration, durationStacking);
                }

                // Incase there aren't any Quality items or Quality reduces charm chance below base
                float baseChance = chance * _info.procCoefficient;
                if (charmChance < baseChance) charmChance = baseChance;
            }

            // No Quality
            else
            {
                // Calculate charm chance and duration
                charmChance = chance * _info.procCoefficient;
                charmDuration = Utils.CalculateStackingValue(attackerItemCount, duration, durationStacking);
            }

            // Check for duration
            if (charmDuration <= 0.0f) return;

            // Roll for charm
            if (!Util.CheckRoll(charmChance, attackerBody.master)) return;

            // Record ownership before applying the buff so callbacks can find the owner
            if (victims == null)
            {
                victims = new HashSet<CharacterBody>();
                charmedVictims.Add(attackerBody, victims);
            }
            victims.Add(victimBody);
            charmOwners[victimBody] = attackerBody;

            // Apply charmed buff to victim
            victimBody.AddTimedBuff(charmedBuff.buffDef, charmDuration);

            // Do not retain a slot if the buff was prevented or the victim died during application
            if (!victimBody.healthComponent.alive || !victimBody.HasBuff(charmedBuff.buffDef)) ReleaseCharm(victimBody);
        }

        private void OnBuffFirstStackGained(CharacterBody _body, BuffDef _buffDef)
        {
            if (!Utils.hosting || _buffDef != charmedBuff.buffDef) return;

            // Ownership is recorded before the buff is applied
            Utils.ResetCharacterAITargets(_body);
        }

        private void OnBuffFinalStackLost(CharacterBody _body, BuffDef _buffDef)
        {
            if (!Utils.hosting || _buffDef != charmedBuff.buffDef) return;

            // Remove ownership before reacquiring with normal targeting rules
            ReleaseCharm(_body);
            Utils.ResetCharacterAITargets(_body);
        }

        private void OnCharacterDeath(DamageReport _report)
        {
            if (!Utils.hosting) return;
            ReleaseCharm(_report.victimBody);
        }

        private void OnBodyDestroyed(CharacterBody _body)
        {
            if (!Utils.hosting) return;

            // The destroyed character may be both a victim and an owner
            ReleaseCharm(_body);
            if (!charmedVictims.TryGetValue(_body, out HashSet<CharacterBody> victims)) return;

            // Detach the set before ending its charms so buff callbacks cannot change the iteration
            charmedVictims.Remove(_body);
            foreach (CharacterBody victim in victims)
            {
                // End the timed buff so clients also remove the charm's visuals
                if (victim) victim.ClearTimedBuffs(charmedBuff.buffDef);
                ReleaseCharm(victim);
            }
        }

        private void ReleaseCharm(CharacterBody _victim)
        {
            // Repeated expiry, death and destruction callbacks can safely release the same victim
            if (ReferenceEquals(_victim, null) || !charmOwners.TryGetValue(_victim, out CharacterBody owner)) return;
            charmOwners.Remove(_victim);

            if (charmedVictims.TryGetValue(owner, out HashSet<CharacterBody> victims))
            {
                victims.Remove(_victim);
                if (victims.Count == 0) charmedVictims.Remove(owner);
            }
        }

        private void ModifyAIEnemySearch(BaseAI _ai, BullseyeSearch _search)
        {
            if (!Utils.hosting || !IsCharmed(_ai.body)) return;

            // Include the character's own team, but never neutral
            _search.teamMaskFilter = TeamMask.allButNeutral;
        }

        private bool TrySelectAIEnemy(BaseAI _ai, IEnumerable<HurtBox> _candidates, out HurtBox _target)
        {
            _target = null;

            // Leave normal characters to vanilla targeting
            if (!Utils.hosting || !TryGetCharmOwner(_ai.body, out CharacterBody owner)) return false;
            if (!owner || !owner.teamComponent) return true;

            TeamIndex ownerTeam = owner.teamComponent.teamIndex;
            HurtBox fallback = null;

            // Vanilla's search already returns candidates in distance order
            foreach (HurtBox candidate in _candidates)
            {
                if (!candidate || !candidate.healthComponent) continue;

                HealthComponent health = candidate.healthComponent;
                CharacterBody body = health.body;

                // Exclude invalid targets and the searching character itself
                if (!health.alive || !body || body == _ai.body) continue;
                if (!body.teamComponent || body.teamComponent.teamIndex == TeamIndex.Neutral) continue;

                // Preserve vanilla's flying and spirit form exclusions
                if (_ai.ignoreFliers && body.isFlying) continue;
                if (body.bodyIndex == DLC4Content.BodyPrefabs.PlayerSpiritForm.bodyIndex) continue;

                // Prefer the nearest valid target outside the charmer's team
                if (body.teamComponent.teamIndex != ownerTeam)
                {
                    _target = candidate;
                    return true;
                }

                // Retain the nearest fallback on the charmer's team
                if (!fallback) fallback = candidate;
            }

            _target = fallback;
            return true;
        }

        private bool IsCharmed(CharacterBody _body)
        {
            // Check for valid body
            if (ReferenceEquals(_body, null)) return false;

            return charmOwners.ContainsKey(_body);
        }

        private bool CanCharmEnableHit(CharacterBody _attacker, CharacterBody _victim)
        {
            return IsCharmed(_attacker) || IsCharmed(_victim);
        }

        private bool TryGetCharmOwner(CharacterBody _body, out CharacterBody _owner)
        {
            // Check for valid body
            if (ReferenceEquals(_body, null))
            {
                _owner = null;
                return false;
            }

            return charmOwners.TryGetValue(_body, out _owner);
        }
    }
}
