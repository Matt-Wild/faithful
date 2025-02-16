using RoR2;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStatsMonitor : DebugPanel
    {
        // Store reference to stat texts
        protected TextMeshProUGUI baseHealth;
        protected TextMeshProUGUI baseRegen;
        protected TextMeshProUGUI baseArmour;
        protected TextMeshProUGUI baseShield;
        protected TextMeshProUGUI baseDamage;
        protected TextMeshProUGUI baseAttackSpeed;
        protected TextMeshProUGUI baseCrit;
        protected TextMeshProUGUI baseSpeed;
        protected TextMeshProUGUI baseAcceleration;
        protected TextMeshProUGUI baseJumpCount;
        protected TextMeshProUGUI baseJumpPower;
        protected TextMeshProUGUI currentHealth;
        protected TextMeshProUGUI currentRegen;
        protected TextMeshProUGUI currentArmour;
        protected TextMeshProUGUI currentShield;
        protected TextMeshProUGUI currentBarrier;
        protected TextMeshProUGUI currentBarrierDecay;
        protected TextMeshProUGUI currentOneShotProtection;
        protected TextMeshProUGUI currentDamage;
        protected TextMeshProUGUI currentAttackSpeed;
        protected TextMeshProUGUI currentCrit;
        protected TextMeshProUGUI currentCritMultiplier;
        protected TextMeshProUGUI currentOutOfCombat;
        protected TextMeshProUGUI currentOutOfDanger;
        protected TextMeshProUGUI currentSpeed;
        protected TextMeshProUGUI currentAcceleration;
        protected TextMeshProUGUI currentJumpCount;
        protected TextMeshProUGUI currentJumpPower;
        protected TextMeshProUGUI currentJumpHeight;
        protected TextMeshProUGUI currentSprinting;
        protected TextMeshProUGUI currentFlying;
        protected TextMeshProUGUI positionX;
        protected TextMeshProUGUI positionY;
        protected TextMeshProUGUI positionZ;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Get stat texts
            baseHealth = Utils.FindChildWithTerm(transform, "BaseHealthText").GetComponent<TextMeshProUGUI>();
            baseRegen = Utils.FindChildWithTerm(transform, "BaseRegenText").GetComponent<TextMeshProUGUI>();
            baseArmour = Utils.FindChildWithTerm(transform, "BaseArmourText").GetComponent<TextMeshProUGUI>();
            baseShield = Utils.FindChildWithTerm(transform, "BaseShieldText").GetComponent<TextMeshProUGUI>();
            baseDamage = Utils.FindChildWithTerm(transform, "BaseDamageText").GetComponent<TextMeshProUGUI>();
            baseAttackSpeed = Utils.FindChildWithTerm(transform, "BaseAttackSpeedText").GetComponent<TextMeshProUGUI>();
            baseCrit = Utils.FindChildWithTerm(transform, "BaseCritText").GetComponent<TextMeshProUGUI>();
            baseSpeed = Utils.FindChildWithTerm(transform, "BaseSpeedText").GetComponent<TextMeshProUGUI>();
            baseAcceleration = Utils.FindChildWithTerm(transform, "BaseAccelerationText").GetComponent<TextMeshProUGUI>();
            baseJumpCount = Utils.FindChildWithTerm(transform, "BaseJumpCountText").GetComponent<TextMeshProUGUI>();
            baseJumpPower = Utils.FindChildWithTerm(transform, "BaseJumpPowerText").GetComponent<TextMeshProUGUI>();
            currentHealth = Utils.FindChildWithTerm(transform, "CurrentHealthText").GetComponent<TextMeshProUGUI>();
            currentRegen = Utils.FindChildWithTerm(transform, "CurrentRegenText").GetComponent<TextMeshProUGUI>();
            currentArmour = Utils.FindChildWithTerm(transform, "CurrentArmourText").GetComponent<TextMeshProUGUI>();
            currentShield = Utils.FindChildWithTerm(transform, "CurrentShieldText").GetComponent<TextMeshProUGUI>();
            currentBarrier = Utils.FindChildWithTerm(transform, "CurrentBarrierText").GetComponent<TextMeshProUGUI>();
            currentBarrierDecay = Utils.FindChildWithTerm(transform, "CurrentBarrierDecayText").GetComponent<TextMeshProUGUI>();
            currentOneShotProtection = Utils.FindChildWithTerm(transform, "CurrentOneShotProtectionText").GetComponent<TextMeshProUGUI>();
            currentDamage = Utils.FindChildWithTerm(transform, "CurrentDamageText").GetComponent<TextMeshProUGUI>();
            currentAttackSpeed = Utils.FindChildWithTerm(transform, "CurrentAttackSpeedText").GetComponent<TextMeshProUGUI>();
            currentCrit = Utils.FindChildWithTerm(transform, "CurrentCritText").GetComponent<TextMeshProUGUI>();
            currentCritMultiplier = Utils.FindChildWithTerm(transform, "CurrentCritMultiplierText").GetComponent<TextMeshProUGUI>();
            currentOutOfCombat = Utils.FindChildWithTerm(transform, "CurrentOutOfCombatText").GetComponent<TextMeshProUGUI>();
            currentOutOfDanger = Utils.FindChildWithTerm(transform, "CurrentOutOfDangerText").GetComponent<TextMeshProUGUI>();
            currentSpeed = Utils.FindChildWithTerm(transform, "CurrentSpeedText").GetComponent<TextMeshProUGUI>();
            currentAcceleration = Utils.FindChildWithTerm(transform, "CurrentAccelerationText").GetComponent<TextMeshProUGUI>();
            currentJumpCount = Utils.FindChildWithTerm(transform, "CurrentJumpCountText").GetComponent<TextMeshProUGUI>();
            currentJumpPower = Utils.FindChildWithTerm(transform, "CurrentJumpPowerText").GetComponent<TextMeshProUGUI>();
            currentJumpHeight = Utils.FindChildWithTerm(transform, "CurrentJumpHeightText").GetComponent<TextMeshProUGUI>();
            currentSprinting = Utils.FindChildWithTerm(transform, "CurrentSprintingText").GetComponent<TextMeshProUGUI>();
            currentFlying = Utils.FindChildWithTerm(transform, "CurrentFlyingText").GetComponent<TextMeshProUGUI>();
            positionX = Utils.FindChildWithTerm(transform, "PositionXText").GetComponent<TextMeshProUGUI>();
            positionY = Utils.FindChildWithTerm(transform, "PositionYText").GetComponent<TextMeshProUGUI>();
            positionZ = Utils.FindChildWithTerm(transform, "PositionZText").GetComponent<TextMeshProUGUI>();
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
