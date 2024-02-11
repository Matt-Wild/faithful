using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulHealthComponentBehaviour : MonoBehaviour, IOnIncomingDamageServerReceiver
    {
        // Store reference to Behaviour
        public Behaviour behaviour;

        public void OnIncomingDamageServer(DamageInfo _damageInfo)
        {
            // Get victim
            CharacterBody victimBody = GetComponent<CharacterBody>();
            CharacterMaster victim = victimBody ? victimBody.master : null;

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
                }
            }

            // Send request for behaviour to handle
            behaviour.OnIncomingDamageServer(_damageInfo, attacker, victim);
        }
    }
}
