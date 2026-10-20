using System.Collections.Generic;
using RoR2;

namespace Faithful
{
    internal delegate void Callback();

    internal delegate void PlayerToPlayerCallback(PlayerCharacterMasterController _player1, PlayerCharacterMasterController _player2);

    internal delegate void PlayerHolderToPlayerCallback(int _count, PlayerCharacterMasterController _holder, PlayerCharacterMasterController _other);

    internal delegate void AllyHolderToAllyCallback(int _count, CharacterMaster _holder, CharacterMaster _other);

    internal static partial class Behaviour
    {
        // Frame updates
        private static readonly List<Callback> updateCallbacks = new List<Callback>();

        private static readonly List<Callback> debugUpdateCallbacks = new List<Callback>();

        // Add update callback
        public static void AddUpdateCallback(Callback _callback, bool _debugOnly = false)
        {
            // Only for debug mode?
            if (_debugOnly)
            {
                // Add debug update callback
                debugUpdateCallbacks.Add(_callback);
            }
            else
            {
                // Add update callback
                updateCallbacks.Add(_callback);
            }
        }

        public static void Update()
        {
            if (!enabled || !Faithful.InitializationSucceeded) return;

            FaithfulRuntimeProfiler.StartScope("Behaviour.UpdateCallbacks");
            try
            {
                // Cycle through update callbacks
                foreach (Callback callback in updateCallbacks)
                {
                    // Call
                    callback();
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.UpdateCallbacks");
            }

            // In debug mode?
            if (Utils.debugMode)
            {
                // Cycle through debug mode update callbacks
                foreach (Callback callback in debugUpdateCallbacks)
                {
                    // Call
                    callback();
                }
            }
        }

        // Fixed updates
        private static readonly List<Callback> fixedUpdateCallbacks = new List<Callback>();

        private static readonly List<Callback> debugFixedUpdateCallbacks = new List<Callback>();

        // Add fixed update callback
        public static void AddFixedUpdateCallback(Callback _callback, bool _debugOnly = false)
        {
            // Only for debug mode?
            if (_debugOnly)
            {
                // Add debug fixed update callback
                debugFixedUpdateCallbacks.Add(_callback);
            }
            else
            {
                // Add fixed update callback
                fixedUpdateCallbacks.Add(_callback);
            }
        }

        public static void FixedUpdate()
        {
            if (!enabled || !Faithful.InitializationSucceeded) return;

            FaithfulRuntimeProfiler.StartScope("Behaviour.FixedUpdateCallbacks");
            try
            {
                // Cycle through fixed update callbacks
                foreach (Callback callback in fixedUpdateCallbacks)
                {
                    // Call
                    callback();
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.FixedUpdateCallbacks");
            }

            // In debug mode?
            if (Utils.debugMode)
            {
                // Cycle through debug mode fixed update callbacks
                foreach (Callback callback in debugFixedUpdateCallbacks)
                {
                    // Call
                    callback();
                }
            }

            // Player on player behaviour
            PlayerOnPlayerFixedUpdate();

            // Ally on ally behaviour
            AllyOnAllyFixedUpdate();
        }

        // Player interactions
        private static readonly List<PlayerToPlayerCallback> playerToPlayerCallbacks = new List<PlayerToPlayerCallback>();

        private static readonly List<PlayerItemToPlayer> playerItemToPlayerCallbacks = new List<PlayerItemToPlayer>();

        private static readonly List<PlayerBuffToPlayer> playerBuffToPlayerCallbacks = new List<PlayerBuffToPlayer>();

        // Add player to player callback
        public static void AddPlayerToPlayerCallback(PlayerToPlayerCallback _callback)
        {
            playerToPlayerCallbacks.Add(_callback);

            DebugLog("Added Player to Player behaviour");
        }

        // Add player with item to player callback
        public static void AddPlayerToPlayerCallback(Item _requiredItem, PlayerHolderToPlayerCallback _callback)
        {
            playerItemToPlayerCallbacks.Add(new PlayerItemToPlayer(_requiredItem, _callback));

            DebugLog("Added Player to Player behaviour");
        }

        // Add player with buff to player callback
        public static void AddPlayerToPlayerCallback(Buff _requiredBuff, PlayerHolderToPlayerCallback _callback)
        {
            playerBuffToPlayerCallbacks.Add(new PlayerBuffToPlayer(_requiredBuff, _callback));

            DebugLog("Added Player to Player behaviour");
        }

        // Fixed update for checking player to player interactions
        private static void PlayerOnPlayerFixedUpdate()
        {
            // Skip unused interactions
            if (playerItemToPlayerCallbacks.Count == 0 && playerBuffToPlayerCallbacks.Count == 0 && playerToPlayerCallbacks.Count == 0) return;

            FaithfulRuntimeProfiler.StartScope("Behaviour.PlayerOnPlayerFixedUpdate");
            try
            {
                // Reuse a snapshot so callbacks can safely change the live collection
                List<PlayerCharacterMasterController> players = HG.CollectionPool<PlayerCharacterMasterController, List<PlayerCharacterMasterController>>.RentCollection();
                try
                {
                    players.AddRange(PlayerCharacterMasterController.instances);

                    // Cycle through players
                    foreach (PlayerCharacterMasterController player in players)
                    {
                        // Process player holder on player behaviour
                        foreach (PlayerItemToPlayer callback in playerItemToPlayerCallbacks)
                        {
                            callback.Process(player, players);
                        }
                        foreach (PlayerBuffToPlayer callback in playerBuffToPlayerCallbacks)
                        {
                            callback.Process(player, players);
                        }

                        // Skip pair checks when no callbacks need them
                        if (playerToPlayerCallbacks.Count == 0) continue;

                        // Cycle through players again
                        foreach (PlayerCharacterMasterController subPlayer in players)
                        {
                            // Skip self interactions
                            if (player == subPlayer)
                            {
                                continue;
                            }

                            // Cycle through player to player callbacks
                            foreach (PlayerToPlayerCallback callback in playerToPlayerCallbacks)
                            {
                                // Call
                                callback(player, subPlayer);
                            }
                        }
                    }
                }
                finally
                {
                    HG.CollectionPool<PlayerCharacterMasterController, List<PlayerCharacterMasterController>>.ReturnCollection(players);
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.PlayerOnPlayerFixedUpdate");
            }
        }

        // Ally interactions
        private static readonly List<AllyItemToAlly> allyItemToAllyCallbacks = new List<AllyItemToAlly>();

        private static readonly List<AllyBuffToAlly> allyBuffToAllyCallbacks = new List<AllyBuffToAlly>();

        // Add ally with item to ally callback
        public static void AddAllyToAllyCallback(Item _requiredItem, AllyHolderToAllyCallback _callback)
        {
            allyItemToAllyCallbacks.Add(new AllyItemToAlly(_requiredItem, _callback));

            DebugLog("Added Ally to Ally behaviour");
        }

        // Add ally with buff to ally callback
        public static void AddAllyToAllyCallback(Buff _requiredBuff, AllyHolderToAllyCallback _callback)
        {
            allyBuffToAllyCallbacks.Add(new AllyBuffToAlly(_requiredBuff, _callback));

            DebugLog("Added Ally to Ally behaviour");
        }

        // Fixed update for checking ally to ally interactions
        private static void AllyOnAllyFixedUpdate()
        {
            // Skip unused interactions
            if (allyItemToAllyCallbacks.Count == 0 && allyBuffToAllyCallbacks.Count == 0) return;

            FaithfulRuntimeProfiler.StartScope("Behaviour.AllyOnAllyFixedUpdate");
            try
            {
                // Reuse a snapshot so callbacks can safely change the live collection
                List<CharacterMaster> allies = HG.CollectionPool<CharacterMaster, List<CharacterMaster>>.RentCollection();
                try
                {
                    Utils.GetCharactersForTeam(allies, TeamIndex.Player);

                    // Cycle through allies
                    foreach (CharacterMaster ally in allies)
                    {
                        // Process ally holder on ally behaviour
                        foreach (AllyItemToAlly callback in allyItemToAllyCallbacks)
                        {
                            callback.Process(ally, allies);
                        }
                        foreach (AllyBuffToAlly callback in allyBuffToAllyCallbacks)
                        {
                            callback.Process(ally, allies);
                        }
                    }
                }
                finally
                {
                    HG.CollectionPool<CharacterMaster, List<CharacterMaster>>.ReturnCollection(allies);
                }
            }
            finally
            {
                FaithfulRuntimeProfiler.StopScope("Behaviour.AllyOnAllyFixedUpdate");
            }
        }
    }
}
