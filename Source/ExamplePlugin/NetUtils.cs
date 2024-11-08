using BepInEx;
using HarmonyLib;
using R2API;
using RoR2;
using RoR2.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class NetUtils : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();

            // Check if this instance is for the local player and has authority
            StartCoroutine(CheckForAuthority());
        }

        protected IEnumerator CheckForAuthority()
        {
            // Define local character master
            CharacterMaster localCharacterMaster = null;

            // Fetch local character master
            while (localCharacterMaster == null)
            {
                // Attempt to fetch
                localCharacterMaster = Utils.localPlayer;
                yield return null;
            }

            // Define character master for this instance
            CharacterMaster instanceCharacterMaster = null;

            // Fetch instance character master
            while (instanceCharacterMaster == null)
            {
                // Attempt to fetch
                instanceCharacterMaster = GetComponent<CharacterMaster>();
                yield return null;
            }

            // Check if local and instance match
            if (localCharacterMaster == instanceCharacterMaster)
            {
                // Register net utils with utils
                Utils.netUtils = this;

                // Check if debug mode
                if (Utils.debugMode)
                {
                    // Log that local player net utils has been created and linked
                    Debug.Log($"[NET UTILS] - Net Utils created for local player.");
                }
            }
        }

        public void LogMessage(string _message)
        {
            // Check if server
            if (NetworkServer.active)
            {
                // Log message
                RpcLogMessage(_message);
            }
            else
            {
                // Tell server to tell clients to log message
                CmdLogMessage(_message);
            }
        }

        [Command]
        private void CmdLogMessage(string _message)
        {
            // Log message on all clients
            RpcLogMessage(_message);
        }

        [ClientRpc]
        private void RpcLogMessage(string _message)
        {
            // Log message
            Log.Debug(_message);
        }

        public void SyncJetpack(NetworkInstanceId _netID, JetpackSyncData _data)
        {
            // Ask server to sync jetpack
            CmdSyncJetpack(_netID, _data);
        }

        [Command]
        private void CmdSyncJetpack(NetworkInstanceId _netID, JetpackSyncData _data)
        {
            // Fetch jetpack to sync
            if (NetworkServer.objects.TryGetValue(_netID, out NetworkIdentity networkIdentity))
            {
                // Attempt to get jetpack behaviour
                FaithfulTJetpackBehaviour jetpackBehaviour = networkIdentity.gameObject.GetComponent<FaithfulTJetpackBehaviour>();
                if (jetpackBehaviour != null)
                {
                    // Call command to sync the jetpack
                    jetpackBehaviour.CmdSyncJetpack(_data);
                }
            }
        }
    }
}
