using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulTJetpackBehaviour
    {
        // Store reference to Character Body
        protected CharacterBody character;

        // Store state
        protected bool active = false;

        // Store item count
        protected int itemCount;

        // Store jetting state, last jet frame time and jet time
        protected bool jetting = false;
        protected float lastJetTime;
        protected float fuelUsed = 0.0f;

        // Store time when deactivated
        protected float deactivatedTime;

        // Jetpack stats
        protected float maxVelocity = 32.0f;
        protected float acceleration = 60.0f;
        protected float baseDuration = 3.0f;
        protected float baseRechargeDuration = 10.0f;

        public FaithfulTJetpackBehaviour(CharacterBody _character)
        {
            // Assign character
            character = _character;
        }

        ~FaithfulTJetpackBehaviour()
        {
            // Deactivate
            Deactivate();
        }

        public void Activate(int _itemCount)
        {
            // Update item count
            itemCount = _itemCount;

            // Return if already active and player has item
            if (active || _itemCount == 0)
            {
                return;
            }

            // Test if used all fuel
            if (fuelUsed >= duration)
            {
                // Test if recharge duration complete
                if (Time.time - deactivatedTime > rechargeDuration)
                {
                    // Reset fuel used to 0
                    fuelUsed = 0.0f;
                }
                else
                {
                    return;
                }
            }

            // Set active
            active = true;

            // Add hooks
            On.EntityStates.GenericCharacterMain.ProcessJump += OnProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate += OnFixedUpdate;
        }

        public void Deactivate()
        {
            // Return if already not active
            if (!active)
            {
                return;
            }

            // Set active
            active = false;

            // Reset jetting state
            jetting = false;

            // Set deactivated time
            deactivatedTime = Time.time;

            // Remove hooks
            On.EntityStates.GenericCharacterMain.ProcessJump -= OnProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate -= OnFixedUpdate;
        }

        protected void OnProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, GenericCharacterMain self)
        {
            orig(self); // Run normal processes first

            // Check if valid and correct character
            if (self == null || self.characterBody != character || !self.hasCharacterMotor || !self.hasInputBank || !self.isAuthority)
            {
                return;
            }

            // Check if jump is being held
            if (!self.inputBank.jump.down)
            {
                // Reset jetting state
                jetting = false;

                return;
            }

            // Check if jet is depleted
            if (fuelUsed >= duration)
            {
                // Deactivate
                Deactivate();
                return;
            }

            // Is already jetting?
            if (jetting)
            {
                // Increase jet time
                fuelUsed = Mathf.Clamp(fuelUsed + (Time.time - lastJetTime), 0.0f, duration);
            }
            else
            {
                // Update jetting state
                jetting = true;
            }

            // Update last jet time
            lastJetTime = Time.time;

            // Get y velocity
            float yVel = self.characterMotor.velocity.y;

            // Check if y velocity is below limit
            if (yVel < maxVelocity)
            {
                // Modify y velocity
                yVel = Mathf.MoveTowards(yVel, maxVelocity, acceleration * Time.fixedDeltaTime);

                // Apply new y velocity
                self.characterMotor.velocity = new Vector3(self.characterMotor.velocity.x, yVel, self.characterMotor.velocity.z);
            }
        }

        protected void OnFixedUpdate(On.EntityStates.GenericCharacterMain.orig_FixedUpdate orig, GenericCharacterMain self)
        {
            orig(self); // Run normal processes first

            // Check if valid and correct character
            if (self == null || self.characterBody != character || !self.hasCharacterMotor || !self.hasInputBank || !self.isAuthority)
            {
                return;
            }

            // Check if landed
            if (self.isGrounded)
            {
                // Deactivate
                Deactivate();
                return;
            }
        }

        protected float duration
        {
            get
            {
                // Duration is calculated by base duration multiplied by item count
                return baseDuration * itemCount;
            }
        }

        protected float rechargeDuration
        {
            get
            {
                // Calculate recharge duration
                return baseRechargeDuration / (1.0f + Mathf.Log(itemCount, 16));
            }
        }
    }
}
