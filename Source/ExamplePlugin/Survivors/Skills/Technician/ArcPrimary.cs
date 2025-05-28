using EntityStates;
using RoR2;
using UnityEngine;

namespace Faithful.Skills.Technician
{
    public class ArcPrimary : BaseSkillState
    {
        GameObject arcEffectPrefab = Assets.technicianArcPrefab;

        float maxDistance = 45.0f;

        float baseEntryDuration = 0.8f;

        float procCoefficientPerTick = 0.625f;

        float tickFrequency = 8.0f;

        string startAttackSoundString;

        string endAttackSoundString;

        float tickDamageCoefficient = 0.5f;

        float flamethrowerStopwatch;

        float stopwatch;

        float entryDuration = 0.6f;

        bool hasBegunFlamethrower;

        ChildLocator childLocator;

        Transform leftFlamethrowerTransform;

        Transform rightFlamethrowerTransform;

        TechnicianTracker tracker;

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            entryDuration = baseEntryDuration / attackSpeedStat;
            Transform modelTransform = GetModelTransform();
            if (characterBody)
            {
                // Fetch tracker
                tracker = characterBody.GetComponent<TechnicianTracker>();

                characterBody.SetAimTimer(1f);
            }
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }
            PlayAnimation("BothArms, Override", "ArcStart", "ArcStart.playbackRate", entryDuration);
        }

        public override void OnExit()
        {
            Util.PlaySound(endAttackSoundString, gameObject);
            PlayAnimation("BothArms, Override", "ArcEnd", "ArcEnd.playbackRate", 0.4f / attackSpeedStat);
            if (leftFlamethrowerTransform)
            {
                Destroy(leftFlamethrowerTransform.gameObject);
            }
            if (rightFlamethrowerTransform)
            {
                Destroy(rightFlamethrowerTransform.gameObject);
            }
            base.OnExit();
        }

        private void FireGauntlet(string muzzleString)
        {
            Ray aimRay = GetAimRay();
            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0.0f;
                bulletAttack.damage = tickDamageCoefficient * damageStat;
                bulletAttack.force = 0.0f;
                bulletAttack.muzzleName = muzzleString;
                //bulletAttack.hitEffectPrefab = impactEffectPrefab;
                bulletAttack.isCrit = Util.CheckRoll(critStat, characterBody.master);
                bulletAttack.radius = 1.0f;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.stopperMask = LayerIndex.world.mask;
                bulletAttack.procCoefficient = procCoefficientPerTick;
                bulletAttack.maxDistance = maxDistance;
                bulletAttack.smartCollision = true;
                bulletAttack.damageType = DamageType.SlowOnHit;
                bulletAttack.allowTrajectoryAimAssist = false;
                bulletAttack.damageType.damageSource = DamageSource.Primary;
                bulletAttack.Fire();
            }

            if (characterBody)
            {
                characterBody.SetAimTimer(1f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += GetDeltaTime();
            if (stopwatch >= entryDuration && !hasBegunFlamethrower)
            {
                hasBegunFlamethrower = true;
                Util.PlaySound(startAttackSoundString, gameObject);
                PlayAnimation("BothArms, Override", "ArcLoop", "ArcLoop.playbackRate", 10.0f);
                if (childLocator)
                {
                    Transform transform = childLocator.FindChild("ArcLeft");
                    Transform transform2 = childLocator.FindChild("ArcRight");
                    if (transform)
                    {
                        leftFlamethrowerTransform = Object.Instantiate(arcEffectPrefab, transform).transform;
                    }
                    if (transform2)
                    {
                        rightFlamethrowerTransform = Object.Instantiate(arcEffectPrefab, transform2).transform;
                    }
                }
                FireGauntlet("MuzzleCenter");
            }
            if (hasBegunFlamethrower)
            {
                PlayAnimation("BothArms, Override", "ArcLoop", "ArcLoop.playbackRate", 10.0f);
                flamethrowerStopwatch += Time.deltaTime;
                float num = 1f / tickFrequency / attackSpeedStat;
                if (flamethrowerStopwatch > num)
                {
                    flamethrowerStopwatch -= num;
                    FireGauntlet("MuzzleCenter");
                }
                UpdateFlamethrowerEffect();
            }
            if (isAuthority && (!IsKeyDownAuthority() || characterBody.isSprinting || characterBody.allSkillsDisabled || tracker == null || tracker.GetTrackingTarget() == null))
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void UpdateFlamethrowerEffect()
        {
            Ray aimRay = GetAimRay();
            Vector3 direction = aimRay.direction;
            Vector3 direction2 = aimRay.direction;
            if (leftFlamethrowerTransform)
            {
                leftFlamethrowerTransform.forward = direction;
            }
            if (rightFlamethrowerTransform)
            {
                rightFlamethrowerTransform.forward = direction2;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
