﻿using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class FaithfulTJetpackBehaviour : NetworkBehaviour, ICharacterBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

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

        // Store if this jetpack belongs to Artificer
        protected bool artificer = false;

        // Store if this jetpack has a display
        protected bool hasDisplay = false;

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

            // Get character model
            Transform characterModel = _characterBody.modelLocator.modelTransform;

            // Get jetpack game object
            jetpack = Utils.FindChildByName(characterModel, "4T0NJetpackDisplayMesh(Clone)");

            // Check for jetpack
            if (jetpack == null)
            {
                // Jetpack does not belong to Artificer
                artificer = false;

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

            // Add jet flate behaviour
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

            // Check if Artificer
            if (characterModel.name == "mdlMage")
            {
                // Jetpack belongs to Artificer
                artificer = true;

                // Disable additional boosters
                additionalBoosters.transform.localScale = Vector3.zero;
            }

            // Otherwise say jetpack does not belong to artificer
            else
            {
                // Jetpack does not belong to Artificer
                artificer = false;
            }
        }

        private void OnDestroy()
        {
            // Unregister with utils
            Utils.UnregisterCharacterBehaviour(this);
        }

        public void FetchSettings()
        {
            // Get jetpack item
            Item jetpackItem = Items.GetItem("4T0N_JETPACK");

            // Update stats
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

            // Inject hooks
            On.EntityStates.GenericCharacterMain.ProcessJump += OnProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate += OnFixedUpdate;
            //On.EntityStates.Mage.JetpackOn.OnEnter += OnArtificerJetpackOnEnter;
            //On.EntityStates.Mage.JetpackOn.OnExit += OnArtificerJetpackOnExit;
            On.EntityStates.Mage.JetpackOn.FixedUpdate += OnArtificerJetpackFixedUpdate;
            On.EntityStates.Mage.MageCharacterMain.ProcessJump += OnArtificerProcessJump;
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

            // Remove hooks
            On.EntityStates.GenericCharacterMain.ProcessJump -= OnProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate -= OnFixedUpdate;
            //On.EntityStates.Mage.JetpackOn.OnEnter -= OnArtificerJetpackOnEnter;
            //On.EntityStates.Mage.JetpackOn.OnExit -= OnArtificerJetpackOnExit;
            On.EntityStates.Mage.JetpackOn.FixedUpdate -= OnArtificerJetpackFixedUpdate;
            On.EntityStates.Mage.MageCharacterMain.ProcessJump -= OnArtificerProcessJump;
        }

        protected void OnFixedUpdate(On.EntityStates.GenericCharacterMain.orig_FixedUpdate orig, GenericCharacterMain self)
        {
            // Check if valid and correct character
            if (self == null || self.characterBody != character)
            {
                orig(self); // Otherwise run normal processes
                return;
            }

            // Ensure jetpack is initialised
            Init(self.characterBody);

            // Update visuals
            UpdateVisuals();

            // Sync jetpack
            SyncJetpack();

            orig(self); // Run normal processes
        }

        protected void OnArtificerProcessJump(On.EntityStates.Mage.MageCharacterMain.orig_ProcessJump orig, EntityStates.Mage.MageCharacterMain self)
        {
            // Check if Artificer is this character
            if (self == null || self.characterBody != character)
            {
                orig(self); // Run normal processes
                return;
            }

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
            }
            else
            {
                orig(self); // Run normal processes
            }
        }

        protected void OnProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            // Check if valid and correct character
            if (self == null || self.characterBody != character || !self.hasCharacterMotor || !self.hasInputBank || !self.isAuthority)
            {
                orig(self); // Otherwise run normal processes
                return;
            }

            // Do jetpack behaviour
            JetpackBehaviour(self);

            // Check if not to run normal processes
            if (jetActivated || (character.inputBank.jump.down && canBeActivated))
            {
                return;
            }

            orig(self); // Otherwise run normal processes
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

        /*protected void OnArtificerJetpackOnEnter(On.EntityStates.Mage.JetpackOn.orig_OnEnter orig, EntityStates.Mage.JetpackOn self)
        {
            // Check if Artificer is this character
            if (self == null || self.characterBody != character)
            {
                orig(self); // Run normal processes
                return;
            }

            // Attempt to get Jet On effect
            Transform jetOnEffect = self.FindModelChild("JetOn");

            // Check for jetpack object
            if (jetOnEffect)
            {
                // Kidnap Artificer jetpack
                artificerJetpack = jetOnEffect.gameObject;

                // Enable effect
                artificerJetpack.SetActive(true);
            }
        }

        protected void OnArtificerJetpackOnExit(On.EntityStates.Mage.JetpackOn.orig_OnExit orig, EntityStates.Mage.JetpackOn self)
        {
            // Check if Artificer is this character
            if (self == null || self.characterBody != character)
            {
                orig(self); // Run normal processes
                return;
            }

            // Check for jetpack object
            if (artificerJetpack)
            {
                // Only disable effect if 4-T0N jetpack isn't active
                if (!jetActivated)
                {
                    // Disable effect
                    artificerJetpack.SetActive(false);
                }
            }
        }*/

        protected void OnArtificerJetpackFixedUpdate(On.EntityStates.Mage.JetpackOn.orig_FixedUpdate orig, EntityStates.Mage.JetpackOn self)
        {
            // Check if Artificer is this character
            if (self == null || self.characterBody != character)
            {
                orig(self); // Run normal processes
                return;
            }

            // Check if doing TJetpack behaviour
            if (jetActivated)
            {
                // Skip Artificer jetpack behaviour to prioritise 4-T0N Jetpack
                return;
            }

            // Otherwise run normal processes
            orig(self);
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
                // Send sync request to net utils
                Utils.netUtils.SyncJetpack(GetComponent<NetworkIdentity>().netId, new JetpackSyncData(fuelUsed, jetActivated));
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

        protected float fuelCapacity
        {
            get
            {
                // Calculate fuel capacity
                return baseFuel + fuelPerStack * (itemCount - 1);
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
