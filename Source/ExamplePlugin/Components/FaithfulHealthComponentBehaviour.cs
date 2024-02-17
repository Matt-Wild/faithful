using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulHealthComponentBehaviour : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        // Store reference to Toolbox
        public Toolbox toolbox;

        // Store reference to Character Body
        public CharacterBody character;

        // Store last attacker
        public CharacterMaster lastAttacker;

        private void Start()
        {
            // Get Character Body
            character = gameObject.GetComponent<CharacterBody>();
        }

        public void OnIncomingDamageServer(DamageInfo _damageInfo)
        {
            // Reset last attacker
            lastAttacker = null;

            // Check for damage info
            if (_damageInfo == null)
            {
                return;
            }

            // Get victim
            CharacterMaster victim = character ? character.master : null;

            // Get attacker
            CharacterMaster attacker = null;
            if (_damageInfo.attacker)
            {
                // Check for character body
                CharacterBody attackerBody = _damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    // Get attacker character master
                    attacker = attackerBody.master;

                    // Update last attacker
                    lastAttacker = attacker;
                }
            }

            // Send request for behaviour to handle
            toolbox.behaviour.OnIncomingDamageServer(_damageInfo, attacker, victim);
        }
    }
}
