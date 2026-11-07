using System;
using System.Collections.Generic;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Faithful
{
    internal delegate bool AllowFriendlyHitCallback(CharacterBody _attacker, CharacterBody _victim);

    internal static partial class Behaviour
    {
        // Callbacks can allow a hit that vanilla rejected due to team restrictions
        private static readonly List<AllowFriendlyHitCallback> allowFriendlyHitCallbacks = new();

        private static readonly HookRegistration overlapFriendlyHitHook = new("OverlapAttack.HurtBoxPassesFilter",
            () => IL.RoR2.OverlapAttack.HurtBoxPassesFilter += HookOverlapFriendlyHit,
            () => IL.RoR2.OverlapAttack.HurtBoxPassesFilter -= HookOverlapFriendlyHit);

        private static readonly HookRegistration blastFriendlyHitHook = new("BlastAttack.CollectHits",
            () => IL.RoR2.BlastAttack.CollectHits += HookBlastFriendlyHit,
            () => IL.RoR2.BlastAttack.CollectHits -= HookBlastFriendlyHit);

        private static readonly HookRegistration damageTrailFriendlyHitHook = new("DamageTrail.DoDamage",
            () => IL.RoR2.DamageTrail.DoDamage += HookDamageTrailFriendlyHit,
            () => IL.RoR2.DamageTrail.DoDamage -= HookDamageTrailFriendlyHit);

        private static readonly HookRegistration bulletFriendlyHitHook = new("BulletAttack.DefaultHitCallbackImplementation",
            () => IL.RoR2.BulletAttack.DefaultHitCallbackImplementation += HookBulletFriendlyHit,
            () => IL.RoR2.BulletAttack.DefaultHitCallbackImplementation -= HookBulletFriendlyHit);

        private static readonly HookRegistration bulletIgnoreAlliesHook = new("BulletAttack.IgnoreAlliesFilter",
            () => IL.RoR2.BulletAttack.IgnoreAlliesFilter += HookBulletIgnoreAllies,
            () => IL.RoR2.BulletAttack.IgnoreAlliesFilter -= HookBulletIgnoreAllies);

        private static readonly HookRegistration bulletFriendlyFilterHook = new("BulletAttack.DefaultFilterCallbackImplementation",
            () => IL.RoR2.BulletAttack.DefaultFilterCallbackImplementation += HookBulletFriendlyFilter,
            () => IL.RoR2.BulletAttack.DefaultFilterCallbackImplementation -= HookBulletFriendlyFilter);

        private static readonly HookRegistration projectileAllyImpactHook = new("ProjectileIgnoreAllyImpacts.PassesFilters",
            () => On.RoR2.Projectile.ProjectileIgnoreAllyImpacts.PassesFilters += HookProjectileAllyImpact,
            () => On.RoR2.Projectile.ProjectileIgnoreAllyImpacts.PassesFilters -= HookProjectileAllyImpact);

        public static void AddAllowFriendlyHitCallback(AllowFriendlyHitCallback _callback)
        {
            allowFriendlyHitCallbacks.Add(_callback);
            DebugLog("Added Allow Friendly Hit behaviour");
        }

        private static void HookOverlapFriendlyHit(ILContext _il)
        {
            PatchFriendlyHitCheck(_il, typeof(OverlapAttack).GetField("attacker"), overlapFriendlyHitHook);
        }

        private static void HookBlastFriendlyHit(ILContext _il)
        {
            PatchFriendlyHitCheck(_il, typeof(BlastAttack).GetField("attacker"), blastFriendlyHitHook, true);
        }

        private static void HookDamageTrailFriendlyHit(ILContext _il)
        {
            PatchFriendlyHitCheck(_il, typeof(DamageTrail).GetField("owner"), damageTrailFriendlyHitHook, true);
        }

        private static void HookBulletFriendlyHit(ILContext _il)
        {
            PatchFriendlyHitCheck(_il, typeof(BulletAttack).GetField("owner"), bulletFriendlyHitHook);
        }

        private static void HookBulletIgnoreAllies(ILContext _il)
        {
            PatchFriendlyHitCheck(_il, typeof(BulletAttack).GetField("owner"), bulletIgnoreAlliesHook);
        }

        private static void PatchFriendlyHitCheck(ILContext _il, FieldInfo _attackerField, HookRegistration _hook, bool _splash = false)
        {
            PatchFriendlyHitCheck(_il, [_attackerField], _hook, _splash);
        }

        private static void PatchFriendlyHitCheck(ILContext _il, FieldInfo[] _attackerFields, HookRegistration _hook, bool _splash = false)
        {
            ILCursor cursor = FindFriendlyHitCheck(_il, _splash);

            // The victim and attacker team are already on the stack - add the actual attacker
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            foreach (FieldInfo field in _attackerFields)
            {
                cursor.Emit(OpCodes.Ldfld, _il.Method.Module.ImportReference(field));
            }

            cursor.EmitDelegate<Func<HealthComponent, TeamIndex, GameObject, bool>>((victim, team, attacker) =>
            {
                // Preserve vanilla permissions, including artifacts and other friendly fire effects
                bool allowed = _splash ? FriendlyFireManager.ShouldSplashHitProceed(victim, team) : FriendlyFireManager.ShouldDirectHitProceed(victim, team);
                
                return allowed || AllowFriendlyHit(attacker, victim, _hook);
            });
        }

        private static ILCursor FindFriendlyHitCheck(ILContext _il, bool _splash = false)
        {
            ILCursor cursor = new(_il);
            string methodName = _splash ? "ShouldSplashHitProceed" : "ShouldDirectHitProceed";
            if (!cursor.TryGotoNext(MoveType.Before, instruction => (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt) &&
                instruction.Operand is MethodReference method && method.DeclaringType.FullName == typeof(FriendlyFireManager).FullName && method.Name == methodName))
            {
                throw new InvalidOperationException($"Could not find {methodName} in {_il.Method.Name}.");
            }

            return cursor;
        }

        private static void PatchDamageInfoFriendlyHitCheck(ILContext _il, HookRegistration _hook, bool _onHit)
        {
            ILCursor cursor = FindFriendlyHitCheck(_il);

            // Find the argument by type because generated hook methods may include an explicit self parameter
            ParameterDefinition damageInfo = null;
            foreach (ParameterDefinition parameter in _il.Method.Parameters)
            {
                if (parameter.ParameterType.FullName != typeof(DamageInfo).FullName) continue;
                damageInfo = parameter;

                break;
            }

            if (damageInfo == null) throw new InvalidOperationException($"Could not find the damage info parameter in {_il.Method.Name}.");

            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg, damageInfo);
            cursor.EmitDelegate<Func<HealthComponent, TeamIndex, DamageInfo, bool>>((victim, team, info) => AllowDamageInfoFriendlyHit(victim, team, info, _hook, _onHit));
        }

        private static void HookBulletFriendlyFilter(ILContext _il)
        {
            ILCursor cursor = new(_il);
            if (!cursor.TryGotoNext(MoveType.After,
                instruction => instruction.MatchCallOrCallvirt<CharacterBody>("get_teamComponent"),
                instruction => instruction.MatchCallOrCallvirt<TeamComponent>("get_teamIndex")))
            {
                throw new InvalidOperationException("Could not find the victim team comparison in the default bullet filter.");
            }

            // Only replace the same team rejection (preserve weapon and other hit exclusions)
            Instruction comparison = cursor.Next;
            if (comparison == null || (comparison.OpCode != OpCodes.Bne_Un && comparison.OpCode != OpCodes.Bne_Un_S))
            {
                throw new InvalidOperationException("Unexpected victim team comparison in the default bullet filter.");
            }

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<IgnoreFriendlyBulletHit>(ShouldIgnoreFriendlyBulletHit);
            comparison.OpCode = OpCodes.Brfalse;
        }

        private delegate bool IgnoreFriendlyBulletHit(TeamIndex _attackerTeam, TeamIndex _victimTeam, BulletAttack _attack, ref BulletAttack.BulletHit _hit);

        private static bool ShouldIgnoreFriendlyBulletHit(TeamIndex _attackerTeam, TeamIndex _victimTeam, BulletAttack _attack, ref BulletAttack.BulletHit _hit)
        {
            if (_attackerTeam != _victimTeam) return false;
            if (!_hit.collider || !_hit.collider.TryGetComponent(out HurtBox hurtBox)) return true;

            return !AllowFriendlyHit(_attack.owner, hurtBox.healthComponent, bulletFriendlyFilterHook);
        }

        private static bool HookProjectileAllyImpact(On.RoR2.Projectile.ProjectileIgnoreAllyImpacts.orig_PassesFilters orig, ProjectileIgnoreAllyImpacts self, ProjectileImpactInfo impactInfo)
        {
            // Preserve impacts already allowed by vanilla
            if (orig(self, impactInfo)) return true;
            if (!CanRunHook(projectileAllyImpactHook)) return false;

            // Vanilla rejected an allied hurtbox - ask the existing hit permission callbacks
            if (!impactInfo.collider || !impactInfo.collider.TryGetComponent(out HurtBox hurtBox)) return false;
            ProjectileController projectile = self.projectileController;
            if (!projectile) return false;

            return AllowFriendlyHit(projectile.owner, hurtBox.healthComponent, projectileAllyImpactHook);
        }

        private static bool AllowFriendlyHit(GameObject _attacker, HealthComponent _victim, HookRegistration _hook)
        {
            if (!CanRunHook(_hook) || allowFriendlyHitCallbacks.Count == 0 || !_victim || !_victim.body) return false;

            CharacterBody attackerBody = _attacker ? _attacker.GetComponent<CharacterBody>() : null;
            _hook.StartProfiling();
            try
            {
                foreach (AllowFriendlyHitCallback callback in allowFriendlyHitCallbacks)
                {
                    if (callback(attackerBody, _victim.body)) return true;
                }
            }
            finally
            {
                _hook.StopProfiling();
            }

            return false;
        }
    }
}
