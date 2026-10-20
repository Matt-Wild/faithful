using EntityStates;
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
                UpdateItemCount(inventory.GetItemCountEffective(hermitsShawlItem.itemDef));
            }

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
        }

        public void OnInventoryChanged(Inventory _inventory)
        {
            // Get new item count
            int newCount = _inventory.GetItemCountEffective(hermitsShawlItem.itemDef);

            // Update item count
            UpdateItemCount(newCount);
        }

        public void OnCharacterBodyFixedUpdate()
        {
            // Ignore if character body no longer exists
            if (character == null) return;

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
                return Utils.CalculateStackingValue(itemCount, maxBuffs, maxBuffsStacking);
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
