using EntityStates;
using UnityEngine;

namespace Faithful.Skills.Temp
{
    public class TempSpecial : GenericProjectileBaseState
    {
        // Amount of grenades thrown out
        int grenadeCount = 5;

        // Runs every time the skill is activated
        public override void OnEnter()
        {
            // Get needed assets
            effectPrefab = Assets.FetchAsset<GameObject>("RoR2/Base/Commando/OmniExplosionVFXCommandoGrenade.prefab");
            projectilePrefab = Assets.FetchAsset<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab");

            // Set throwing sound string
            attackSoundString = "Play_commando_M2_grenade_throw";

            // Set state values
            damageCoefficient = 5.0f;
            force = 12.0f;
            minSpread = 10.0f;
            maxSpread = -10.0f;
            baseDuration = 0.4f;
            bloom = 1.0f;

            base.OnEnter();
        }

        public override void FireProjectile()
        {
            // Fire grenades
            for (int i = 0; i < grenadeCount; i++) base.FireProjectile();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
