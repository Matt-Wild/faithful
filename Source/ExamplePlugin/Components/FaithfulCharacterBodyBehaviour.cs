using EntityStates;
using Mono.CompilerServices.SymbolWriter;
using RoR2;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore.Text;

namespace Faithful
{
    internal class FaithfulCharacterBodyBehaviour : NetworkBehaviour
    {
        // Store flags
        public Flags frameFlags = new Flags();
        public Flags stageFlags = new Flags();

        // Store CharacterBody net ID
        [SyncVar(hook = nameof(CharacterIDChanged))] public NetworkInstanceId characterID;

        // Store reference to Character Body
        private CharacterBody character;

        // Store if character body has been found
        private bool characterBodyFound = false;

        // Store reference to behaviours
        public FaithfulTJetpackBehaviour tJetpack;
        public FaithfulLeadersPennonBehaviour leadersPennon;
        public FaithfulTargetingMatrixBehaviour targetingMatrix;
        public FaithfulHermitsShawlBehaviour hermitsShawl;

        // Store if searching for character body
        private bool searchingForCharacterBody = false;

        private void CharacterIDChanged(NetworkInstanceId _newValue)
        {
            // Ensure new value is set
            characterID = _newValue;

            // Start coroutine to find and link character body
            StartCoroutine(LinkCharacterBody());
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Start coroutine to find and link character body
            StartCoroutine(LinkCharacterBody());

            // Check for character body on delay
            Invoke("CheckForCharacterBody", 8.0f);
        }

        private void CheckForCharacterBody()
        {
            // Check if debug mode, not the server and no character body
            if (!characterBodyFound && !NetworkServer.active && Utils.debugMode)
            {
                // Send warning
                Log.Warning($"Faithful Character Body behaviour not linked for net ID {characterID}.");
            }
        }

        private void LateUpdate()
        {
            // Reset flags
            frameFlags.Reset();

            // Check if character body is destroyed
            if (characterBodyFound && character == null)
            {
                // Check if server
                if (NetworkServer.active)
                {
                    // Destroy faithful behaviour
                    NetworkServer.Destroy(gameObject);
                }
            }
        }

        private IEnumerator LinkCharacterBody()
        {
            // Only attempt to link if not already attempting to link
            if (!searchingForCharacterBody)
            {
                // Beginning search
                searchingForCharacterBody = true;

                // Cycle until character body is found
                while (character == null)
                {
                    // Attempt to find character body
                    character = ClientScene.FindLocalObject(characterID)?.GetComponent<CharacterBody>();
                    yield return null;
                }

                // Set as character body found
                characterBodyFound = true;

                // Register behaviour with utils
                Utils.RegisterFaithfulCharacterBodyBehaviour(character, this);

                // Fetch TJetpack behaviour
                tJetpack = GetComponent<FaithfulTJetpackBehaviour>();

                // Assign character
                tJetpack.AssignCharacter(character);

                // Fetch leaders pennon behaviour
                leadersPennon = GetComponent<FaithfulLeadersPennonBehaviour>();

                // Initialise leaders pennon behaviour
                leadersPennon.Init(character);

                // Fetch targeting matrix behaviour
                targetingMatrix = GetComponent<FaithfulTargetingMatrixBehaviour>();

                // Initialise targeting matrix behaviour
                targetingMatrix.Init(character);

                // Fetch hermits shawl behaviour
                hermitsShawl = GetComponent<FaithfulHermitsShawlBehaviour>();

                // Initialise hermits shawl behaviour
                hermitsShawl.Init(character);

                // Check for inventory
                if (character.inventory != null)
                {
                    // Update jetpack with jetpack item count
                    tJetpack.UpdateItemCount(character.inventory.GetItemCount(Items.GetItem("4T0N_JETPACK").itemDef));
                }

                // Check if debug mode
                if (Utils.debugMode)
                {
                    // Get string for if client or server
                    string messageSource = NetworkServer.active ? "SERVER" : "CLIENT";

                    Log.Message($"[{messageSource}] - Faithful Character Body behaviour linked for character '{character.name}' with net ID {characterID}.");
                }

                // Done searching
                searchingForCharacterBody = false;
            }
        }
    }

    internal interface ICharacterBehaviour
    {
        public void FetchSettings();
    }
}
