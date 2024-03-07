using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugStatsMonitor : MonoBehaviour
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
        protected Text currentDamage;
        protected Text currentAttackSpeed;
        protected Text currentCrit;
        protected Text currentSpeed;
        protected Text currentAcceleration;
        protected Text currentJumpCount;
        protected Text currentJumpPower;

        void Awake()
        {
            // Get host
            host = PlayerCharacterMasterController.instances[0].master.GetBody();

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
            currentDamage = transform.Find("CurrentDamageText").GetComponent<Text>();
            currentAttackSpeed = transform.Find("CurrentAttackSpeedText").GetComponent<Text>();
            currentCrit = transform.Find("CurrentCritText").GetComponent<Text>();
            currentSpeed = transform.Find("CurrentSpeedText").GetComponent<Text>();
            currentAcceleration = transform.Find("CurrentAccelerationText").GetComponent<Text>();
            currentJumpCount = transform.Find("CurrentJumpCountText").GetComponent<Text>();
            currentJumpPower = transform.Find("CurrentJumpPowerText").GetComponent<Text>();
        }

        void FixedUpdate()
        {
            // Update stat texts
            baseHealth.text = $"b.hp: {host.baseMaxHealth}";
            baseRegen.text = $"b.rgn: {host.baseRegen}";
            baseArmour.text = $"b.arm: {host.baseArmor}";
            baseShield.text = $"b.shd: {host.baseMaxShield}";
            baseDamage.text = $"b.dmg: {host.baseDamage}";
            baseAttackSpeed.text = $"b.asp: {host.baseAttackSpeed}";
            baseCrit.text = $"b.crt: {host.baseCrit}";
            baseSpeed.text = $"b.spd: {host.baseMoveSpeed}";
            baseAcceleration.text = $"b.acc: {host.baseAcceleration}";
            baseJumpCount.text = $"b.jcn: {host.baseJumpCount}";
            baseJumpPower.text = $"b.jpw: {host.baseJumpPower}";
            currentHealth.text = $"c.hp: {host.maxHealth}";
            currentRegen.text = $"c.rgn: {host.regen}";
            currentArmour.text = $"c.arm: {host.armor}";
            currentShield.text = $"c.shd: {host.maxShield}";
            currentDamage.text = $"c.dmg: {host.damage}";
            currentAttackSpeed.text = $"c.asp: {host.attackSpeed}";
            currentCrit.text = $"c.crt: {host.crit}";
            currentSpeed.text = $"c.spd: {host.moveSpeed}";
            currentAcceleration.text = $"c.acc: {host.acceleration}";
            currentJumpCount.text = $"c.jcn: {host.maxJumpCount}";
            currentJumpPower.text = $"c.jpw: {host.jumpPower}";
        }
    }
}
