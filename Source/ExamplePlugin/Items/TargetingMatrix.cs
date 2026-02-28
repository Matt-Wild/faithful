using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class TargetingMatrix : ItemBase
    {
        // Store item and buff
        Item targetingMatrixItem;
        Buff targetingMatrixBuff;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> enableTargetEffectSetting;
        Setting<float> maxDistanceSetting;
        Setting<float> outOfRangeTimeSetting;
        Setting<float> critDamageSetting;
        Setting<float> critDamageStackingSetting;
        Setting<int> maxBuffsSetting;
        Setting<int> maxBuffsStackingSetting;

        // Store item stats
        bool enableTargetEffect;
        float maxDistance;
        float outOfRangeTime;
        float critDamage;
        float critDamageStacking;
        int maxBuffs;
        int maxBuffsStacking;

        // Constructor
        public TargetingMatrix(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("targetingmatrixdisplaymesh");

            // Create Targeting Matrix item and buff
            targetingMatrixItem = Items.AddItem("TARGETING_MATRIX", "Targeting Matrix", [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist], "textargetingmatrixicon", "targetingmatrixmesh", ItemTier.Tier2, _displaySettings: displaySettings, _modifyItemModelPrefabCallback: ModifyModelPrefab, _modifyItemDisplayPrefabCallback: ModifyModelPrefab);
            targetingMatrixBuff = Buffs.AddBuff("TARGETING_MATRIX", "Targeting Matrix", "texTargetingMatrixBuff", Color.white);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath);
            
            // Add On Incoming Damage behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);

            // Add On Inventory Changed behaviour
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);

            // Add fix for Huntress Flurry (modifies Seeking Arrow)
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += OnHuntressSeekingArrayEnter;

            // Add fix for CMD_SWARM
            On.EntityStates.Drone.Command.Headbutt.OnEnter += OnOperatorCMDSwarmEnter;
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName);

            // Check for required asset
            if (!Assets.HasAsset(_displayMeshName))
            {
                return;
            }

            // Add character display settings
            displaySettings.AddCharacterDisplay("Commando", "Head", new Vector3(-0.1025F, 0.2495F, 0.11375F), new Vector3(351.75F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Huntress", "Head", new Vector3(-0.00583F, 0.25485F, 0.08575F), new Vector3(344.5F, 0F, 0F), new Vector3(0.06F, 0.06F, 0.06F));
            displaySettings.AddCharacterDisplay("Bandit", "Head", new Vector3(-0.00457F, 0.0605F, 0.11F), new Vector3(0F, 0F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("MUL-T", "Head", new Vector3(0.4475F, 2.8375F, -0.9325F), new Vector3(305F, 180F, 0F), new Vector3(0.5F, 0.5F, 0.5F));
            displaySettings.AddCharacterDisplay("Engineer", "CannonHeadR", new Vector3(0.02F, 0.378F, 0F), new Vector3(270F, 180F, 0F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.025F, 0.575F, -0.373F), new Vector3(0F, 0F, 0F), new Vector3(0.25F, 0.25F, 0.25F), "EngiTurretBody");
            displaySettings.AddCharacterDisplay("Turret", "Head", new Vector3(-0.044F, 0.775F, 0.575F), new Vector3(0F, 0F, 0F), new Vector3(0.5F, 0.5F, 0.5F), "EngiWalkerTurretBody");
            displaySettings.AddCharacterDisplay("Artificer", "Head", new Vector3(-0.0515F, 0.0585F, 0.095F), new Vector3(0F, 355F, 0F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Mercenary", "Head", new Vector3(-0.06525F, 0.1525F, 0.1085F), new Vector3(0F, 355F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("REX", "Eye", new Vector3(-0.01875F, 0.775F, -0.0015F), new Vector3(270F, 0F, 0F), new Vector3(0.175F, 0.175F, 0.175F));
            displaySettings.AddCharacterDisplay("Loader", "MechBase", new Vector3(-0.14F, 0.41F, 0.37725F), new Vector3(0F, 0F, 0F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Acrid", "HeadCenter", new Vector3(0.9485F, 0.836F, -0.1325F), new Vector3(345F, 70F, 100F), new Vector3(1F, 1F, 1F));
            displaySettings.AddCharacterDisplay("Captain", "MuzzleGun", new Vector3(0.05965F, 0.00025F, 0.055F), new Vector3(0F, 0F, 275F), new Vector3(0.05F, 0.05F, 0.05F));
            displaySettings.AddCharacterDisplay("Railgunner", "GunScope", new Vector3(0.0165F, 0.196F, 0.33F), new Vector3(0F, 0F, 180F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Void Fiend", "Head", new Vector3(-0.0795F, 0.06825F, 0.1075F), new Vector3(327.5F, 0F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Scavenger", "Head", new Vector3(-5.255F, 3.0075F, 0.08075F), new Vector3(288.5F, 201.5F, 159.5F), new Vector3(2F, 2F, 2F));
            displaySettings.AddCharacterDisplay("Seeker", "Head", new Vector3(-0.056F, 0.08975F, 0.112F), new Vector3(0F, 353.25F, 0F), new Vector3(0.065F, 0.065F, 0.055F));
            displaySettings.AddCharacterDisplay("False Son", "Head", new Vector3(0F, 0.2175F, 0.13F), new Vector3(0F, 0F, 270F), new Vector3(0.15F, 0.15F, 0.15F));
            displaySettings.AddCharacterDisplay("Chef", "Head", new Vector3(-0.206F, 0.1105F, 0.09725F), new Vector3(270F, 90F, 0F), new Vector3(0.1F, 0.1F, 0.1F));
            displaySettings.AddCharacterDisplay("Technician", "Head", new Vector3(-0.0795F, 0.153F, 0.10375F), new Vector3(350F, 0F, 0F), new Vector3(0.09F, 0.09F, 0.1F));
            displaySettings.AddCharacterDisplay("Operator", "Head", new Vector3(-0.1224F, -0.109F, -0.141F), new Vector3(70F, 223.75F, 312.5F), new Vector3(0.075F, 0.075F, 0.075F));
            displaySettings.AddCharacterDisplay("Drifter", "Head", new Vector3(-0.1286F, 0.1905F, 0.076F), new Vector3(270F, 90F, 0F), new Vector3(0.0825F, 0.0825F, 0.0825F));
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            enableTargetEffectSetting = targetingMatrixItem.CreateSetting("ENABLE_TARGET_EFFECT", "Enable Target Visual Effect?", true, "Should the target have a visual effect?", false, true);
            maxDistanceSetting = targetingMatrixItem.CreateSetting("MAX_DISTANCE", "Max Targeting Distance", 300.0f, "How far away does the target need to be to be considered out of range? (300.0 = 300 meters)", false, _minValue: 100.0f, _valueFormatting: "{0:0}m");
            outOfRangeTimeSetting = targetingMatrixItem.CreateSetting("OUT_OF_RANGE_TIME", "Out Of Range Time", 15.0f, "How long does a target need to be out of range until it is removed from being a target? (15.0 = 15 seconds)", false, _minValue: 0.0f, _valueFormatting: "{0:0.0}s");
            critDamageSetting = targetingMatrixItem.CreateSetting("CRIT_DAMAGE", "Crit Damage", 10.0f, "How much critical damage should be provided by each stack of this item's buff with a single stack of this item? (10.0 = 10% increase)", _randomiserMin: 0.0f, _randomiserMax: 50.0f, _valueFormatting: "{0:0.0}%");
            critDamageStackingSetting = targetingMatrixItem.CreateSetting("CRIT_DAMAGE_STACKING", "Crit Damage Stacking", 5.0f, "How much should the critical damage of an individual stack of this item's buff be increased by per stack of this item? (5.0 = 5% increase)", _randomiserMin: 0.0f, _randomiserMax: 50.0f, _valueFormatting: "{0:0.0}%");
            maxBuffsSetting = targetingMatrixItem.CreateSetting("MAX_BUFFS", "Max Buffs", 3, "What's the maximum stack of this item's buff that the player should be able to receive with a single stack of this item? (3 = 3 stacks)", _minValue: 0, _randomiserMin: 0, _randomiserMax: 5);
            maxBuffsStackingSetting = targetingMatrixItem.CreateSetting("MAX_BUFFS_STACKING", "Max Buffs Stacking", 1, "How many extra stacks of this item's buff should the player be able to receive per stack? (1 = 1 stack)", _minValue: 0, _randomiserMin: 0, _randomiserMax: 3);
        }

        public override void FetchSettings()
        {
            // Get item settings
            enableTargetEffect = enableTargetEffectSetting.Value;
            maxDistance = maxDistanceSetting.Value;
            outOfRangeTime = outOfRangeTimeSetting.Value;
            critDamage = critDamageSetting.Value / 100.0f;
            critDamageStacking = critDamageStackingSetting.Value / 100.0f;
            maxBuffs = maxBuffsSetting.Value;
            maxBuffsStacking = maxBuffsStackingSetting.Value;

            // Update item texts with new settings
            targetingMatrixItem.UpdateItemTexts();
        }

        void ModifyModelPrefab(GameObject _prefab)
        {
            // Get clockwise lens
            GameObject clockwise = Utils.FindChildByName(_prefab.transform, "Lens_Clock");

            // Get anti-clockwise lens
            GameObject antiClockwise = Utils.FindChildByName(_prefab.transform, "Lens_Anti");

            // Add rotators
            FaithfulRotatorBehaviour clockwiseRotator = clockwise.AddComponent<FaithfulRotatorBehaviour>();
            FaithfulRotatorBehaviour antiClockwiseRotator = antiClockwise.AddComponent<FaithfulRotatorBehaviour>();

            // Initialise rotators
            clockwiseRotator.Init(new Vector3(0.0f, 1.0f, 0.0f), 45.0f);
            antiClockwiseRotator.Init(new Vector3(0.0f, 1.0f, 0.0f), -20.0f);
        }

        void OnCharacterDeath(DamageReport _report)
        {
            // Check for attacking character
            if (_report.attackerMaster == null) return;

            // Get attacker character master
            CharacterMaster character = _report.attackerMaster;

            // Check for body
            if (!character.hasBody) return;

            // Check for attacker inventory
            Inventory inventory = character.inventory;
            if (inventory == null) return;

            // Get count for item
            int count = inventory.GetItemCountEffective(targetingMatrixItem.itemDef);

            // Check for item
            if (count == 0) return;

            // Get attacker character body
            CharacterBody attackerBody = character.GetBody();

            // Get faithful behaviour for attacker
            FaithfulCharacterBodyBehaviour characterBehaviour = Utils.FindCharacterBodyHelper(attackerBody);
            if (characterBehaviour == null) return;

            // Get targeting matrix behaviour
            FaithfulTargetingMatrixBehaviour targetingMatrixBehaviour = characterBehaviour.targetingMatrix;
            if (targetingMatrixBehaviour == null) return;

            // Check if victim is attacker target
            if (targetingMatrixBehaviour.target == _report.victimBody)
            {
                // Calculate max buff stack for targeting matrix
                int maxBuffStack = maxBuffs + (maxBuffsStacking * (count - 1));

                // Get current buff count
                int currentBuffCount = attackerBody.GetBuffCount(targetingMatrixBuff.buffDef);

                // Check if can add buff
                if (currentBuffCount < maxBuffStack)
                {
                    // Add buff
                    attackerBody.AddBuff(targetingMatrixBuff.buffDef);
                }
            }

            // Do on kill behaviour
            targetingMatrixBehaviour.OnKill(_report.victimBody);
        }

        void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check for attacker and victim
            if (_attacker == null || _attacker.inventory == null || _victim == null) return;

            // Get attacker character body
            CharacterBody attacker = _attacker.GetBody();

            // Get attacker item count
            int attackerItemCount = _attacker.inventory.GetItemCountEffective(targetingMatrixItem.itemDef);

            // Get attacker buff count
            int attackerBuffCount = attacker.GetBuffCount(targetingMatrixBuff.buffDef);

            // Calculate crit damage increase from buffs
            float critDamageIncrease = attackerBuffCount == 0 ? 0.0f : (critDamage + critDamageStacking * Mathf.Max(attackerItemCount - 1, 0)) * attackerBuffCount;

            // Check if targeting matrix should be activated
            if (!GetMatrixActivated(_report, attacker, _victim.GetBody()))
            {
                // If crit and attacker has buff, increase crit damage
                if (_report.crit && attackerBuffCount > 0)
                {
                    // Increase crit damage
                    _report.damage *= (attacker.critMultiplier + critDamageIncrease) / attacker.critMultiplier;
                }

                return;
            }

            // Check if railgunner
            if (attacker?.modelLocator?.modelTransform != null && attacker.modelLocator.modelTransform && attacker.modelLocator.modelTransform.name == "mdlRailGunner")
            {
                // Check if crit
                if (_report.crit)
                {
                    // Increase non-crit damage (+1 for 100% crit chance)
                    _report.damage *= (attacker.critMultiplier + 1.0f + critDamageIncrease) / attacker.critMultiplier;
                }
            }

            // Not railgunner
            else
            {
                // Always crit target
                _report.crit = true;

                // Increase crit damage
                _report.damage *= (attacker.critMultiplier + critDamageIncrease) / attacker.critMultiplier;
            }
        }

        void OnInventoryChanged(Inventory _inventory)
        {
            if (_inventory == null) return;

            // Get character body for inventory
            CharacterBody characterBody = Utils.GetInventoryBody(_inventory);
            if (characterBody == null) return;

            // Get item and buff count
            int itemCount = _inventory.GetItemCountEffective(targetingMatrixItem.itemDef);
            int buffCount = characterBody.GetBuffCount(targetingMatrixBuff.buffDef);

            // Calculate max buff stack for targeting matrix
            int maxBuffStack = itemCount == 0 ? 0 : maxBuffs + (maxBuffsStacking * (itemCount - 1));

            // Check if buff count exceeds max buff stack
            if (buffCount > maxBuffStack)
            {
                // Remove excess buffs
                characterBody.SetBuffCount(targetingMatrixBuff.buffDef.buffIndex, maxBuffStack);
            }

            // Check if item count is zero
            if (itemCount == 0)
            {
                // Attempt to get faithful behaviour for character body
                FaithfulCharacterBodyBehaviour characterBehaviour = Utils.FindCharacterBodyHelper(characterBody);
                if (characterBehaviour == null) return;

                // Attempt to get targeting matrix behaviour
                FaithfulTargetingMatrixBehaviour targetingMatrixBehaviour = characterBehaviour.targetingMatrix;
                if (targetingMatrixBehaviour == null) return;

                // Remove target
                targetingMatrixBehaviour.RemoveTarget();
            }
        }

        void OnHuntressSeekingArrayEnter(On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.orig_OnEnter orig, EntityStates.Huntress.HuntressWeapon.FireSeekingArrow self)
        {
            // Run original process
            orig(self);

            // Check for victim
            if (self.initialOrbTarget == null) return;

            // Check for victim health component
            if (self.initialOrbTarget.healthComponent == null) return;

            // Check for victim body
            CharacterBody victim = self.initialOrbTarget.healthComponent.body;
            if (victim == null) return;

            // Check for attacker body
            CharacterBody attacker = self.characterBody;
            if (attacker == null) return;

            // Check if targeting matrix should be activated
            if (!GetMatrixActivated(null, attacker, victim)) return;

            // Set to crit
            self.isCrit = true;
        }

        private void OnOperatorCMDSwarmEnter(On.EntityStates.Drone.Command.Headbutt.orig_OnEnter orig, EntityStates.Drone.Command.Headbutt self)
        {
            // Get victim and attacker
            CharacterBody victim = self.target?.healthComponent?.body;
            CharacterBody attacker = GetSwarmDroneLeaderBody(self);

            // Check if matrix should activate
            if (victim != null && attacker != null && GetMatrixActivated(null, attacker, victim))
            {
                // Override crit
                Behaviour.OverrideCritCheck(attacker);
            }

            // Run original process
            orig(self);
        }

        CharacterBody GetSwarmDroneLeaderBody(EntityStates.Drone.Command.Headbutt _drone)
        {
            // Attempt to fetch via command receiver
            CharacterBody leader = _drone.commandReceiver?.leaderBody;
            if (leader) return leader;

            // Attempt to fetch via minion ownership
            CharacterMaster droneMaster = _drone.characterBody?.master;
            CharacterMaster ownerMaster = droneMaster?.minionOwnership?.ownerMaster;
            return ownerMaster ? ownerMaster.GetBody() : null;
        }

        bool GetMatrixActivated(DamageInfo _report, CharacterBody _attacker, CharacterBody _victim)
        {
            // REPORT CAN BE NULL

            // Check for attacker and victim
            if (_attacker == null || _victim == null) return false;

            // Do not effect DoTs
            if (_report != null && _report.dotIndex != DotController.DotIndex.None) return false;

            // Check for attacker inventory
            if (!_attacker.inventory) return false;

            // Get item count
            int count = _attacker.inventory.GetItemCountEffective(targetingMatrixItem.itemDef);

            // Check for item
            if (count == 0) return false;

            // Get faithful behaviour for attacker
            FaithfulCharacterBodyBehaviour characterBehaviour = Utils.FindCharacterBodyHelper(_attacker);
            if (characterBehaviour == null) return false;

            // Get targeting matrix behaviour
            FaithfulTargetingMatrixBehaviour targetingMatrixBehaviour = characterBehaviour.targetingMatrix;
            if (targetingMatrixBehaviour == null) return false;

            // Check if targeting matrix target is victim
            if (targetingMatrixBehaviour.target != _victim) return false;

            // Damage should be effected by targeting matrix
            return true;
        }
    }
}
