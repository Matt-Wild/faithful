using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStatsMonitor : DebugPanel
    {
        // Store reference to stat texts
        protected Text baseHealth;
        protected Text baseRegen;
        protected Text baseArmour;
        protected Text baseShield;
        protected Text baseDamage;
        protected Text baseAttackSpeed;
        protected Text baseCrit;
        protected Text baseSpeed;
        protected Text baseAcceleration;
        protected Text baseJumpCount;
        protected Text baseJumpPower;
        protected Text currentHealth;
        protected Text currentRegen;
        protected Text currentArmour;
        protected Text currentShield;
        protected Text currentBarrier;
        protected Text currentBarrierDecay;
        protected Text currentOneShotProtection;
        protected Text currentDamage;
        protected Text currentAttackSpeed;
        protected Text currentCrit;
        protected Text currentCritMultiplier;
        protected Text currentOutOfCombat;
        protected Text currentOutOfDanger;
        protected Text currentSpeed;
        protected Text currentAcceleration;
        protected Text currentJumpCount;
        protected Text currentJumpPower;
        protected Text currentJumpHeight;
        protected Text currentSprinting;
        protected Text currentFlying;
        protected Text positionX;
        protected Text positionY;
        protected Text positionZ;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Get stat texts
            baseHealth = transform.Find("BaseHealthText").GetComponent<Text>();
            baseRegen = transform.Find("BaseRegenText").GetComponent<Text>();
            baseArmour = transform.Find("BaseArmourText").GetComponent<Text>();
            baseShield = transform.Find("BaseShieldText").GetComponent<Text>();
            baseDamage = transform.Find("BaseDamageText").GetComponent<Text>();
            baseAttackSpeed = transform.Find("BaseAttackSpeedText").GetComponent<Text>();
            baseCrit = transform.Find("BaseCritText").GetComponent<Text>();
            baseSpeed = transform.Find("BaseSpeedText").GetComponent<Text>();
            baseAcceleration = transform.Find("BaseAccelerationText").GetComponent<Text>();
            baseJumpCount = transform.Find("BaseJumpCountText").GetComponent<Text>();
            baseJumpPower = transform.Find("BaseJumpPowerText").GetComponent<Text>();
            currentHealth = transform.Find("CurrentHealthText").GetComponent<Text>();
            currentRegen = transform.Find("CurrentRegenText").GetComponent<Text>();
            currentArmour = transform.Find("CurrentArmourText").GetComponent<Text>();
            currentShield = transform.Find("CurrentShieldText").GetComponent<Text>();
            currentBarrier = transform.Find("CurrentBarrierText").GetComponent<Text>();
            currentBarrierDecay = transform.Find("CurrentBarrierDecayText").GetComponent<Text>();
            currentOneShotProtection = transform.Find("CurrentOneShotProtectionText").GetComponent<Text>();
            currentDamage = transform.Find("CurrentDamageText").GetComponent<Text>();
            currentAttackSpeed = transform.Find("CurrentAttackSpeedText").GetComponent<Text>();
            currentCrit = transform.Find("CurrentCritText").GetComponent<Text>();
            currentCritMultiplier = transform.Find("CurrentCritMultiplierText").GetComponent<Text>();
            currentOutOfCombat = transform.Find("CurrentOutOfCombatText").GetComponent<Text>();
            currentOutOfDanger = transform.Find("CurrentOutOfDangerText").GetComponent<Text>();
            currentSpeed = transform.Find("CurrentSpeedText").GetComponent<Text>();
            currentAcceleration = transform.Find("CurrentAccelerationText").GetComponent<Text>();
            currentJumpCount = transform.Find("CurrentJumpCountText").GetComponent<Text>();
            currentJumpPower = transform.Find("CurrentJumpPowerText").GetComponent<Text>();
            currentJumpHeight = transform.Find("CurrentJumpHeightText").GetComponent<Text>();
            currentSprinting = transform.Find("CurrentSprintingText").GetComponent<Text>();
            currentFlying = transform.Find("CurrentFlyingText").GetComponent<Text>();
            positionX = transform.Find("PositionXText").GetComponent<Text>();
            positionY = transform.Find("PositionYText").GetComponent<Text>();
            positionZ = transform.Find("PositionZText").GetComponent<Text>();
        }

        void FixedUpdate()
        {
            // Check for character body
            if (debugController.localBody == null)
            {
                return;
            }

            // Update stat texts
            baseHealth.text = $"b.hp: {debugController.localBody.baseMaxHealth}";
            baseRegen.text = $"b.rgn: {debugController.localBody.baseRegen}";
            baseArmour.text = $"b.arm: {debugController.localBody.baseArmor}";
            baseShield.text = $"b.shd: {debugController.localBody.baseMaxShield}";
            baseDamage.text = $"b.dmg: {debugController.localBody.baseDamage}";
            baseAttackSpeed.text = $"b.asp: {debugController.localBody.baseAttackSpeed * 100.0f}%";
            baseCrit.text = $"b.crt: {debugController.localBody.baseCrit}%";
            baseSpeed.text = $"b.spd: {debugController.localBody.baseMoveSpeed}";
            baseAcceleration.text = $"b.acc: {debugController.localBody.baseAcceleration}";
            baseJumpCount.text = $"b.jcn: {debugController.localBody.baseJumpCount}";
            baseJumpPower.text = $"b.jpw: {debugController.localBody.baseJumpPower}";
            currentHealth.text = $"c.hp: {debugController.localBody.maxHealth}";
            currentRegen.text = $"c.rgn: {debugController.localBody.regen}";
            currentArmour.text = $"c.arm: {debugController.localBody.armor}";
            currentShield.text = $"c.shd: {debugController.localBody.maxShield}";
            currentBarrier.text = $"c.brr: {debugController.localBody.maxBarrier}";
            currentBarrierDecay.text = $"c.bdc: {debugController.localBody.barrierDecayRate}";
            currentOneShotProtection.text = $"c.osp: {debugController.localBody.oneShotProtectionFraction * 100.0f}%";
            currentDamage.text = $"c.dmg: {debugController.localBody.damage}";
            currentAttackSpeed.text = $"c.asp: {debugController.localBody.attackSpeed * 100.0f}%";
            currentCrit.text = $"c.crt: {debugController.localBody.crit}%";
            currentCritMultiplier.text = $"c.cmt: {debugController.localBody.critMultiplier * 100.0f}%";
            currentOutOfCombat.text = $"c.ooc: {debugController.localBody.outOfCombat}";
            currentOutOfDanger.text = $"c.ood: {debugController.localBody.outOfDanger}";
            currentSpeed.text = $"c.spd: {debugController.localBody.moveSpeed}";
            currentAcceleration.text = $"c.acc: {debugController.localBody.acceleration}";
            currentJumpCount.text = $"c.jcn: {debugController.localBody.maxJumpCount}";
            currentJumpPower.text = $"c.jpw: {debugController.localBody.jumpPower}";
            currentJumpHeight.text = $"c.jhg: {debugController.localBody.maxJumpHeight}";
            currentSprinting.text = $"c.spr: {debugController.localBody.isSprinting}";
            currentFlying.text = $"c.fly: {debugController.localBody.isFlying}";
            positionX.text = $"x: {Mathf.RoundToInt(debugController.localBody.corePosition.x)}";
            positionY.text = $"y: {Mathf.RoundToInt(debugController.localBody.corePosition.y)}";
            positionZ.text = $"z: {Mathf.RoundToInt(debugController.localBody.corePosition.z)}";
        }
    }
}
