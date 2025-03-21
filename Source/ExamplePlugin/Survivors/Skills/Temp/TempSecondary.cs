using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using UnityEngine;

namespace Faithful.Skills.Temp
{
    public class TempSecondary : BaseSkillState
    {
        // Indicates how fast the skill fires
        public float baseDuration = 0.2f;
        private float duration;

        // Effect prefabs for the skill
        public GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;
        public GameObject tracerEffectPrefab = Assets.FetchAsset<GameObject>("RoR2/Base/Bandit2/TracerBandit2Rifle.prefab");

        // Runs every time the skill is activated
        public override void OnEnter()
        {
            base.OnEnter();

            // Calculate duration
            duration = baseDuration / attackSpeedStat;

            // Get aim ray
            Ray aimRay = GetAimRay();
            StartAimMode(aimRay, 2f, false);

            // Play firing sound
            Util.PlaySound("Play_bandit2_m1_rifle", gameObject);

            // Apply recoil
            AddRecoil(-1.2f, 1.2f, -1.2f, 1.2f);

            // Check for firing effect
            if (FireBarrage.effectPrefab)
            {
                // Do muzzle flash (won't happen most of the time)
                EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, gameObject, "MuzzleRight", false);
            }

            // Check for network authority
            if (isAuthority)
            {
                // Do bullet attack
                new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0.0f,
                    maxSpread = 0.0f,
                    bulletCount = 1U,
                    procCoefficient = 1.0f,
                    damage = characterBody.damage * 6.0f,
                    force = 5,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = tracerEffectPrefab,
                    muzzleName = "MuzzleRight",
                    hitEffectPrefab = hitEffectPrefab,
                    isCrit = RollCrit(),
                    HitEffectNormal = false,
                    stopperMask = LayerIndex.world.mask,
                    smartCollision = true,
                    maxDistance = 500f
                }.Fire();
            }
        }

        // Runs once after the skill ends
        public override void OnExit()
        {
            base.OnExit();
        }

        // Runs while the skill is active - always check to end the skill when the duration is exceeded
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Check if duration exceeded
            if (fixedAge >= duration && isAuthority)
            {
                // End skill
                outer.SetNextStateToMain();
                return;
            }
        }

        // Returns the priority needed to interrupt the skill while active
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
