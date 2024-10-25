using EntityStates;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulTJetpackBehaviour
    {
        // Store reference to Character Body and Character Inventory
        public CharacterBody character;

        // Store reference to Artificer jetpack (OFTEN NULL)
        protected GameObject artificerJetpack;

        // Store if active
        protected bool active = false;

        // Store item count
        protected int itemCount = 0;

        // Store jetting state, last jet frame time and jet time
        //protected bool jetting = false;
        //protected float lastJetTime;
        //protected float fuelUsed = 0.0f;

        // Store time when deactivated
        //protected float deactivatedTime;

        // Jetpack stats
        protected float maxVelocity = 30.0f;
        protected float risingAcceleration = 36.0f;
        protected float fallingAcceleration = 100.0f;
        protected float baseFuel = 2.0f;
        protected float fuelPerStack = 1.0f;
        protected float baseRefuelDuration = 12.0f;
        protected float refuelWaitRatio = 0.4f;
        protected float minimumFuelToActivate = 0.5f;

        // Store current state of jetpack
        protected bool grounded = false;
        protected bool jetActivated = false;
        protected bool firstJet = true;
        protected bool refueling = false;
        protected float fuelUsed = 0.0f;
        protected float lastJetTime;

        public FaithfulTJetpackBehaviour(CharacterBody _character)
        {
            // Assign character
            character = _character;
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
            //On.EntityStates.Mage.JetpackOn.OnEnter -= OnArtificerJetpackOnEnter;
            //On.EntityStates.Mage.JetpackOn.OnExit -= OnArtificerJetpackOnExit;
            On.EntityStates.Mage.JetpackOn.FixedUpdate -= OnArtificerJetpackFixedUpdate;
            On.EntityStates.Mage.MageCharacterMain.ProcessJump -= OnArtificerProcessJump;
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

        protected void OnArtificerJetpackOnEnter(On.EntityStates.Mage.JetpackOn.orig_OnEnter orig, EntityStates.Mage.JetpackOn self)
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
        }

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
                // Check if should start refueling
                if (timeSinceLastJet >= refuelWaitDuration)
                {
                    // Start refueling
                    refueling = true;
                }
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
            // Activate jetpack
            jetActivated = true;

            // Indicate that jet is recently activated
            firstJet = true;
        }

        protected void DeactivateJet()
        {
            // Deactivate jetpack
            jetActivated = false;
        }

        protected void Jet()
        {
            // Get current y velocity
            float yVel = character.characterMotor.velocity.y;

            // Calculate force
            float force = yVel > 0.0f ? risingAcceleration : risingAcceleration + Mathf.Clamp01(-yVel / 30.0f) * dynamicAccelerationDifference;

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

        protected float refuelDuration
        {
            get
            {
                // Calculate refuel duration
                return baseRefuelDuration / (1.0f + Mathf.Log(itemCount, 16));
            }
        }

        protected float refuelingDuration
        {
            get
            {
                // Based on refuel duration and wait ratio
                return refuelDuration * (1.0f - refuelWaitRatio);
            }
        }

        protected float refuelWaitDuration
        {
            get
            {
                // Based on refuel duration and wait ratio
                return refuelDuration * refuelWaitRatio;
            }
        }

        protected float refuelRate
        {
            get
            {
                // Refuel rate is calculated based on fuel capacity and refueling duration
                return fuelCapacity / refuelingDuration;
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
                return character.characterMotor.velocity.y < 0.0f && fuelRemaining > minimumFuelToActivate;
            }
        }
    }
}
