using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful
{
    internal class TargetingMatrix : ItemBase
    {
        // Store item
        Item targetingMatrixItem;

        // Store display settings
        ItemDisplaySettings displaySettings;

        // Store additional item settings
        Setting<bool> enableTargetEffectSetting;
        Setting<float> damageSetting;
        Setting<float> damageStackingSetting;
        Setting<float> maxDistanceSetting;
        Setting<float> closeDistanceSetting;
        Setting<float> preferredDistanceSetting;
        Setting<float> outOfRangeTimeSetting;

        // Store item stats
        bool enableTargetEffect;
        float damage;
        float damageStacking;
        float maxDistance;
        float closeDistance;
        float preferredDistance;
        float outOfRangeTime;

        // Constructor
        public TargetingMatrix(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("targetingmatrixdisplaymesh");

            // Create Copper Gear item and buff
            targetingMatrixItem = Items.AddItem("TARGETING_MATRIX", [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist], "textargetingmatrixicon", "targetingmatrixmesh", _displaySettings: displaySettings, _modifyItemModelPrefabCallback: ModifyModelPrefab, _modifyItemDisplayPrefabCallback: ModifyModelPrefab);

            // Create item settings
            CreateSettings();

            // Fetch item settings
            FetchSettings();

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath);
            
            // Add On Incoming Damage behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);

            // Add fix for Huntress Flurry (modifies Seeking Arrow)
            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += OnHuntressSeekingArrayEnter;
        }

        private void CreateDisplaySettings(string _displayMeshName)
        {
            // Create display settings
            displaySettings = Utils.CreateItemDisplaySettings(_displayMeshName, _useHopooShader: false);

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
            displaySettings.AddCharacterDisplay("Turret", "Neck", new Vector3(0F, 0.325F, -0.20525F), new Vector3(0F, 180F, 0F), new Vector3(0.25F, 0.25F, 0.25F));
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
        }

        protected override void CreateSettings()
        {
            // Create settings specific to this item
            enableTargetEffectSetting = targetingMatrixItem.CreateSetting("ENABLE_TARGET_EFFECT", "Enable Target Visual Effect?", true, "Should the target have a visual effect?", false, true);
            damageSetting = targetingMatrixItem.CreateSetting("DAMAGE", "Damage", 0.0f, "How much should the first stack of this item increase damage dealt to target? (0.0 = 0% increase)", _randomiserMin: 0.0f, _randomiserMax: 50.0f);
            damageStackingSetting = targetingMatrixItem.CreateSetting("DAMAGE_STACKING", "Damage Stacking", 25.0f, "How much should further stacks of this item increase damage dealt to target? (25.0 = 25% increase)");
            maxDistanceSetting = targetingMatrixItem.CreateSetting("MAX_DISTANCE", "Max Targeting Distance", 300.0f, "How far away does the target need to be to be considered out of range? (300.0 = 300 meters)", false, _minValue: 100.0f);
            closeDistanceSetting = targetingMatrixItem.CreateSetting("CLOSE_DISTANCE", "Close Targeting Distance", 120.0f, "How close does the target need to be to be prioritised by target selection? (120.0 = 120 meters)", false, _minValue: 0.0f);
            preferredDistanceSetting = targetingMatrixItem.CreateSetting("PREFERRED_DISTANCE", "Preferred Targeting Distance", 30.0f, "How close does the target need to be to the previous target to be preferred by target selection? (30.0 = 30 meters)", false, _minValue: 0.0f);
            outOfRangeTimeSetting = targetingMatrixItem.CreateSetting("OUT_OF_RANGE_TIME", "Out Of Range Time", 15.0f, "How long does a target need to be out of range until it is removed from being a target? (15.0 = 15 seconds)", false, _minValue: 0.0f);
        }

        public override void FetchSettings()
        {
            // Get item settings
            enableTargetEffect = enableTargetEffectSetting.Value;
            damage = damageSetting.Value / 100.0f;
            damageStacking = damageStackingSetting.Value / 100.0f;
            maxDistance = maxDistanceSetting.Value;
            closeDistance = closeDistanceSetting.Value;
            preferredDistance = preferredDistanceSetting.Value;
            outOfRangeTime = outOfRangeTimeSetting.Value;

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
            int count = inventory.GetItemCount(targetingMatrixItem.itemDef);

            // Check for item
            if (count == 0) return;

            // Get faithful behaviour for attacker
            FaithfulCharacterBodyBehaviour characterBehaviour = Utils.FindCharacterBodyHelper(character.GetBody());
            if (characterBehaviour == null) return;

            // Get targeting matrix behaviour
            FaithfulTargetingMatrixBehaviour targetingMatrixBehaviour = characterBehaviour.targetingMatrix;
            if (targetingMatrixBehaviour == null) return;

            // Do on kill behaviour
            targetingMatrixBehaviour.OnKill(_report.victimBody);
        }

        void OnIncomingDamage(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Check for attacker and victim
            if (_attacker == null || _victim == null) return;

            // Get attacker character body
            CharacterBody attacker = _attacker.GetBody();

            // Check if targeting matrix should be activated
            if (!GetMatrixActivated(_report, attacker, _victim.GetBody())) return;

            // Get item count
            int count = _attacker.inventory.GetItemCount(targetingMatrixItem.itemDef);

            // Check if railgunner
            if (attacker?.modelLocator?.modelTransform != null && attacker.modelLocator.modelTransform && attacker.modelLocator.modelTransform.name == "mdlRailGunner")
            {
                // Check if crit
                if (_report.crit)
                {
                    // Increase damage
                    _report.damage *= (attacker.critMultiplier + 1.0f) / attacker.critMultiplier;
                }
            }

            // Not railgunner
            else
            {
                // Always crit target
                _report.crit = true;
            }

            // Increase damage
            _report.damage *= 1.0f + damage + (damageStacking * (count - 1));
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
            int count = _attacker.inventory.GetItemCount(targetingMatrixItem.itemDef);

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
