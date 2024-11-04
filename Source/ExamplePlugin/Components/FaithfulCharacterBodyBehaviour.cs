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

        // Store reference to behaviours
        public FaithfulTJetpackBehaviour tJetpack;

        /*[Command]
        public void CmdSetCharacterID(NetworkInstanceId _characterID)
        {
            // Set character body network ID
            characterID = _characterID;

            // Notify clients via RPC
            //RpcUpdateCharacterID(_characterID);

            // Start coroutine to find and link character body
            //StartCoroutine(LinkCharacterBody(true));
        }*/

        /*[ClientRpc]
        private void RpcUpdateCharacterID(NetworkInstanceId _value)
        {
            // Update character body net ID
            characterID = _value;

            // Start coroutine to find and link character body
            StartCoroutine(LinkCharacterBody());
        }*/

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
        }

        private void LateUpdate()
        {
            // Reset flags
            frameFlags.Reset();
        }

        private IEnumerator LinkCharacterBody()
        {
            // Cycle until character body is found
            while (character == null)
            {
                // Attempt to find character body
                character = ClientScene.FindLocalObject(characterID)?.GetComponent<CharacterBody>();
                yield return null;
            }

            // Register behaviour with utils
            Utils.RegisterFaithfulCharacterBodyBehaviour(character, this);

            // Create TJetpack behaviour
            tJetpack = new FaithfulTJetpackBehaviour(character);

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

                Log.Message($"[{messageSource}] - Faithful Character Body behaviour linked for character '{character.name}' with net ID {characterID}");
            }
        }
    }
}
