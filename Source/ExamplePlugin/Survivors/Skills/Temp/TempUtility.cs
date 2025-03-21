using EntityStates;
using EntityStates.Huntress;
using RoR2;
using UnityEngine;

namespace Faithful.Skills.Temp
{
    public class TempUtility : BaseState
    {
        // Token: 0x06001214 RID: 4628 RVA: 0x0004EB24 File Offset: 0x0004CD24
        public override void OnEnter()
        {
            base.OnEnter();

            // Play shift sound
            Util.PlaySound("Play_huntress_shift_start", gameObject);

            // Get model transform
            modelTransform = GetModelTransform();
            if (modelTransform)
            {
                // Get character model and hurtbox group
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }

            // Check for character model
            if (characterModel)
            {
                // Add invisibility to character model
                characterModel.invisibilityCount++;
            }

            // Check for hurtbox group
            if (hurtboxGroup)
            {
                // Add deactivator counter to hurtbox group for temporary immunity
                HurtBoxGroup hurtBoxGroup = hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            // Get blink direction
            blinkVector = GetBlinkVector();

            // Create blink effect
            CreateBlinkEffect(Util.GetCorePosition(gameObject));
        }

        protected virtual Vector3 GetBlinkVector()
        {
            return inputBank.aimDirection;
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            // Create effect and spawn it
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(blinkVector);
            effectData.origin = origin;
            EffectManager.SpawnEffect(BlinkState.blinkPrefab, effectData, false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Update stopwatch
            stopwatch += GetDeltaTime();

            // Check for character motor and direction
            if (characterMotor && characterDirection)
            {
                // Zero velocity and blink forward
                characterMotor.velocity = Vector3.zero;
                characterMotor.rootMotion += blinkVector * (moveSpeedStat * speedCoefficient * GetDeltaTime());
            }

            // Check if skill timer has expired
            if (stopwatch >= duration && isAuthority)
            {
                // End skill
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            if (!outer.destroying)
            {
                // Play sound effect
                Util.PlaySound("Play_huntress_shift_end", base.gameObject);

                // Create blink effect
                CreateBlinkEffect(Util.GetCorePosition(base.gameObject));

                // Get model transform
                modelTransform = GetModelTransform();

                // Check for model transform
                if (modelTransform)
                {
                    // Create flash overlay effects
                    TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlayInstance.duration = 0.6f;
                    temporaryOverlayInstance.animateShaderAlpha = true;
                    temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlayInstance.destroyComponentOnEnd = true;
                    temporaryOverlayInstance.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
                    temporaryOverlayInstance.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
                    TemporaryOverlayInstance temporaryOverlayInstance2 = TemporaryOverlayManager.AddOverlay(modelTransform.gameObject);
                    temporaryOverlayInstance2.duration = 0.7f;
                    temporaryOverlayInstance2.animateShaderAlpha = true;
                    temporaryOverlayInstance2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlayInstance2.destroyComponentOnEnd = true;
                    temporaryOverlayInstance2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
                    temporaryOverlayInstance2.AddToCharacterModel(modelTransform.GetComponent<CharacterModel>());
                }
            }

            // Check for character model
            if (characterModel)
            {
                // Set no longer invisible
                characterModel.invisibilityCount--;
            }

            // Check for hurtbox group
            if (hurtboxGroup)
            {
                // Remove invinsibility
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            // Check for character motor
            if (characterMotor)
            {
                // Re-enable air control
                characterMotor.disableAirControlUntilCollision = false;
            }

            base.OnExit();
        }

        // Returns the priority needed to interrupt the skill while active
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }

        private Transform modelTransform;

        private float stopwatch;

        private Vector3 blinkVector = Vector3.zero;

        [SerializeField]
        public float duration = 0.2f;

        [SerializeField]
        public float speedCoefficient = 25f;

        private CharacterModel characterModel;

        private HurtBoxGroup hurtboxGroup;
    }
}
