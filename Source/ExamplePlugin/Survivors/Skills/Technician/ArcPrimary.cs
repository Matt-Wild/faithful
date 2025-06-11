using EntityStates;
using RoR2;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful.Skills.Technician
{
    public class ArcPrimary : BaseSkillState
    {
        GameObject arcEffectPrefab = Assets.technicianArcPrefab;

        float baseEntryDuration = 0.8f;

        float procCoefficientPerTick = 0.625f;

        float tickFrequency = 8.0f;

        string startAttackSoundString;

        string endAttackSoundString;

        float tickDamageCoefficient = 0.5f;

        float arcStopwatch;

        float stopwatch;

        float entryDuration = 0.6f;

        bool hasBegunArc;

        ChildLocator childLocator;

        Transform leftArcTransform;

        Transform rightArcTransform;

        Transform leftArcEndTransform;

        Transform rightArcEndTransform;

        TechnicianTracker tracker;

        BuffIndex overclockedBuffIndex;

        public override void OnEnter()
        {
            base.OnEnter();

            // Fetch buff indexes
            overclockedBuffIndex = Buffs.GetBuff("OVERCLOCKED").buffDef.buffIndex;

            // Get how long the "build up" for this skill is
            stopwatch = 0f;
            entryDuration = baseEntryDuration / attackSpeedStat;

            // Check for model transfor
            Transform modelTransform = GetModelTransform();
            if (characterBody)
            {
                // Fetch tracker
                tracker = characterBody.GetComponent<TechnicianTracker>();

                characterBody.SetAimTimer(1f);

                // Set tracker lock
                if (tracker != null) tracker.SetLock(SkillSlot.Primary, true);
            }
            if (modelTransform)
            {
                // Fetch child locator
                childLocator = modelTransform.GetComponent<ChildLocator>();
            }

            // Play arc start animation based on entry duration
            PlayAnimation("BothArms, Override", "ArcStart", "ArcStart.playbackRate", entryDuration);
        }

        public override void OnExit()
        {
            Util.PlaySound(endAttackSoundString, gameObject);

            // Play exit duration
            PlayAnimation("BothArms, Override", "ArcEnd", "ArcEnd.playbackRate", 0.4f / attackSpeedStat);

            // Destroy arc effects
            if (leftArcTransform) Destroy(leftArcTransform.gameObject);
            if (rightArcTransform) Destroy(rightArcTransform.gameObject);

            base.OnExit();

            // Set tracker lock
            if (tracker != null) tracker.SetLock(SkillSlot.Primary, false);
        }

        private void FireArc(string muzzleString, float _damageMult = 1.0f)
        {
            // Check for target
            Transform target = tracker?.GetTrackingTarget()?.transform;
            if (target == null) return;

            // Get arc source
            Vector3 arcSource = (leftArcTransform.position + rightArcTransform.position) / 2.0f;

            // Get arc direction
            Vector3 arcDir = target.position - arcSource;

            // Calculate aim ray and do bullet attack if server/host
            Ray aimRay = new Ray(arcSource, arcDir.normalized);
            if (isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack();
                bulletAttack.owner = gameObject;
                bulletAttack.weapon = gameObject;
                bulletAttack.origin = aimRay.origin;
                bulletAttack.aimVector = aimRay.direction;
                bulletAttack.minSpread = 0.0f;
                bulletAttack.damage = tickDamageCoefficient * damageStat * _damageMult;
                bulletAttack.force = 0.0f;
                bulletAttack.muzzleName = muzzleString;
                bulletAttack.isCrit = Util.CheckRoll(critStat, characterBody.master);
                bulletAttack.radius = 1.0f;
                bulletAttack.falloffModel = BulletAttack.FalloffModel.None;
                bulletAttack.stopperMask = LayerIndex.world.mask;
                bulletAttack.procCoefficient = procCoefficientPerTick;
                bulletAttack.maxDistance = arcDir.magnitude;
                bulletAttack.smartCollision = true;
                bulletAttack.damageType = DamageType.SlowOnHit;
                bulletAttack.allowTrajectoryAimAssist = false;
                bulletAttack.damageType.damageSource = DamageSource.Primary;
                bulletAttack.hitCallback = ArcHitCallback;
                bulletAttack.Fire();
            }

            if (characterBody)
            {
                characterBody.SetAimTimer(1f);
            }
        }

        // This is a modified method from RoR2.BulletAttack - UPDATE IF ISSUES ARISE
        private bool ArcHitCallback(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            bool result = false;
            if (hitInfo.collider)
            {
                result = ((1 << hitInfo.collider.gameObject.layer & bulletAttack.stopperMask) == 0);
            }
            BulletAttack.PlayHitEffect(bulletAttack, ref hitInfo);
            GameObject entityObject = hitInfo.entityObject;
            if (entityObject)
            {
                float num = BulletAttack.CalcFalloffFactor(bulletAttack.falloffModel, hitInfo.distance);
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.damage = bulletAttack.damage * num;
                damageInfo.crit = bulletAttack.isCrit;
                damageInfo.attacker = bulletAttack.owner;
                damageInfo.inflictor = bulletAttack.weapon;
                damageInfo.position = hitInfo.point;
                damageInfo.force = hitInfo.direction * (bulletAttack.force * num);
                damageInfo.procChainMask = bulletAttack.procChainMask;
                damageInfo.procCoefficient = bulletAttack.procCoefficient;
                damageInfo.damageType = bulletAttack.damageType;
                damageInfo.damageColorIndex = bulletAttack.damageColorIndex;
                damageInfo.ModifyDamageInfo(hitInfo.damageModifier);
                if (hitInfo.isSniperHit)
                {
                    damageInfo.crit = true;
                    damageInfo.damageColorIndex = DamageColorIndex.Sniper;
                }
                BulletAttack.ModifyOutgoingDamageCallback modifyOutgoingDamageCallback = bulletAttack.modifyOutgoingDamageCallback;
                if (modifyOutgoingDamageCallback != null)
                {
                    modifyOutgoingDamageCallback(bulletAttack, ref hitInfo, damageInfo);
                }
                TeamIndex attackerTeamIndex = TeamIndex.None;
                if (bulletAttack.owner)
                {
                    TeamComponent component = bulletAttack.owner.GetComponent<TeamComponent>();
                    if (component)
                    {
                        attackerTeamIndex = component.teamIndex;
                    }
                }
                HealthComponent healthComponent = null;
                if (hitInfo.hitHurtBox)
                {
                    healthComponent = hitInfo.hitHurtBox.healthComponent;
                }
                bool flag = healthComponent && FriendlyFireManager.ShouldDirectHitProceed(healthComponent, attackerTeamIndex);
                if (NetworkServer.active)
                {
                    if (flag)
                    {
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, hitInfo.entityObject);
                    }
                    // INJECTED
                    else
                    {
                        // Fetch character body
                        CharacterBody body = healthComponent?.body;

                        // Check for mechanical flag
                        if (body != null && body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
                        {
                            // Add times overclocked buff
                            body.AddTimedBuff(overclockedBuffIndex, 1.0f);
                        }
                    }
                    // END INJECTED
                    GlobalEventManager.instance.OnHitAll(damageInfo, hitInfo.entityObject);
                }
                else if (ClientScene.ready)
                {
                    GameObject networkedGameObject = BulletAttack.GetNetworkedGameObject(entityObject);
                    BulletAttack.messageWriter.StartMessage(53);
                    int currentLogLevel = LogFilter.currentLogLevel;
                    LogFilter.currentLogLevel = 4;
                    BulletAttack.messageWriter.Write(networkedGameObject);
                    LogFilter.currentLogLevel = currentLogLevel;
                    BulletAttack.messageWriter.Write(damageInfo);
                    BulletAttack.messageWriter.Write(flag);
                    BulletAttack.messageWriter.FinishMessage();
                    ClientScene.readyConnection.SendWriter(BulletAttack.messageWriter, QosChannelIndex.defaultReliable.intVal);
                }
            }
            return result;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            // Check if should end Arc
            if (isAuthority && (!IsKeyDownAuthority() || characterBody.isSprinting || characterBody.allSkillsDisabled))
            {
                // Give stock back if arc hasn't even begun
                if (!hasBegunArc) skillLocator.primary.AddOneStock();

                outer.SetNextStateToMain();
                return;
            }

            // Check if target not found (probably killed)
            if (isAuthority && tracker != null && tracker.GetTrackingTarget() == null)
            {
                // Attempt to get new target
                tracker.SearchForTarget();

                // Check if target still not found
                if (tracker.GetTrackingTarget() == null)
                {
                    // Give stock back if arc hasn't even begun
                    if (!hasBegunArc) skillLocator.primary.AddOneStock();

                    outer.SetNextStateToMain();
                    return;
                }
            }

            // Has entry duration been exceeded
            stopwatch += GetDeltaTime();
            if (stopwatch >= entryDuration && !hasBegunArc)
            {
                // Has started actually attacking
                hasBegunArc = true;
                Util.PlaySound(startAttackSoundString, gameObject);

                // Play loop aniation
                PlayAnimation("BothArms, Override", "ArcLoop", "ArcLoop.playbackRate", 10.0f);

                // Create Arc effects if child locator exists
                if (childLocator)
                {
                    Transform transform = childLocator.FindChild("ArcLeft");
                    Transform transform2 = childLocator.FindChild("ArcRight");
                    if (transform)
                    {
                        leftArcTransform = Object.Instantiate(arcEffectPrefab, transform).transform;
                        leftArcEndTransform = leftArcTransform.Find("LaserEnd");
                    }
                    if (transform2)
                    {
                        rightArcTransform = Object.Instantiate(arcEffectPrefab, transform2).transform;
                        rightArcEndTransform = rightArcTransform.Find("LaserEnd");
                    }
                }

                // Do initial Arc attack (initial tick is 5x damage)
                FireArc("MuzzleCenter", 5.0f);
            }

            // Continuation of attack
            if (hasBegunArc)
            {
                // Constantly play loop
                PlayAnimation("BothArms, Override", "ArcLoop", "ArcLoop.playbackRate", 10.0f);

                // Stopwatch determines damage frequency
                arcStopwatch += Time.deltaTime;
                float num = 1f / tickFrequency / attackSpeedStat;
                if (arcStopwatch > num)
                {
                    // Reset stopwatch and do attack
                    arcStopwatch -= num;
                    FireArc("MuzzleCenter");
                }

                // Check for target
                Transform target = tracker?.GetTrackingTarget()?.transform;
                if (target != null)
                {
                    // Update arc end points
                    if (leftArcEndTransform != null) leftArcEndTransform.position = target.position;
                    if (rightArcEndTransform != null) rightArcEndTransform.position = target.position;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
