using EntityStates;
using RoR2;
using UnityEngine;

namespace Faithful.Skills.Technician
{
    public class ArcPrimary : BaseSkillState
    {
        GameObject flamethrowerEffectPrefab = Assets.FetchAsset<GameObject>("RoR2/Base/Mage/MageFlamethrowerEffect.prefab");

        float maxDistance = 45.0f;

        float baseEntryDuration = 0.8f;

        float baseFlamethrowerDuration = 2f;

        float procCoefficientPerTick = 0.625f;

        float tickFrequency = 8.0f;

        string startAttackSoundString;

        string endAttackSoundString;

        float tickDamageCoefficient = 0.5f;

        float flamethrowerStopwatch;

        float stopwatch;

        float entryDuration = 0.8f;

        float flamethrowerDuration;

        bool hasBegunFlamethrower;

        ChildLocator childLocator;

        Transform leftFlamethrowerTransform;

        Transform rightFlamethrowerTransform;

        static int PrepFlamethrowerStateHash = Animator.StringToHash("PrepFlamethrower");

        int ExitFlamethrowerStateHash = Animator.StringToHash("ExitFlamethrower");

        int FlamethrowerParamHash = Animator.StringToHash("playbackRate");

        int FlamethrowerStateHash = Animator.StringToHash("Flamethrower");

        public override void OnEnter()
        {
            base.OnEnter();
            stopwatch = 0f;
            entryDuration = baseEntryDuration / attackSpeedStat;
            flamethrowerDuration = baseFlamethrowerDuration;
            Transform modelTransform = GetModelTransform();
            if (characterBody)
            {
                characterBody.SetAimTimer(entryDuration + flamethrowerDuration + 1f);
            }
            if (modelTransform)
            {
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }
            int num = Mathf.CeilToInt(flamethrowerDuration * tickFrequency);
            PlayAnimation("Gesture, Additive", PrepFlamethrowerStateHash, FlamethrowerParamHash, entryDuration);
        }

        public override void OnExit()
        {
            Util.PlaySound(endAttackSoundString, gameObject);
            PlayCrossfade("Gesture, Additive", ExitFlamethrowerStateHash, 0.1f);
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
                bulletAttack.damageType = DamageType.Generic;
                bulletAttack.allowTrajectoryAimAssist = false;
                bulletAttack.damageType.damageSource = DamageSource.Primary;
                bulletAttack.Fire();
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
                PlayAnimation("Gesture, Additive", FlamethrowerStateHash, FlamethrowerParamHash, flamethrowerDuration);
                if (childLocator)
                {
                    Transform transform = childLocator.FindChild("HandL");
                    Transform transform2 = childLocator.FindChild("HandR");
                    if (transform)
                    {
                        leftFlamethrowerTransform = Object.Instantiate(flamethrowerEffectPrefab, transform).transform;
                    }
                    if (transform2)
                    {
                        rightFlamethrowerTransform = Object.Instantiate(flamethrowerEffectPrefab, transform2).transform;
                    }
                    if (leftFlamethrowerTransform)
                    {
                        leftFlamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
                    }
                    if (rightFlamethrowerTransform)
                    {
                        rightFlamethrowerTransform.GetComponent<ScaleParticleSystemDuration>().newDuration = flamethrowerDuration;
                    }
                }
                FireGauntlet("MuzzleCenter");
            }
            if (hasBegunFlamethrower)
            {
                flamethrowerStopwatch += Time.deltaTime;
                float num = 1f / tickFrequency / attackSpeedStat;
                if (flamethrowerStopwatch > num)
                {
                    flamethrowerStopwatch -= num;
                    FireGauntlet("MuzzleCenter");
                }
                UpdateFlamethrowerEffect();
            }
            if (stopwatch >= flamethrowerDuration + entryDuration && isAuthority)
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
