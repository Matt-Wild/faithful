using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class FaithfulTJetpackBehaviour : NetworkBehaviour, ICharacterBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

        // Store buffs for fuel amount
        protected Buff emptyFuelBuff;
        protected Buff lowFuelBuff;
        protected Buff highFuelBuff;

        // Current displayed fuel buff percentage
        protected int currentFuelBuffPerc;

        // Keep clients aware of whether the server is using display-only fuel values
        [SyncVar] private bool fuelBuffUsesDisplayValue;

        // Store if active
        protected bool active = false;

        // Store item count
        protected int itemCount = 0;

        // Store buffed jetpack item count
        protected int buffedItemCount = 5;

        // Store jetting state, last jet frame time and jet time
        //protected bool jetting = false;
        //protected float lastJetTime;
        //protected float fuelUsed = 0.0f;

        // Store time when deactivated
        //protected float deactivatedTime;

        // Jetpack stats
        protected float baseMaxVelocity = 42.0f;
        protected float baseRisingAcceleration = 36.0f;
        protected float fallingAcceleration = 320.0f;
        protected float baseFuel = 4.0f;
        protected float fuelPerStack = 2.0f;
        protected float baseRefuelDuration = 12.0f;
        protected float refuelDurationReductionPerStack = 0.2f;
        protected float maxVelocityMultiplier = 1.0f;
        protected float accelerationMultiplier = 1.0f;

        // Buffed jetpack additive stats
        protected float maxVelocityBuff = 10.0f;
        protected float risingAccelerationBuff = 8.0f;

        // Store current state of jetpack
        protected bool grounded = false;
        protected bool jetActivated = false;
        protected bool firstJet = true;
        protected bool refueling = false;
        protected float fuelUsed = 0.0f;
        protected float lastJetTime;

        // Store last sent jetpack state
        protected bool hasSyncedJetpack = false;
        protected JetpackSyncData lastSyncedJetpack;
        protected float lastJetpackSyncTime;
        protected const float jetpackSyncRefreshInterval = 1.0f;

        // Store if this jetpack has a display
        protected bool hasDisplay = false;

        // If fall damage is ignored when jetting
        protected bool ignoreFallDamage = false;

        // Store reference to 4-T0N Jetpack game objects
        protected GameObject jetpack;
        protected GameObject additionalBoosters;
        protected GameObject dial;
        protected GameObject jetMiddle;
        protected GameObject jetLeft;
        protected GameObject jetRight;
        protected GameObject jetsOn;
        protected GameObject jetsOff;

        // Store if this jetpack has initialised
        protected bool initialised = false;

        // Quality "ignite" settings
        protected static readonly float igniteRange = 14.0f;
        protected static readonly float igniteAngle = 45.0f;
        protected static readonly float baseIgniteDuration = 4.0f;
        protected static readonly float directDamageFraction = 0.5f;
        protected static readonly float burnDamageFraction = 0.5f;
        protected static readonly float igniteDurationIncreaseFraction = 0.25f;  // Must be between 0 and 1, increases ignite duration when over 100% base damage (e.g. 200% base damage increases duration by 100% * igniteDurationIncreaseFraction (0.25) = 25%)
        
        // Quality "ignite" state
        protected bool hasDoneInitialIgnite = false;
        protected float igniteStopwatch = 0.0f;

        public FaithfulTJetpackBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void AssignCharacter(CharacterBody _character)
        {
            // Assign character
            character = _character;
        }

        protected void Init(CharacterBody _characterBody)
        {
            // Check if already initialised
            if (initialised && (jetpack != null || !hasDisplay)) return;

            // Set as initialised
            initialised = true;

            // Fetch jetpack settings
            FetchSettings();

            // Fetch buffs
            emptyFuelBuff = Buffs.GetBuff("4T0N_JETPACK_EMPTY");
            lowFuelBuff = Buffs.GetBuff("4T0N_JETPACK_LOW");
            highFuelBuff = Buffs.GetBuff("4T0N_JETPACK_HIGH");

            // Get character model
            Transform characterModel = _characterBody.modelLocator.modelTransform;

            // Get jetpack game object
            jetpack = Utils.FindChildByName(characterModel, "4T0NJetpackDisplayMesh(Clone)");

            // Check for jetpack
            if (jetpack == null)
            {
                // Does not have display
                hasDisplay = false;

                // Set additional components to null
                additionalBoosters = null;
                dial = null;
                jetMiddle = null;
                jetLeft = null;
                jetRight = null;
                jetsOff = null;

                // Create empty jets on game object
                jetsOn = new GameObject("JetsOn");

                // Set jets on parent
                jetsOn.transform.SetParent(characterModel, false);

                // Create mage jet ak events
                GameObject mageJetEffects = Instantiate(Assets.mageJetAkEventsPrefab);
                mageJetEffects.transform.SetParent(jetsOn.transform, false);
                mageJetEffects.SetActive(true);

                // Done
                return;
            }

            // Has display
            hasDisplay = true;

            // Fetch the additional boosters and dial game objects
            additionalBoosters = Utils.FindChildByName(jetpack.transform, "Pack");
            dial = Utils.FindChildByName(jetpack.transform, "Dial");
            jetMiddle = Utils.FindChildByName(jetpack.transform, "Jet_Middle");
            jetLeft = Utils.FindChildByName(jetpack.transform, "Jet_Left");
            jetRight = Utils.FindChildByName(jetpack.transform, "Jet_Right");

            // Add jet flare behaviour
            jetpack.GetComponent<Transform>().Find("4-T0N_Jetpack_Jetflare_Display").gameObject.AddComponent<TJetpackJetFlare>();

            // Get jet lights objects
            jetsOn = jetpack.GetComponent<Transform>().Find("Jets_On").gameObject;
            jetsOff = jetpack.GetComponent<Transform>().Find("Jets_Off").gameObject;

            // Disable jet lights
            jetsOn.SetActive(false);

            // Create mage jet ak events
            GameObject mageJetAkEvents = Instantiate(Assets.mageJetAkEventsPrefab);
            mageJetAkEvents.transform.SetParent(jetsOn.transform, false);
            mageJetAkEvents.SetActive(true);

            // Add flicker light to point lights
            FlickerLight flickerMiddle = jetsOn.GetComponent<Transform>().Find("Point_Light_Middle").gameObject.AddComponent<FlickerLight>();
            FlickerLight flickerLeft = jetsOn.GetComponent<Transform>().Find("Point_Light_Left").gameObject.AddComponent<FlickerLight>();
            FlickerLight flickerRight = jetsOn.GetComponent<Transform>().Find("Point_Light_Right").gameObject.AddComponent<FlickerLight>();
            flickerMiddle.light = jetsOn.GetComponent<Transform>().Find("Point_Light_Middle").GetComponent<Light>();
            flickerMiddle.sinWaves = Assets.mageJetWaves;
            flickerLeft.light = jetsOn.GetComponent<Transform>().Find("Point_Light_Left").GetComponent<Light>();
            flickerLeft.sinWaves = Assets.mageJetWaves;
            flickerRight.light = jetsOn.GetComponent<Transform>().Find("Point_Light_Right").GetComponent<Light>();
            flickerRight.sinWaves = Assets.mageJetWaves;

            FlickerLight flickerMiddleOff = jetsOff.GetComponent<Transform>().Find("Point_Light_Middle").gameObject.AddComponent<FlickerLight>();
            FlickerLight flickerLeftOff = jetsOff.GetComponent<Transform>().Find("Point_Light_Left").gameObject.AddComponent<FlickerLight>();
            FlickerLight flickerRightOff = jetsOff.GetComponent<Transform>().Find("Point_Light_Right").gameObject.AddComponent<FlickerLight>();
            flickerMiddleOff.light = jetsOff.GetComponent<Transform>().Find("Point_Light_Middle").GetComponent<Light>();
            flickerMiddleOff.sinWaves = Assets.mageJetWaves;
            flickerLeftOff.light = jetsOff.GetComponent<Transform>().Find("Point_Light_Left").GetComponent<Light>();
            flickerLeftOff.sinWaves = Assets.mageJetWaves;
            flickerRightOff.light = jetsOff.GetComponent<Transform>().Find("Point_Light_Right").GetComponent<Light>();
            flickerRightOff.sinWaves = Assets.mageJetWaves;

            // Check if Artificer or Seeker
            if (characterModel.name == "mdlMage" || characterModel.name == "mdlSeeker")
            {
                // Disable additional boosters
                additionalBoosters.transform.localScale = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            // Stop routed jetpack behaviour
            Deactivate();

            // Unregister with utils
            Utils.UnregisterCharacterBehaviour(this);
        }

        public void FetchSettings()
        {
            // Get jetpack item
            Item jetpackItem = Items.GetItem("4T0N_JETPACK");

            // Update stats
            ignoreFallDamage = jetpackItem.FetchSetting<bool>("DISABLE_FALL_DAMAGE").Value;
            baseFuel = jetpackItem.FetchSetting<float>("FUEL_TIME").Value;
            fuelPerStack = jetpackItem.FetchSetting<float>("FUEL_TIME_STACKING").Value;
            baseRefuelDuration = jetpackItem.FetchSetting<float>("RECHARGE_TIME").Value;
            refuelDurationReductionPerStack = jetpackItem.FetchSetting<float>("RECHARGE_TIME_REDUCTION").Value / 100.0f;
            maxVelocityMultiplier = jetpackItem.FetchSetting<float>("MAX_VELOCITY_MULTIPLIER").Value;
            accelerationMultiplier = jetpackItem.FetchSetting<float>("ACCELERATION_MULTIPLIER").Value;
        }

        public void UpdateItemCount(int _itemCount)
        {
            // Update item count
            itemCount = _itemCount;

            // Check if should be activated
            if (!active && itemCount > 0)
            {
                // Activate
                Activate();
            }
            else if (active && itemCount == 0)  // Otherwise, check if should be deactivated
            {
                // Deactivate
                Deactivate();
            }
        }

        protected void Activate()
        {
            // Check if already active and can be activated
            if (active || character == null)
            {
                return;
            }

            // Set active
            active = true;

            // Reset state of jetpack
            grounded = false;
            jetActivated = false;
            firstJet = true;
            refueling = false;
            fuelUsed = 0.0f;

            // Send initial state after activation
            hasSyncedJetpack = false;

            // Register callbacks for this character
            Behaviour.AddProcessJumpOverrideCallback(character, OnProcessJump);
            Behaviour.AddCharacterPreFixedUpdateCallback(character, OnFixedUpdate);
            Behaviour.AddArtificerProcessJumpOverrideCallback(character, OnArtificerProcessJump);
            Behaviour.AddArtificerJetpackFixedUpdateOverrideCallback(character, SkipArtificerJetpackFixedUpdate);
            Behaviour.AddFallDamageImmunityCallback(character, IsImmuneToFallDamage);
        }

        protected void Deactivate()
        {
            // Check if not active
            if (!active)
            {
                return;
            }

            // Set active
            active = false;

            // Remove callbacks when the item is lost or the character is destroyed
            Behaviour.RemoveProcessJumpOverrideCallback(character, OnProcessJump);
            Behaviour.RemoveCharacterPreFixedUpdateCallback(character, OnFixedUpdate);
            Behaviour.RemoveArtificerProcessJumpOverrideCallback(character, OnArtificerProcessJump);
            Behaviour.RemoveArtificerJetpackFixedUpdateOverrideCallback(character, SkipArtificerJetpackFixedUpdate);
            Behaviour.RemoveFallDamageImmunityCallback(character, IsImmuneToFallDamage);
        }

        public void OnFixedUpdate(GenericCharacterMain self)
        {
            if (!active) return;
            // Ensure jetpack is initialised
            Init(self.characterBody);

            // Update visuals
            UpdateVisuals();

            // Update display buff
            UpdateDisplayBuff();

            // Update quality ignite
            UpdateIgnite();

            // Sync jetpack
            SyncJetpack();
        }

        public bool OnArtificerProcessJump(EntityStates.Mage.MageCharacterMain self)
        {
            if (!active) return false;
            // Get if jetpack state is currently on
            bool jetOn = self.jetpackStateMachine.state.GetType() == typeof(EntityStates.Mage.JetpackOn);

            // Check if TJetpack behaviour should be run
            if (self.hasCharacterMotor && self.hasInputBank && self.isAuthority && (jetActivated || (character.inputBank.jump.down && canBeActivated)))
            {
                // Do jetpack behaviour
                JetpackBehaviour(self);

                // Is jetpack state currently on but needs to be turned off?
                if (jetOn && !jetActivated)
                {
                    // Deactivate jetpack state
                    self.jetpackStateMachine.SetNextState(new Idle());
                }

                // Is jetpack state currently off but needs to be turned on?
                else if (!jetOn && jetActivated)
                {
                    // Activate jetpack state
                    self.jetpackStateMachine.SetNextState(new EntityStates.Mage.JetpackOn());
                }
                return true;
            }
            return false;
        }

        public bool OnProcessJump(GenericCharacterMain self)
        {
            // Leave normal jump handling alone when this character has no authority or motor
            if (!active || !self.hasCharacterMotor || !self.hasInputBank || !self.isAuthority) return false;

            // Do jetpack behaviour
            JetpackBehaviour(self);

            // Skip normal jump handling while the jetpack takes control
            return jetActivated || (character.inputBank.jump.down && canBeActivated);
        }

        public bool IsImmuneToFallDamage(GlobalEventManager _manager, CharacterBody _body)
        {
            return active && ignoreFallDamage && jetActivated;
        }

        protected void JetpackBehaviour(GenericCharacterMain self)
        {
            // Check if game is paused
            if (Time.timeScale == 0.0f)
            {
                // Increase last jet time
                lastJetTime += Time.fixedDeltaTime;

                // Skip behaviour
                return;
            }

            // Check if grounded
            if (self.isGrounded)
            {
                // Run grounded behaviour
                Grounded();
            }
            else
            {
                // Run mid-air behaviour
                MidAir();
            }

            // Run refuel behaviour
            Refuel();
        }

        protected void UpdateVisuals()
        {
            // Check if has display
            if (hasDisplay)
            {
                // Update dial rotation
                dial.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 360.0f * fuelRemainingPerc);

                // Get new jet visuals scale
                Vector3 newJetScale = jetActivated ? new Vector3(1.0f, 3.5f, 1.0f) : new Vector3(1.0f, 1.0f, 1.0f);

                // Update jet visuals
                jetMiddle.transform.localScale = newJetScale;
                jetLeft.transform.localScale = newJetScale;
                jetRight.transform.localScale = newJetScale;

                // Update jet effects
                jetsOn.SetActive(jetActivated);
                jetsOff.SetActive(!jetActivated);
            }
            
            // Does not have display
            else
            {
                // Update jet effects
                jetsOn.SetActive(jetActivated);
            }
        }

        protected void UpdateDisplayBuff()
        {
            if (!Utils.hosting) return;

            // Fetch fuel percentage
            int fuelPerc = Mathf.CeilToInt(fuelRemainingPerc * 100.0f);

            // Check if fuel percentage has changed
            bool useDisplayValue = Behaviour.buffDisplayValuesAvailable;
            if (fuelPerc == currentFuelBuffPerc && fuelBuffUsesDisplayValue == useDisplayValue) return;
            fuelBuffUsesDisplayValue = useDisplayValue;

            // Update current fuel buff percentage
            currentFuelBuffPerc = fuelPerc;

            // Get needed buff to be displayed
            if (fuelPerc == 0)
            {
                // Update buff counts
                character.SetBuffCount(emptyFuelBuff.buffDef.buffIndex, 1);
                character.SetBuffCount(lowFuelBuff.buffDef.buffIndex, 0);
                character.SetBuffCount(highFuelBuff.buffDef.buffIndex, 0);
            }
            else if (fuelPerc < 26)
            {
                // Update buff counts
                character.RemoveBuff(emptyFuelBuff.buffDef.buffIndex);
                character.SetBuffCount(lowFuelBuff.buffDef.buffIndex, useDisplayValue ? 1 : fuelPerc);
                character.SetBuffCount(highFuelBuff.buffDef.buffIndex, 0);
            }
            else
            {
                // Update buff counts
                character.RemoveBuff(emptyFuelBuff.buffDef.buffIndex);
                character.SetBuffCount(lowFuelBuff.buffDef.buffIndex, 0);
                character.SetBuffCount(highFuelBuff.buffDef.buffIndex, useDisplayValue ? 1 : fuelPerc);
            }
        }

        public bool SkipArtificerJetpackFixedUpdate(EntityStates.Mage.JetpackOn _state)
        {
            // Prioritise 4-T0N Jetpack while it is thrusting
            return active && jetActivated;
        }

        protected void Grounded()
        {
            // Check for first contact with ground
            if (!grounded)
            {
                // Deactivate jet
                DeactivateJet();

                // Update grounded
                grounded = true;
            }
        }

        protected void MidAir()
        {
            // Update grounded
            grounded = false;

            // Is jet already activated?
            if (jetActivated)
            {
                // Run jetting mid-air behaviour instead
                JettingMidAir();
                return;
            }
            
            // Check if should activate jetpack
            if (character.inputBank.jump.down && canBeActivated)
            {
                // Activate jetpack
                ActivateJet();

                // Jet
                Jet();
            }
        }

        protected void Refuel()
        {
            // Do not attempt to refuel if currently jetting or not grounded
            if (jetActivated || !grounded)
            {
                // Ensure not refueling
                refueling = false;
                return;
            }

            // Skip if no fuel used or artificer hover is active
            if (fuelUsed == 0.0f)
            {
                return;
            }

            // Is already refueling?
            if (refueling)
            {
                // Decrease fuel used
                fuelUsed = Mathf.Max(fuelUsed - refuelRate * Time.fixedDeltaTime, 0.0f);
            }
            else
            {
                // Start refueling
                refueling = true;
            }
        }

        protected void JettingMidAir()
        {
            // Check if jetpack should be deactivated
            if (!character.inputBank.jump.down || fuelRemaining == 0.0f || character.characterMotor.velocity.y > maxVelocity)
            {
                // Deactivate jetpack
                DeactivateJet();
                return;
            }

            // Jet
            Jet();
        }

        protected void ActivateJet()
        {
            // Skip if jet is already active
            if (jetActivated) return;

            // Activate jetpack
            jetActivated = true;

            // Indicate that jet is recently activated
            firstJet = true;
        }

        protected void DeactivateJet()
        {
            // Skip if jet is already inactive
            if (!jetActivated) return;

            // Deactivate jetpack
            jetActivated = false;
        }

        protected void Jet()
        {
            // Get current y velocity
            float yVel = character.characterMotor.velocity.y;

            // Calculate force (give an extra boost until 30% of the max velocity is reached)
            float force = (risingAcceleration + Mathf.Clamp01((-yVel + maxVelocity * 0.3f) / 80.0f) * dynamicAccelerationDifference) * jumpPowerModifier * accelerationMultiplier;

            // Apply jet force to character
            character.characterMotor.velocity = new Vector3(character.characterMotor.velocity.x, Mathf.MoveTowards(yVel, maxVelocity, force * Time.fixedDeltaTime), character.characterMotor.velocity.z);

            // Don't reduce fuel if first jet since activation
            if (firstJet)
            {
                // Remove first jet state
                firstJet = false;
            }
            else
            {
                // Reduce fuel
                ReduceFuel();
            }

            // Update time since last jet
            lastJetTime = Time.time;
        }

        protected void ReduceFuel()
        {
            // Increase fuel used
            fuelUsed = Mathf.Min(fuelUsed + timeSinceLastJet, fuelCapacity);
        }

        protected void SyncJetpack()
        {
            // Only sync if belonging to the local player and net utils is found
            if (character == Utils.localPlayerBody && Utils.netUtils != null)
            {
                // Skip unchanged state, but periodically refresh clients which missed an earlier update
                if (hasSyncedJetpack && fuelUsed == lastSyncedJetpack.fuelUsed && jetActivated == lastSyncedJetpack.jetActivated && Time.fixedTime - lastJetpackSyncTime < jetpackSyncRefreshInterval) return;

                // Send sync request to net utils
                JetpackSyncData data = new JetpackSyncData(fuelUsed, jetActivated);
                Utils.netUtils.SyncJetpack(GetComponent<NetworkIdentity>().netId, data);

                // Remember sent state
                lastSyncedJetpack = data;
                lastJetpackSyncTime = Time.fixedTime;
                hasSyncedJetpack = true;
            }
        }

        [Command]
        public void CmdSyncJetpack(JetpackSyncData _data)
        {
            // Sync on all clients
            RpcSyncJetpack(_data);
        }

        [ClientRpc]
        private void RpcSyncJetpack(JetpackSyncData _data)
        {
            // Don't sync if this jetpack belongs to this local player (pretending authority)
            if (character == Utils.localPlayerBody) return;

            // Sync with incoming data
            fuelUsed = _data.fuelUsed;
            jetActivated = _data.jetActivated;
        }

        protected void UpdateIgnite()
        {
            // Only the server should apply DoTs
            if (!Utils.hosting || itemCount <= 0 || character == null || character.inventory == null || character.healthComponent == null || !character.healthComponent.alive)
            {
                igniteStopwatch = 0.0f;
                hasDoneInitialIgnite = false;
                return;
            }

            // Quality must be enabled for this extra behaviour
            if (!Utils.qualityEnabled)
            {
                igniteStopwatch = 0.0f;
                hasDoneInitialIgnite = false;
                return;
            }

            // Check if jetpack item is valid and supports quality
            if (Faithful.tJetpack == null || !Faithful.tJetpack.MainItem.supportsQuality)
            {
                igniteStopwatch = 0.0f;
                hasDoneInitialIgnite = false;
                return;
            }

            // Get quality counts
            QualityCounts qualityCounts = QualityCompat.GetItemCountsEffective(character.inventory, Faithful.tJetpack.MainItem);
            if (qualityCounts.Total <= 0)
            {
                igniteStopwatch = 0.0f;
                hasDoneInitialIgnite = false;
                return;
            }

            // Calculate frequency
            float igniteFrequency = CalculateIgniteFrequency(qualityCounts);
            if (igniteFrequency <= 0.0f)
            {
                igniteStopwatch = 0.0f;
                hasDoneInitialIgnite = false;
                return;
            }

            // Convert ignites-per-second into interval
            float igniteInterval = 1.0f / igniteFrequency;

            // If not actively thrusting, preserve partial progress but do not advance it
            if (!jetActivated)
            {
                igniteStopwatch = Mathf.Min(igniteStopwatch, igniteInterval * 0.95f);
                return;
            }

            // Very first valid thrust gets an immediate ignite
            if (!hasDoneInitialIgnite)
            {
                IgniteEnemiesBelow(CalculateIgniteDamage(qualityCounts));

                hasDoneInitialIgnite = true;
                igniteStopwatch = 0.0f;

                return;
            }

            // Tick ignite interval
            igniteStopwatch += Time.fixedDeltaTime;

            // Use while loop to handle cases where framerate is too low and multiple ignites should occur in the same frame
            while (igniteStopwatch >= igniteInterval)
            {
                igniteStopwatch -= igniteInterval;
                IgniteEnemiesBelow(CalculateIgniteDamage(qualityCounts));
            }
        }

        protected float CalculateIgniteDamage(QualityCounts _qualityCounts)
        {
            TJetpack jetpackItem = Faithful.tJetpack;
            if (jetpackItem == null) return 0.0f;

            // Damage is cumulative across qualities
            return Utils.CalculateStackingValue(_qualityCounts.UNCOMMON, jetpackItem.DamageQualityValues.UNCOMMON, jetpackItem.DamageStackingQualityValues.UNCOMMON)
                + Utils.CalculateStackingValue(_qualityCounts.RARE, jetpackItem.DamageQualityValues.RARE, jetpackItem.DamageStackingQualityValues.RARE)
                + Utils.CalculateStackingValue(_qualityCounts.EPIC, jetpackItem.DamageQualityValues.EPIC, jetpackItem.DamageStackingQualityValues.EPIC)
                + Utils.CalculateStackingValue(_qualityCounts.LEGENDARY, jetpackItem.DamageQualityValues.LEGENDARY, jetpackItem.DamageStackingQualityValues.LEGENDARY);
        }

        protected float CalculateIgniteFrequency(QualityCounts _qualityCounts)
        {
            TJetpack jetpackItem = Faithful.tJetpack;
            if (jetpackItem == null) return 0.0f;

            // Calculate each quality independently
            float uncommonFrequency = CalculateIgniteFrequencyForQuality(_qualityCounts.UNCOMMON, jetpackItem.FrequencyQualityValues.UNCOMMON, jetpackItem.FrequencyStackingQualityValues.UNCOMMON, jetpackItem.FrequencyMaxQualityValues.UNCOMMON);
            float rareFrequency = CalculateIgniteFrequencyForQuality(_qualityCounts.RARE, jetpackItem.FrequencyQualityValues.RARE, jetpackItem.FrequencyStackingQualityValues.RARE, jetpackItem.FrequencyMaxQualityValues.RARE);
            float epicFrequency = CalculateIgniteFrequencyForQuality(_qualityCounts.EPIC, jetpackItem.FrequencyQualityValues.EPIC, jetpackItem.FrequencyStackingQualityValues.EPIC, jetpackItem.FrequencyMaxQualityValues.EPIC);
            float legendaryFrequency = CalculateIgniteFrequencyForQuality(_qualityCounts.LEGENDARY, jetpackItem.FrequencyQualityValues.LEGENDARY, jetpackItem.FrequencyStackingQualityValues.LEGENDARY, jetpackItem.FrequencyMaxQualityValues.LEGENDARY);

            // Frequencies are not cumulative - use the highest calculated value without allocating an array
            return Mathf.Max(Mathf.Max(uncommonFrequency, rareFrequency), Mathf.Max(epicFrequency, legendaryFrequency));
        }

        protected float CalculateIgniteFrequencyForQuality(int _count, float _baseFrequency, float _stackingFrequency, int _maxFrequency)
        {
            // Calculate frequency for this quality
            float frequency = Utils.CalculateStackingValue(_count, _baseFrequency, _stackingFrequency);

            // Clamp to this quality's max frequency
            return Mathf.Min(frequency, _maxFrequency);
        }

        protected void IgniteEnemiesBelow(float _damageCoefficient)
        {
            // Validate damage
            if (_damageCoefficient <= 0.0f) return;

            // Need team information
            if (character.teamComponent == null) return;

            // Get search origin
            Vector3 origin = character.corePosition;

            // Search below the jetpack user
            BullseyeSearch search = new()
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(character.teamComponent.teamIndex),
                filterByLoS = false,
                searchOrigin = origin,
                searchDirection = Vector3.down,
                sortMode = BullseyeSearch.SortMode.Distance,
                maxDistanceFilter = igniteRange,
                maxAngleFilter = igniteAngle
            };

            search.RefreshCandidates();
            search.FilterOutGameObject(character.gameObject);

            // Keep separate pooled results and victim tracking for each flame attack
            using (HG.CollectionPool<HurtBox, List<HurtBox>>.RentCollection(out List<HurtBox> hurtBoxes))
            using (HG.CollectionPool<HealthComponent, HashSet<HealthComponent>>.RentCollection(out HashSet<HealthComponent> igniteVictims))
            {
                search.GetResults(hurtBoxes);

                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    // Validate target
                    if (hurtBox == null || hurtBox.healthComponent == null) continue;

                    HealthComponent healthComponent = hurtBox.healthComponent;
                    CharacterBody victimBody = healthComponent.body;

                    // Check if valid victim
                    if (victimBody == null || victimBody == character || !healthComponent.alive) continue;

                    // Keep the effect strictly below the jetpack user
                    if (victimBody.corePosition.y >= origin.y) continue;

                    // Only ignite each victim once per tick
                    if (!igniteVictims.Add(healthComponent)) continue;

                    // Jetpack flame attack
                    float directDamageCoefficient = _damageCoefficient * directDamageFraction;
                    float burnDamageCoefficient = _damageCoefficient * burnDamageFraction;
                    HitEnemyWithJetpackFlame(hurtBox, victimBody, directDamageCoefficient);
                    ApplyJetpackBurn(victimBody, burnDamageCoefficient);
                }
            }
        }

        protected void HitEnemyWithJetpackFlame(HurtBox _hurtBox, CharacterBody _victimBody, float _damageCoefficient)
        {
            // Host only behaviour
            if (!Utils.hosting) return;

            // Validate input
            if (_hurtBox == null || _victimBody == null || _victimBody.healthComponent == null) return;
            if (_damageCoefficient <= 0.0f) return;

            // Create damage info
            DamageInfo damageInfo = new()
            {
                attacker = character.gameObject,
                inflictor = gameObject,
                damage = character.damage * _damageCoefficient,
                damageColorIndex = DamageColorIndex.Item,
                damageType = DamageType.Generic,
                crit = character.RollCrit(),
                force = Vector3.zero,
                position = _hurtBox.transform.position,
                procCoefficient = 1.0f
            };

            // Apply damage
            _victimBody.healthComponent.TakeDamage(damageInfo);
            GlobalEventManager.instance.OnHitEnemy(damageInfo, _victimBody.gameObject);
            GlobalEventManager.instance.OnHitAll(damageInfo, _victimBody.gameObject);
        }

        protected void ApplyJetpackBurn(CharacterBody _victimBody, float _burnDamageCoefficient)
        {
            // Host only behaviour
            if (!Utils.hosting) return;

            // Validate input
            if (_victimBody == null || _victimBody.healthComponent == null) return;
            if (_burnDamageCoefficient <= 0.0f) return;
            if (character == null || character.inventory == null) return;

            DotController.DotIndex dotIndex = DotController.DotIndex.Burn;
            DotController.DotDef dotDef = DotController.dotDefs[(int)dotIndex];

            // Desired total burn damage
            float desiredBurnDamage = character.damage * _burnDamageCoefficient;

            // Calculate burn duration
            float desiredBurnDuration = baseIgniteDuration;
            if (_burnDamageCoefficient > 1.0f) desiredBurnDuration *= 1.0f + (_burnDamageCoefficient - 1.0f) * igniteDurationIncreaseFraction;

            // Vanilla Burn tick damage at damageMultiplier = 1
            float baseTickDamage = Mathf.Min(dotDef.damageCoefficient * character.damage, _victimBody.healthComponent.fullCombinedHealth * 0.01f);
            if (baseTickDamage <= 0.0f) return;

            // Number of ticks over the fixed duration
            float tickCount = desiredBurnDuration / dotDef.interval;
            if (tickCount <= 0.0f) return;

            // Convert desired total damage into the multiplier Burn actually expects
            float damageMultiplier = desiredBurnDamage / (baseTickDamage * tickCount);

            // Create DoT info
            InflictDotInfo dotInfo = new()
            {
                attackerObject = character.gameObject,
                victimObject = _victimBody.gameObject,
                dotIndex = dotIndex,
                duration = desiredBurnDuration,
                damageMultiplier = damageMultiplier,
                hitHurtBox = _victimBody.mainHurtBox
            };

            // Upgrade Burn if needed
            StrengthenBurnUtils.CheckDotForUpgrade(character.inventory, ref dotInfo);

            // Inflict DoT
            DotController.InflictDot(ref dotInfo);
        }

        public int GetFuelDisplayValue(int _fallback)
        {
            if (!fuelBuffUsesDisplayValue || character == null || itemCount <= 0) return _fallback;
            return Mathf.Clamp(Mathf.CeilToInt(fuelRemainingPerc * 100.0f), 0, 100);
        }

        protected float fuelCapacity
        {
            get
            {
                // Calculate fuel capacity
                return Utils.CalculateStackingValue(itemCount, baseFuel, fuelPerStack);
            }
        }

        protected float fuelRemaining
        {
            get
            {
                // Calculate fuel remaining based on fuel capacity and fuel used
                return fuelCapacity - fuelUsed;
            }
        }

        protected float fuelRemainingPerc
        {
            get
            {
                // Calculate remaining fuel percentage
                return 1.0f - fuelUsed / fuelCapacity;
            }
        }

        protected float refuelDuration
        {
            get
            {
                // Calculate refuel duration
                return baseRefuelDuration * Mathf.Max(0.1f, 1.0f - Mathf.Log(itemCount, Mathf.Pow(2.0f, 1.0f / refuelDurationReductionPerStack)));
            }
        }

        protected float refuelRate
        {
            get
            {
                // Refuel rate is calculated based on fuel capacity and refueling duration
                return fuelCapacity / refuelDuration;
            }
        }

        protected float timeSinceLastJet
        {
            get
            {
                // Get time since last frame of jet
                return Time.time - lastJetTime;
            }
        }

        protected float dynamicAccelerationDifference
        {
            get
            {
                return fallingAcceleration - risingAcceleration;
            }
        }

        protected bool canBeActivated
        {
            get
            {
                // Return if the jetpack can be activated
                return character.characterMotor.velocity.y < 0.0f && fuelRemaining > 0.0f;
            }
        }

        protected float buffPerc
        {
            get
            {
                // Get buff amount and max buff amount
                int buffAmount = itemCount - 1;
                int maxBuffAmount = buffedItemCount - 1;

                // Return buff percentage
                return Mathf.Min((float)buffAmount / maxBuffAmount, 1.0f);
            }
        }

        protected float maxVelocity
        {
            get
            {
                

                // Return and modify max velocity based on buff
                return (baseMaxVelocity + maxVelocityBuff * buffPerc) * jumpPowerModifier * maxVelocityMultiplier;
            }
        }

        protected float risingAcceleration
        {
            get
            {


                // Return and modify rising acceleration based on buff
                return baseRisingAcceleration + risingAccelerationBuff * buffPerc;
            }
        }

        protected float jumpPowerModifier
        {
            get
            {
                // Return modifier for max velocity and acceleration based on jump power
                return Mathf.Sqrt(character.jumpPower / character.baseJumpPower);
            }
        }
    }

    internal class TJetpackJetFlare : MonoBehaviour
    {
        // Store reference to renderer
        Renderer renderer;

        private void Awake()
        {
            // Get renderer
            renderer = GetComponent<Renderer>();

            // Set renderer material
            renderer.material = Assets.mageJetMaterial;
        }

        private void FixedUpdate()
        {
            // Check if incorrect material is currently applied
            if (!renderer.material.name.Contains("mage"))
            {
                // Reapply material
                renderer.material = Assets.mageJetMaterial;
            }
        }
    }

    internal struct JetpackSyncData
    {
        // Syncs fuel used and jet activated state
        public float fuelUsed;
        public bool jetActivated;

        public JetpackSyncData(float _fuelUsed, bool _jetActivated)
        {
            // Assign values
            fuelUsed = _fuelUsed;
            jetActivated = _jetActivated;
        }
    }
}
