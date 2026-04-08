using RoR2;
using System.Collections;
using System.Collections.Generic;
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

                // Check if not hosting
                if (!Utils.hosting)
                {
                    // Sync settings with host
                    StartCoroutine(SyncSettingsWithHost());
                }

                // Check if verbose console
                if (Utils.verboseConsole)
                {
                    // Log that local player net utils has been created and linked
                    Debug.Log($"[NET UTILS] - Net Utils created for local player.");
                }
            }
        }

        private static IEnumerator SyncSettingsWithHost()
        {
            // Create list of settings that currently exist
            List<ISetting> currentSettings = new List<ISetting>();

            // Cycle through settings
            foreach (KeyValuePair<string, ISetting> pair in Config.GetSettings())
            {
                // Add setting to current settings
                currentSettings.Add(pair.Value);
            }

            Log.Info($"[CONFIG SYNC] - Syncing {currentSettings.Count} settings.");

            // Cycle through settings
            foreach (ISetting setting in currentSettings)
            {
                // Tell setting to sync
                setting.Sync();
            }

            // Store if all settings have synced
            bool synced = false;

            // Cycle until all settings have synced
            while (!synced)
            {
                // Assume has synced
                synced = true;

                // Cycle through settings
                foreach (ISetting setting in currentSettings)
                {
                    // Check if setting has not synced yet
                    if (!setting.isSynced)
                    {
                        // Set as not synced
                        synced = false;
                        break;
                    }
                }

                // Go to next frame
                yield return null;
            }

            Log.Info($"[CONFIG SYNC] - Synced {currentSettings.Count} settings.");

            // Refresh all item settings
            Utils.RefreshItemSettings();
        }

        public void SyncSetting(ISetting _setting)
        {
            // Ask server to sync setting
            CmdSyncSetting(_setting.token);
        }

        [Command]
        private void CmdSyncSetting(string _token)
        {
            // Log message on all clients
            RpcSyncSetting(Config.FetchSetting(_token).GetSettingData());
        }

        [ClientRpc]
        private void RpcSyncSetting(SettingData _setting)
        {
            // Sync setting on client
            Config.FetchSetting(_setting.token).SetSyncedValue(_setting);
        }

        [Command]
        public void CmdSetLookupInt(string _key, int _value)
        {
            // Call on all clients
            RpcSetLookupInt(_key, _value);
        }

        [ClientRpc]
        public void RpcSetLookupInt(string _key, int _value)
        {
            // Set value in lookup table
            LookupTable.SetInt(_key, _value);

            // Check if verbose console is on
            if (Utils.verboseConsole)
            {
                // Log that value was set
                Log.Debug($"[NET UTILS] - Set integer in lookup table | Key: {_key} | Value: {_value}.");
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

        public void QueueDelayedPickupNotification(CharacterMaster _master, PickupIndex _triggerPickupIndex, PickupIndex _queuedPickupIndex)
        {
            // Validate input
            if (!NetworkServer.active || _master == null) return;

            // Host/local-player fast path
            if (_master == Utils.localPlayer)
            {
                CollectorsVision.QueuePendingNotification(_master, _triggerPickupIndex, _queuedPickupIndex);
                return;
            }

            // Attempt to get required components
            NetUtils netUtils = _master.GetComponent<NetUtils>();
            NetworkIdentity masterIdentity = _master.GetComponent<NetworkIdentity>();
            PlayerCharacterMasterController playerController = _master.playerCharacterMasterController;
            NetworkUser networkUser = playerController != null ? playerController.networkUser : null;

            if (netUtils == null || masterIdentity == null || networkUser == null || networkUser.connectionToClient == null)
            {
                if (Utils.verboseConsole)
                {
                    Log.Warning($"[NET UTILS] - QueueDelayedPickupNotification failed | Master: {_master.name} | Has NetUtils: {netUtils != null} | Has MasterIdentity: {masterIdentity != null} | Has PlayerController: {playerController != null} | Has NetworkUser: {networkUser != null} | Has Connection: {networkUser?.connectionToClient != null}");
                }
                return;
            }

            // Tell only the owning client to queue the delayed notification
            netUtils.TargetQueueDelayedPickupNotification(networkUser.connectionToClient, masterIdentity.netId, _triggerPickupIndex.value, _queuedPickupIndex.value);
        }

        [TargetRpc]
        private void TargetQueueDelayedPickupNotification(NetworkConnection _target, NetworkInstanceId _masterNetId, int _triggerPickupIndexValue, int _queuedPickupIndexValue)
        {
            // Attempt to resolve master on this client
            GameObject masterObject = ClientScene.FindLocalObject(_masterNetId);
            if (masterObject == null) return;

            CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
            if (master == null) return;

            if (Utils.verboseConsole) Log.Debug($"[NET UTILS] - TargetQueueDelayedPickupNotification received | MasterNetId: {_masterNetId} | Trigger: {_triggerPickupIndexValue} | Queued: {_queuedPickupIndexValue}");

            // Do not gate this with master.isLocalPlayer
            // This TargetRpc already arrived on the correct client
            CollectorsVision.QueuePendingNotification(master, new PickupIndex(_triggerPickupIndexValue), new PickupIndex(_queuedPickupIndexValue));
        }
    }
}