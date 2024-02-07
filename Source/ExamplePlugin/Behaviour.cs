using System.Collections.Generic;
using R2API;
using RoR2;

namespace Faithful
{
    internal delegate void Callback();

    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);
    internal delegate void OnHoldoutZoneStartCallback(HoldoutZoneController _zone);
    internal delegate void OnHoldoutZoneCalcRadiusCallback(ref float _radius, HoldoutZoneController _zone);

    internal delegate void StatsModCallback(int _count, RecalculateStatsAPI.StatHookEventArgs _stats);

    internal delegate void OnDamageDealtCallback(DamageReport _report);

    internal delegate void PlayerToPlayerCallback(PlayerCharacterMasterController _player1, PlayerCharacterMasterController _player2);
    internal delegate void PlayerHolderToPlayerCallback(int _count, PlayerCharacterMasterController _holder, PlayerCharacterMasterController _other);

    internal delegate void AllyHolderToAllyCallback(int _count, CharacterMaster _holder, CharacterMaster _other);

    internal class Behaviour
    {
        // Toolbox
        protected Toolbox toolbox;

        // Update callbacks
        protected List<Callback> updateCallbacks = new List<Callback>();
        protected List<Callback> debugUpdateCallbacks = new List<Callback>();
        protected List<Callback> fixedUpdateCallbacks = new List<Callback>();
        protected List<Callback> debugFixedUpdateCallbacks = new List<Callback>();

        // Holdout Zone callbacks
        protected List<InHoldoutZoneCallback> inHoldoutZoneCallbacks = new List<InHoldoutZoneCallback>();
        protected List<OnHoldoutZoneStartCallback> onHoldoutZoneStartCallbacks = new List<OnHoldoutZoneStartCallback>();
        protected List<OnHoldoutZoneCalcRadiusCallback> onHoldoutZoneCalcRadiusCallbacks = new List<OnHoldoutZoneCalcRadiusCallback>();

        // Stat modification item and buff callbacks
        List<ItemStatsMod> itemStatsMods = new List<ItemStatsMod>();
        List<BuffStatsMod> buffStatsMods = new List<BuffStatsMod>();

        // On Damage Dealt callbacks
        protected List<OnDamageDealtCallback> onDamageDealtCallbacks = new List<OnDamageDealtCallback>();

        // Player to player callbacks
        protected List<PlayerToPlayerCallback> playerToPlayerCallbacks = new List<PlayerToPlayerCallback>();
        protected List<PlayerItemToPlayer> playerItemToPlayerCallbacks = new List<PlayerItemToPlayer>();
        protected List<PlayerBuffToPlayer> playerBuffToPlayerCallbacks = new List<PlayerBuffToPlayer>();

        // Ally to ally callbacks
        protected List<AllyItemToAlly> allyItemToAllyCallbacks = new List<AllyItemToAlly>();
        protected List<AllyBuffToAlly> allyBuffToAllyCallbacks = new List<AllyBuffToAlly>();

        // Constructor
        public Behaviour(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Inject hooks
            On.RoR2.HoldoutZoneController.FixedUpdate += HookHoldoutZoneControllerFixedUpdate;
            On.RoR2.HoldoutZoneController.Start += HookHoldoutZoneControllerStart;
            RecalculateStatsAPI.GetStatCoefficients += HookStatsMod;
            GlobalEventManager.onServerDamageDealt += HookOnDamageDealt;

            Log.Debug("Behaviour initialised");
        }

        public void Update()
        {
            // Cycle through update callbacks
            foreach (Callback callback in updateCallbacks)
            {
                // Call
                callback();
            }

            // In debug mode?
            if (toolbox.utils.debugMode)
            {
                // Cycle through debug mode update callbacks
                foreach (Callback callback in debugUpdateCallbacks)
                {
                    // Call
                    callback();
                }
            }
        }

        public void FixedUpdate()
        {
            // Cycle through fixed update callbacks
            foreach (Callback callback in fixedUpdateCallbacks)
            {
                // Call
                callback();
            }

            // In debug mode?
            if (toolbox.utils.debugMode)
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

        // Add update callback
        public void AddUpdateCallback(Callback _callback, bool _debugOnly = false)
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

        // Add fixed update callback
        public void AddFixedUpdateCallback(Callback _callback, bool _debugOnly = false)
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

        // Add item stats mod
        public void AddStatsMod(Item _item, StatsModCallback _callback)
        {
            // Add item stats mod
            itemStatsMods.Add(new ItemStatsMod(_item, _callback));

            Log.Debug($"Added stat mods for '{_item.token}' item");
        }

        // Add buff stats mod
        public void AddStatsMod(Buff _buff, StatsModCallback _callback)
        {
            // Add buff stats mod
            buffStatsMods.Add(new BuffStatsMod(_buff, _callback));

            Log.Debug($"Added stat mods for '{_buff.token}' buff");
        }

        // Add in Holdout Zone callback
        public void AddInHoldoutZoneCallback(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            Log.Debug("Added in Holdout Zone behaviour");
        }

        // Add on Holdout Zone start callback
        public void AddOnHoldoutZoneStartCallback(OnHoldoutZoneStartCallback _callback)
        {
            onHoldoutZoneStartCallbacks.Add(_callback);

            Log.Debug("Added on Holdout Zone start behaviour");
        }

        // Add on Holdout Zone calc radius callback
        public void AddOnHoldoutZoneCalcRadiusCallback(OnHoldoutZoneCalcRadiusCallback _callback)
        {
            onHoldoutZoneCalcRadiusCallbacks.Add(_callback);

            Log.Debug("Added on Holdout Zone calc radius behaviour");
        }

        // Add player to player callback
        public void AddPlayerToPlayerCallback(PlayerToPlayerCallback _callback)
        {
            playerToPlayerCallbacks.Add(_callback);

            Log.Debug("Added Player to Player behaviour");
        }

        // Add player with item to player callback
        public void AddPlayerToPlayerCallback(Item _requiredItem, PlayerHolderToPlayerCallback _callback)
        {
            playerItemToPlayerCallbacks.Add(new PlayerItemToPlayer(_requiredItem, _callback));

            Log.Debug("Added Player to Player behaviour");
        }

        // Add player with buff to player callback
        public void AddPlayerToPlayerCallback(Buff _requiredBuff, PlayerHolderToPlayerCallback _callback)
        {
            playerBuffToPlayerCallbacks.Add(new PlayerBuffToPlayer(_requiredBuff, _callback));

            Log.Debug("Added Player to Player behaviour");
        }

        // Add ally with item to ally callback
        public void AddAllyToAllyCallback(Item _requiredItem, AllyHolderToAllyCallback _callback)
        {
            allyItemToAllyCallbacks.Add(new AllyItemToAlly(_requiredItem, _callback));

            Log.Debug("Added Ally to Ally behaviour");
        }

        // Add ally with buff to ally callback
        public void AddAllyToAllyCallback(Buff _requiredBuff, AllyHolderToAllyCallback _callback)
        {
            allyBuffToAllyCallbacks.Add(new AllyBuffToAlly(_requiredBuff, _callback));

            Log.Debug("Added Ally to Ally behaviour");
        }

        // Add on Damage Dealt callback
        public void AddOnDamageDealtCallback(OnDamageDealtCallback _callback)
        {
            onDamageDealtCallbacks.Add(_callback);

            Log.Debug("Added On Damage Dealt behaviour");
        }

        // Fixed update for checking player to player interactions
        private void PlayerOnPlayerFixedUpdate()
        {
            // Get list of players
            List<PlayerCharacterMasterController> players = toolbox.utils.GetPlayers();

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

        // Fixed update for checking ally to ally interactions
        private void AllyOnAllyFixedUpdate()
        {
            // Get list of allies
            List<CharacterMaster> allies = toolbox.utils.GetCharactersForTeam(TeamIndex.Player);

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

        // Do stat modifications
        private void HookStatsMod(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Cycle through item stats mod callbacks
            foreach (ItemStatsMod itemStatsMod in itemStatsMods)
            {
                // Proess item stats mod
                itemStatsMod.Process(_body, _stats);
            }

            // Cycle through buff stats mod callbacks
            foreach (BuffStatsMod buffStatsMod in buffStatsMods)
            {
                // Proess buff stats mod
                buffStatsMod.Process(_body, _stats);
            }
        }

        protected void HookHoldoutZoneControllerFixedUpdate(On.RoR2.HoldoutZoneController.orig_FixedUpdate orig, HoldoutZoneController self)
        {
            // Get Hurt Boxes in range of Holdout Zone
            HurtBox[] hurtBoxes = toolbox.utils.GetHurtBoxesInSphere(self.transform.position, self.currentRadius);

            // Cycle through Hurt Boxes
            foreach (HurtBox hurtBox in hurtBoxes)
            {
                // Cycle through InHoldoutZone callbacks and call with Character Body
                foreach (InHoldoutZoneCallback callback in inHoldoutZoneCallbacks)
                {
                    callback(hurtBox.healthComponent.body, self);
                }
            }

            orig(self); // Run normal processes
        }

        protected void HookHoldoutZoneControllerStart(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            // Add Faithful Holdout Zone mono behaviours
            FaithfulHoldoutZoneBehaviour behaviour = self.gameObject.AddComponent<FaithfulHoldoutZoneBehaviour>();
            behaviour.Init(onHoldoutZoneCalcRadiusCallbacks);

            // Cycle through OnHoldoutZoneStart callbacks
            foreach (OnHoldoutZoneStartCallback callback in onHoldoutZoneStartCallbacks)
            {
                // Call
                callback(self);
            }

            orig(self); // Run normal processes
        }

        protected void HookOnDamageDealt(DamageReport _report)
        {
            // Cycle through On Damage Dealt callbacks
            foreach (OnDamageDealtCallback callback in onDamageDealtCallbacks)
            {
                // Call
                callback(_report);
            }
        }
    }

    internal struct ItemStatsMod
    {
        // Store item and callback
        public Item item;
        public StatsModCallback callback;

        // Constructor
        public ItemStatsMod(Item _item, StatsModCallback _callback)
        {
            // Assign item and callback
            item = _item;
            callback = _callback;
        }

        public void Process(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Get character inventory
            Inventory inventory = _body.inventory;

            if (!inventory)
            {
                // Doesn't have an inventory
                return;
            }

            // Get item amount
            int itemCount = inventory.GetItemCount(item.itemDef);

            if (itemCount == 0)
            {
                // Doesn't have the item
                return;
            }

            // Call
            callback(itemCount, _stats);
        }
    }

    internal struct BuffStatsMod
    {
        // Store buff and callback
        public Buff buff;
        public StatsModCallback callback;

        // Constructor
        public BuffStatsMod(Buff _buff, StatsModCallback _callback)
        {
            // Assign buff and callback
            buff = _buff;
            callback = _callback;
        }

        public void Process(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Get buff amount
            int buffCount = _body.GetBuffCount(buff.buffDef);

            if (buffCount == 0)
            {
                // Doesn't have the buff
                return;
            }

            // Call
            callback(buffCount, _stats);
        }
    }

    internal struct PlayerItemToPlayer
    {
        // Store item and callback
        public Item item;
        public PlayerHolderToPlayerCallback callback;
        
        // Constructor
        public PlayerItemToPlayer(Item _item, PlayerHolderToPlayerCallback _callback)
        {
            // Assign item and callback
            item = _item;
            callback = _callback;
        }

        public void Process(PlayerCharacterMasterController _player, List<PlayerCharacterMasterController> _others)
        {
            // Check for body
            if (!_player.body)
            {
                return;
            }

            // Get character inventory
            Inventory inventory = _player.body.inventory;

            if (!inventory)
            {
                // Doesn't have an inventory
                return;
            }

            // Get item amount
            int itemCount = inventory.GetItemCount(item.itemDef);

            if (itemCount == 0)
            {
                // Doesn't have the item
                return;
            }

            // Cycle through other players
            foreach (PlayerCharacterMasterController other in _others)
            {
                // Skip self interaction
                if (other == _player)
                {
                    continue;
                }

                // Call
                callback(itemCount, _player, other);
            }
        }
    }

    internal struct PlayerBuffToPlayer
    {
        // Store buff and callback
        public Buff buff;
        public PlayerHolderToPlayerCallback callback;

        // Constructor
        public PlayerBuffToPlayer(Buff _buff, PlayerHolderToPlayerCallback _callback)
        {
            // Assign buff and callback
            buff = _buff;
            callback = _callback;
        }

        public void Process(PlayerCharacterMasterController _player, List<PlayerCharacterMasterController> _others)
        {
            // Check for body
            if (!_player.body)
            {
                return;
            }

            // Get buff amount
            int buffCount = _player.body.GetBuffCount(buff.buffDef);

            if (buffCount == 0)
            {
                // Doesn't have the buff
                return;
            }

            // Cycle through other players
            foreach (PlayerCharacterMasterController other in _others)
            {
                // Skip self interaction
                if (other == _player)
                {
                    continue;
                }

                // Call
                callback(buffCount, _player, other);
            }
        }
    }

    internal struct AllyItemToAlly
    {
        // Store item and callback
        public Item item;
        public AllyHolderToAllyCallback callback;

        // Constructor
        public AllyItemToAlly(Item _item, AllyHolderToAllyCallback _callback)
        {
            // Assign item and callback
            item = _item;
            callback = _callback;
        }

        public void Process(CharacterMaster _ally, List<CharacterMaster> _others)
        {
            // Check for body
            if (!_ally.hasBody)
            {
                return;
            }

            // Get character inventory
            Inventory inventory = _ally.GetBody().inventory;

            if (!inventory)
            {
                // Doesn't have an inventory
                return;
            }

            // Get item amount
            int itemCount = inventory.GetItemCount(item.itemDef);

            if (itemCount == 0)
            {
                // Doesn't have the item
                return;
            }

            // Cycle through other allies
            foreach (CharacterMaster other in _others)
            {
                // Skip self interaction
                if (other == _ally)
                {
                    continue;
                }

                // Call
                callback(itemCount, _ally, other);
            }
        }
    }

    internal struct AllyBuffToAlly
    {
        // Store buff and callback
        public Buff buff;
        public AllyHolderToAllyCallback callback;

        // Constructor
        public AllyBuffToAlly(Buff _buff, AllyHolderToAllyCallback _callback)
        {
            // Assign buff and callback
            buff = _buff;
            callback = _callback;
        }

        public void Process(CharacterMaster _ally, List<CharacterMaster> _others)
        {
            // Check for body
            if (!_ally.hasBody)
            {
                return;
            }

            // Get buff amount
            int buffCount = _ally.GetBody().GetBuffCount(buff.buffDef);

            if (buffCount == 0)
            {
                // Doesn't have the buff
                return;
            }

            // Cycle through other allies
            foreach (CharacterMaster other in _others)
            {
                // Skip self interaction
                if (other == _ally)
                {
                    continue;
                }

                // Call
                callback(buffCount, _ally, other);
            }
        }
    }
}
