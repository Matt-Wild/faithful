using R2API;
using RoR2;
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

        // Constructor
        public TargetingMatrix(Toolbox _toolbox) : base(_toolbox)
        {
            // Create display settings
            CreateDisplaySettings("targetingmatrixdisplaymesh");

            // Create Copper Gear item and buff
            targetingMatrixItem = Items.AddItem("TARGETING_MATRIX", [ItemTag.Damage, ItemTag.OnKillEffect, ItemTag.AIBlacklist], "textargetingmatrixicon", "targetingmatrixmesh", _displaySettings: displaySettings);

            // Link On Character Death behaviour
            Behaviour.AddOnCharacterDeathCallback(OnCharacterDeath);

            // Add On Incoming Damage behaviour
            Behaviour.AddOnIncomingDamageCallback(OnIncomingDamage);
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
            // displaySettings.AddCharacterDisplay("Turret", Why don't they have different mdl names?!);
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

        void OnCharacterDeath(DamageReport _report)
        {
            // Check for attacking character
            if (_report.attackerMaster == null) return;

            // Check for victim character
            if (_report.victimBody == null || _report.victimMaster == null) return;

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

            // Check for attacker and victim bodies
            if (!_attacker.hasBody || !_victim.hasBody) return;

            // Get attacker and victim bodies
            CharacterBody attackerBody = _attacker.GetBody();
            CharacterBody victimBody = _victim.GetBody();

            // Check for attacker inventory
            if (!attackerBody.inventory) return;

            // Get item count
            int count = attackerBody.inventory.GetItemCount(targetingMatrixItem.itemDef);

            // Check for item
            if (count == 0) return;

            // Get faithful behaviour for attacker
            FaithfulCharacterBodyBehaviour characterBehaviour = Utils.FindCharacterBodyHelper(attackerBody);
            if (characterBehaviour == null) return;

            // Get targeting matrix behaviour
            FaithfulTargetingMatrixBehaviour targetingMatrixBehaviour = characterBehaviour.targetingMatrix;
            if (targetingMatrixBehaviour == null) return;

            // Check if targeting matrix target is victim
            if (targetingMatrixBehaviour.target != victimBody) return;

            // Always crit target
            _report.crit = true;

            // Increase damage
            _report.damage *= 1.0f + (0.25f * (count - 1));
        }
    }
}
