using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace Faithful
{
    internal static partial class Behaviour
    {
        // Team exclusions are decided once when the projectile initialises
        private static readonly HookRegistration projectileAllyCollisionHook = new("ProjectileController.IgnoreCollisionsWithAllies",
            () => IL.RoR2.Projectile.ProjectileController.IgnoreCollisionsWithAllies += HookProjectileAllyCollisions,
            () => IL.RoR2.Projectile.ProjectileController.IgnoreCollisionsWithAllies -= HookProjectileAllyCollisions);

        private static readonly HookRegistration projectileTeamCollisionHook = new("ProjectileIgnoreTeamCollisions.OnProjectileInitialized",
            () => IL.RoR2.Projectile.ProjectileIgnoreTeamCollisions.OnProjectileInitialized += HookProjectileTeamCollisions,
            () => IL.RoR2.Projectile.ProjectileIgnoreTeamCollisions.OnProjectileInitialized -= HookProjectileTeamCollisions);

        private static void HookProjectileAllyCollisions(ILContext _il)
        {
            ILCursor cursor = new(_il);
            if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCall(typeof(Physics), nameof(Physics.IgnoreCollision)) &&
                instruction.Operand is MethodReference method && method.Parameters.Count == 2))
            {
                throw new InvalidOperationException("Could not find the allied collider exclusion in IgnoreCollisionsWithAllies.");
            }

            // Add the projectile to the two colliders already on the stack
            cursor.Remove();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(IgnoreProjectileAllyCollision);
        }

        private static void IgnoreProjectileAllyCollision(Collider _victim, Collider _collider, ProjectileController _projectile)
        {
            // Skip this team exclusion when a callback permits the hit, preserving owner exclusions
            if (CanRunHook(projectileAllyCollisionHook) && _victim.TryGetComponent(out HurtBox hurtBox) && hurtBox.healthComponent &&
                hurtBox.healthComponent.gameObject != _projectile.owner && AllowFriendlyHit(_projectile.owner, hurtBox.healthComponent, projectileAllyCollisionHook)) return;

            Physics.IgnoreCollision(_victim, _collider);
        }

        private static void HookProjectileTeamCollisions(ILContext _il)
        {
            ILCursor cursor = new(_il);
            int count = 0;
            while (cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCallOrCallvirt<ProjectileController>("IgnoreCollisionsWithBody")))
            {
                cursor.Remove();
                cursor.EmitDelegate(IgnoreProjectileTeamBodyCollisions);
                count++;
            }

            if (count == 0) throw new InvalidOperationException("Could not find body exclusions in ProjectileIgnoreTeamCollisions.OnProjectileInitialized.");
        }

        private static void IgnoreProjectileTeamBodyCollisions(ProjectileController _projectile, GameObject _body, bool _ignore)
        {
            // Only override this component's team exclusions, leaving explicit owner/self exclusions intact
            if (_ignore && CanRunHook(projectileTeamCollisionHook) && _body && _body != _projectile.owner &&
                _body.TryGetComponent(out HealthComponent victim) &&
                AllowFriendlyHit(_projectile.owner, victim, projectileTeamCollisionHook)) return;

            _projectile.IgnoreCollisionsWithBody(_body, _ignore);
        }

        // Custom spore filters check team permission before the standard overlap filter
        private static readonly HookRegistration mushroomSporeFilterHook = new("BigMushSporeFilter.BigMushFilter",
            () => On.RoR2.Projectile.BigMushSporeFilter.BigMushFilter += HookMushroomSporeFilter,
            () => On.RoR2.Projectile.BigMushSporeFilter.BigMushFilter -= HookMushroomSporeFilter);

        private static bool HookMushroomSporeFilter(On.RoR2.Projectile.BigMushSporeFilter.orig_BigMushFilter orig, BigMushSporeFilter self, OverlapAttack.OverlapAttackFilterStruct filterStruct)
        {
            // Preserve hits already allowed by vanilla
            if (orig(self, filterStruct)) return true;
            if (!CanRunHook(mushroomSporeFilterHook) || FriendlyFireManager.friendlyFireMode != FriendlyFireManager.FriendlyFireMode.Off) return false;

            HealthComponent victim = filterStruct.healthComponent;
            if (!victim || !victim.body) return false;

            CharacterBody body = victim.body;
            ProjectileController projectile = self.projectileController;
            if (!projectile || !self.teamFilter || body.gameObject == projectile.owner) return false;

            // Only override the same team rejection, preserving the filter's mushroom exclusions
            TeamIndex victimTeam = body.teamComponent ? body.teamComponent.teamIndex : TeamIndex.None;
            if (victimTeam != self.teamFilter.teamIndex) return false;
            if (body.bodyIndex == DLC4Content.BodyPrefabs.BigMushBody.bodyIndex || body.bodyIndex == DLC4Content.BodyPrefabs.PlantMushBody.bodyIndex ||
                body.bodyIndex == RoR2Content.BodyPrefabs.MiniMushroomBody.bodyIndex) return false;

            return AllowFriendlyHit(projectile.owner, victim, mushroomSporeFilterHook);
        }

        // Ring explosions apply damage directly instead of using the standard blast attack
        private static readonly HookRegistration torusFriendlyHitHook = new("ProjectileImpactExplosionTorus.DamageTarget",
            () => IL.RoR2.Projectile.ProjectileImpactExplosionTorus.DamageTarget += HookTorusFriendlyHit,
            () => IL.RoR2.Projectile.ProjectileImpactExplosionTorus.DamageTarget -= HookTorusFriendlyHit);

        private static void HookTorusFriendlyHit(ILContext _il)
        {
            PatchFriendlyHitCheck(_il,
            [
                typeof(ProjectileExplosion).GetField("projectileController", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                typeof(ProjectileController).GetField("owner")
            ], torusFriendlyHitHook, true);
        }

        // Give shared searches the projectile owner, rather than the state that spawned it
        private static readonly HookRegistration sphereProjectileTargetHook = new("ProjectileSphereTargetFinder.SearchForTarget",
            () => On.RoR2.Projectile.ProjectileSphereTargetFinder.SearchForTarget += HookSphereProjectileTargets,
            () => On.RoR2.Projectile.ProjectileSphereTargetFinder.SearchForTarget -= HookSphereProjectileTargets);
        private static readonly HookRegistration wightProjectileTargetHook = new("ProjectileWightSphereTargetFinder.SearchForTarget",
            () => On.RoR2.Projectile.ProjectileWightSphereTargetFinder.SearchForTarget += HookWightProjectileTargets,
            () => On.RoR2.Projectile.ProjectileWightSphereTargetFinder.SearchForTarget -= HookWightProjectileTargets);
        private static readonly HookRegistration directionalProjectileTargetHook = new("ProjectileDirectionalTargetFinder.SearchForTarget",
            () => On.RoR2.Projectile.ProjectileDirectionalTargetFinder.SearchForTarget += HookDirectionalProjectileTargets,
            () => On.RoR2.Projectile.ProjectileDirectionalTargetFinder.SearchForTarget -= HookDirectionalProjectileTargets);
        private static readonly HookRegistration missileTargetHook = new("MissileController.FindTarget",
            () => On.RoR2.Projectile.MissileController.FindTarget += HookMissileTargets,
            () => On.RoR2.Projectile.MissileController.FindTarget -= HookMissileTargets);

        private static TargetSearchContext CreateProjectileSearchContext(Component _source, Func<HurtBox, bool> _filter = null,
            Func<IEnumerable<HurtBox>, IEnumerable<HurtBox>> _order = null)
        {
            ProjectileController projectile = _source.GetComponent<ProjectileController>();
            CharacterBody owner = projectile && projectile.owner ? projectile.owner.GetComponent<CharacterBody>() : null;
            return new(owner, projectile, _filter, _order);
        }

        private static void HookSphereProjectileTargets(On.RoR2.Projectile.ProjectileSphereTargetFinder.orig_SearchForTarget orig, ProjectileSphereTargetFinder self)
        {
            if (!CanRunHook(sphereProjectileTargetHook)) { orig(self); return; }

            TargetSearchContext previous = currentTargetSearchContext;
            currentTargetSearchContext = CreateProjectileSearchContext(self, self.PassesFilters);
            sphereProjectileTargetHook.StartProfiling();
            try
            {
                orig(self);
            }
            finally
            {
                currentTargetSearchContext = previous;
                sphereProjectileTargetHook.StopProfiling();
            }
        }

        private static void HookWightProjectileTargets(On.RoR2.Projectile.ProjectileWightSphereTargetFinder.orig_SearchForTarget orig, ProjectileWightSphereTargetFinder self)
        {
            if (!CanRunHook(wightProjectileTargetHook)) { orig(self); return; }

            TargetSearchContext previous = currentTargetSearchContext;

            // Vanilla prefers the last spirit-form candidate, otherwise the first valid candidate
            currentTargetSearchContext = CreateProjectileSearchContext(self, self.PassesFilters,
                candidates => candidates.Reverse().Where(candidate => candidate.GetComponentInParent<SpiritFormManager>())
                    .Concat(candidates.Where(candidate => !candidate.GetComponentInParent<SpiritFormManager>())));
            wightProjectileTargetHook.StartProfiling();
            try
            {
                orig(self);
            }
            finally
            {
                currentTargetSearchContext = previous;
                wightProjectileTargetHook.StopProfiling();
            }
        }

        private static void HookDirectionalProjectileTargets(On.RoR2.Projectile.ProjectileDirectionalTargetFinder.orig_SearchForTarget orig, ProjectileDirectionalTargetFinder self)
        {
            if (!CanRunHook(directionalProjectileTargetHook)) { orig(self); return; }

            TargetSearchContext previous = currentTargetSearchContext;
            currentTargetSearchContext = CreateProjectileSearchContext(self, self.PassesFilters);
            directionalProjectileTargetHook.StartProfiling();
            try
            {
                orig(self);
            }
            finally
            {
                currentTargetSearchContext = previous;
                directionalProjectileTargetHook.StopProfiling();
            }
        }

        private static Transform HookMissileTargets(On.RoR2.Projectile.MissileController.orig_FindTarget orig, MissileController self)
        {
            if (!CanRunHook(missileTargetHook)) return orig(self);

            TargetSearchContext previous = currentTargetSearchContext;
            currentTargetSearchContext = CreateProjectileSearchContext(self);
            missileTargetHook.StartProfiling();
            try
            {
                return orig(self);
            }
            finally
            {
                currentTargetSearchContext = previous;
                missileTargetHook.StopProfiling();
            }
        }
    }
}
