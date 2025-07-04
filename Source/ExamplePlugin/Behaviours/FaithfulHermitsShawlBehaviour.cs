﻿using EntityStates;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Faithful
{
    internal class FaithfulHermitsShawlBehaviour : MonoBehaviour, ICharacterBehaviour
    {
        // Store reference to Character Body
        public CharacterBody character;

        // Hermit shawl item
        Item m_hermitsShawlItem;

        // Patience buff
        Buff patienceBuff;

        // How many hermit shawl items this character has
        int itemCount = -1;

        // Is behaviour hooked for this character?
        bool hooked = false;

        // Is this character out of combat
        bool ooc = false;

        // if this character is forced into combat until in combat normally (useful because out of combat is sometimes delayed)
        bool forcedInCombat = false;

        // Store item stats
        int maxBuffs;
        int maxBuffsStacking;
        float buffCooldown;

        public FaithfulHermitsShawlBehaviour()
        {
            // Register with utils
            Utils.RegisterCharacterBehaviour(this);
        }

        public void Init(CharacterBody _character)
        {
            // Assign character
            character = _character;

            // Get patience buff
            patienceBuff = Buffs.GetBuff("PATIENCE");

            // Fetch leader's pennon settings
            FetchSettings();

            // Check for inventory
            Inventory inventory = character.inventory;
            if (inventory != null)
            {
                // Update item count with current item amount
                UpdateItemCount(inventory.GetItemCount(hermitsShawlItem.itemDef));
            }

            // Hook behaviour
            Behaviour.AddOnItemAddedCallback(hermitsShawlItem.itemDef, OnItemAdded);

            // Update temporary visual effects
            character.UpdateAllTemporaryVisualEffects();
        }

        public void FetchSettings()
        {
            // Check for null item
            if (hermitsShawlItem == null) return;

            // Update stats
            maxBuffsStacking = hermitsShawlItem.FetchSetting<int>("MAX_BUFFS_STACKING").Value;
            maxBuffs = hermitsShawlItem.FetchSetting<int>("MAX_BUFFS").Value;
            buffCooldown = hermitsShawlItem.FetchSetting<float>("BUFF_RECHARGE").Value;
        }

        private void OnDestroy()
        {
            // Unregister with utils
            Utils.UnregisterCharacterBehaviour(this);

            // Unhook behaviour
            Behaviour.RemoveOnItemAddedCallback(hermitsShawlItem.itemDef, OnItemAdded);  // This is an additional hook
            UnhookBehaviour();
        }

        private void OnItemAdded(Inventory _inventory)
        {
            // Ignore if behaviour already hooked
            if (hooked) return;

            // Get character body for inventory
            CharacterBody characterBody = Utils.GetInventoryBody(_inventory);
            if (characterBody == null) return;

            // Check if for this character
            if (characterBody != character) return;

            // Get item count
            int itemCount = _inventory.GetItemCount(hermitsShawlItem.itemDef);

            // Update item count
            UpdateItemCount(itemCount);
        }

        private void HookBehaviour()
        {
            // Skip if already hooked
            if (hooked) return;

            // Update hooked flag
            hooked = true;

            // Hook behaviour
            Behaviour.AddOnInventoryChangedCallback(OnInventoryChanged);
            Behaviour.AddOnCharacterBodyFixedUpdateCallback(OnCharacterBodyFixedUpdate);
        }

        private void UnhookBehaviour()
        {
            // Skip if already not hooked
            if (!hooked) return;

            // Update hooked flag
            hooked = false;

            // Unhook behaviour
            Behaviour.RemoveOnInventoryChangedCallback(OnInventoryChanged);
            Behaviour.RemoveOnCharacterBodyFixedUpdateCallback(OnCharacterBodyFixedUpdate);
        }

        protected void OnInventoryChanged(Inventory _inventory)
        {
            // Attempt to get Character Body
            CharacterBody body = Utils.GetInventoryBody(_inventory);
            if (body == null)
            {
                return;
            }

            // Check if for this character body
            if (body != character) return;

            // Get new item count
            int newCount = _inventory.GetItemCount(hermitsShawlItem.itemDef);

            // Update item count
            UpdateItemCount(newCount);
        }

        private void OnCharacterBodyFixedUpdate(CharacterBody _body)
        {
            // Ignore if not this character body
            if (_body != character) return;

            // Ignore if not hosting
            if (!Utils.hosting) return;

            // Ignore if doesn't have the item
            if (itemCount < 1) return;

            // Update out of combat behaviour
            UpdateOutOfCombat();
        }

        void UpdateOutOfCombat()
        {
            // Check if currently out of combat
            bool curreentOOC = character.outOfCombat && !forcedInCombat;

            // Check if out of combat update needed
            if (curreentOOC != ooc)
            {
                // Update out of combat
                ooc = curreentOOC;

                // Check if out of combat
                if (ooc)
                {
                    // Get current buff count
                    int buffCount = character.GetBuffCount(patienceBuff.buffDef);

                    // Check if buff count should increase
                    if (buffCount < currentMaxBuffs)
                    {
                        // Add buff
                        character.AddBuff(patienceBuff.buffDef);
                    }

                    // Provide another buff on a delay
                    Invoke("DelayedBuff", currentBuffCooldown);
                }
            }
        }

        public void ForceIntoCombat()
        {
            // Cancel previous invokes for allow out of combat
            CancelInvoke("AllowOutOfCombat");

            // Set as forced into combat
            forcedInCombat = true;

            // Allow out of combat after 5 seconds
            Invoke("AllowOutOfCombat", 5);
        }

        void AllowOutOfCombat()
        {
            // Set as not forced into combat
            forcedInCombat = false;
        }

        private void DelayedBuff()
        {
            // Ignore if in combat
            if (!ooc) return;

            // Get current buff count
            int buffCount = character.GetBuffCount(patienceBuff.buffDef);

            // Check if buff count should increase
            if (buffCount < currentMaxBuffs)
            {
                // Add buff
                character.AddBuff(patienceBuff.buffDef);
            }

            // Repeat
            Invoke("DelayedBuff", currentBuffCooldown);
        }

        private void UpdateItemCount(int _newCount)
        {
            // Check if count already calculated
            if (_newCount == itemCount) return;

            // Update item count
            itemCount = _newCount;

            // Hook if not hooked and item count is above 0
            if (!hooked && itemCount > 0)
            {
                // Hook behaviour
                HookBehaviour();
            }
        }

        Item hermitsShawlItem
        {
            get
            {
                // Check for Hermit's Shawl
                if (m_hermitsShawlItem == null)
                {
                    // Fetch hermit shawl item
                    m_hermitsShawlItem = Items.GetItem("HERMITS_SHAWL");
                }

                // Return Hermit's Shawl
                return m_hermitsShawlItem;
            }
        }

        int currentMaxBuffs
        {
            get
            {
                // Calculate max buffs
                return maxBuffs + maxBuffsStacking * (itemCount - 1);
            }
        }

        float currentBuffCooldown
        {
            get
            {
                // Get max buffs
                int _currentMaxBuffs = currentMaxBuffs;

                // Calculate current buff cooldown
                return buffCooldown / (_currentMaxBuffs > 1 ? (_currentMaxBuffs - 1) : 1);
            }
        }
    }
}
