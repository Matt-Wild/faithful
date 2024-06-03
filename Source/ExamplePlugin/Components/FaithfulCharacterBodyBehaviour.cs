using EntityStates;
using RoR2;
using System;
using UnityEngine;

namespace Faithful
{
    internal class FaithfulCharacterBodyBehaviour : MonoBehaviour
    {
        // Store flags
        public Flags frameFlags = new Flags();
        public Flags stageFlags = new Flags();

        // Store reference to Character Body
        private CharacterBody character;

        // Store reference to behaviours
        public FaithfulTJetpackBehaviour tJetpack;

        private void Awake()
        {
            // Get Character Body
            character = gameObject.GetComponent<CharacterBody>();

            // Create TJetpack behaviour
            tJetpack = new FaithfulTJetpackBehaviour(character);
        }

        private void LateUpdate()
        {
            // Reset flags
            frameFlags.Reset();
        }
    }
}
