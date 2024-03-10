using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStatsMonitor : DebugPanel
    {
        // Store reference to host character body
        protected CharacterBody host;

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

            // Get host
            if (NetworkServer.active)
            {
                // Get first player
                host = PlayerCharacterMasterController.instances[0].master.GetBody();
            }
            else
            {
                // Get client ID
                int clientID = FindObjectOfType<NetworkManager>().client.connection.connectionId;
                
                // Cycle through players
                foreach (PlayerCharacterMasterController playerCharacterMasterController in PlayerCharacterMasterController.instances)
                {
                    // Compare connection IDs
                    if (playerCharacterMasterController.connectionToClient.connectionId == clientID)
                    {
                        // Get Character Body
                        host = playerCharacterMasterController.master.GetBody();
                        break;
                    }
                }
            }

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
            if (host == null)
            {
                return;
            }

            // Update stat texts
            baseHealth.text = $"b.hp: {host.baseMaxHealth}";
            baseRegen.text = $"b.rgn: {host.baseRegen}";
            baseArmour.text = $"b.arm: {host.baseArmor}";
            baseShield.text = $"b.shd: {host.baseMaxShield}";
            baseDamage.text = $"b.dmg: {host.baseDamage}";
            baseAttackSpeed.text = $"b.asp: {host.baseAttackSpeed}";
            baseCrit.text = $"b.crt: {host.baseCrit}%";
            baseSpeed.text = $"b.spd: {host.baseMoveSpeed}";
            baseAcceleration.text = $"b.acc: {host.baseAcceleration}";
            baseJumpCount.text = $"b.jcn: {host.baseJumpCount}";
            baseJumpPower.text = $"b.jpw: {host.baseJumpPower}";
            currentHealth.text = $"c.hp: {host.maxHealth}";
            currentRegen.text = $"c.rgn: {host.regen}";
            currentArmour.text = $"c.arm: {host.armor}";
            currentShield.text = $"c.shd: {host.maxShield}";
            currentBarrier.text = $"c.brr: {host.maxBarrier}";
            currentBarrierDecay.text = $"c.bdc: {host.barrierDecayRate}";
            currentOneShotProtection.text = $"c.osp: {host.oneShotProtectionFraction * 100.0f}%";
            currentDamage.text = $"c.dmg: {host.damage}";
            currentAttackSpeed.text = $"c.asp: {host.attackSpeed}";
            currentCrit.text = $"c.crt: {host.crit}%";
            currentCritMultiplier.text = $"c.cmt: {host.critMultiplier * 100.0f}%";
            currentOutOfCombat.text = $"c.ooc: {host.outOfCombat}";
            currentOutOfDanger.text = $"c.ood: {host.outOfDanger}";
            currentSpeed.text = $"c.spd: {host.moveSpeed}";
            currentAcceleration.text = $"c.acc: {host.acceleration}";
            currentJumpCount.text = $"c.jcn: {host.maxJumpCount}";
            currentJumpPower.text = $"c.jpw: {host.jumpPower}";
            currentJumpHeight.text = $"c.jhg: {host.maxJumpHeight}";
            currentSprinting.text = $"c.spr: {host.isSprinting}";
            currentFlying.text = $"c.fly: {host.isFlying}";
            positionX.text = $"x: {Mathf.RoundToInt(host.corePosition.x)}";
            positionY.text = $"y: {Mathf.RoundToInt(host.corePosition.y)}";
            positionZ.text = $"z: {Mathf.RoundToInt(host.corePosition.z)}";
        }
    }
}
