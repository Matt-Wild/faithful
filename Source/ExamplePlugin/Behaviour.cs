using System.Collections.Generic;
using System.Linq;
using R2API;
using RoR2;
using UnityEngine;
using static Facepunch.Steamworks.Inventory.Item;
using static RoR2.DotController;

namespace Faithful
{
    internal delegate void Callback();

    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);
    internal delegate void OnHoldoutZoneStartCallback(HoldoutZoneController _zone);
    internal delegate void OnHoldoutZoneCalcRadiusCallback(ref float _radius, HoldoutZoneController _zone);

    internal delegate void StatsModCallback(int _count, RecalculateStatsAPI.StatHookEventArgs _stats);

    internal delegate void OnIncomingDamageCallback(DamageInfo _report, CharacterMaster _attacker, CharacterMaster _victim);
    internal delegate void DamageReportCallback(DamageReport _report);

    internal delegate void OnAddBuffCallback(BuffIndex _buff, CharacterBody _character);
    internal delegate void OnAddTimedBuffCallback(BuffDef _buff, float _duration, CharacterBody _character);
    internal delegate void OnInflictDamageOverTime(GameObject _victimObject, GameObject _attackerObject, DotController.DotIndex _dotIndex, float _duration, float _damageMultiplier, uint? _maxStacksFromAttacker);
    internal delegate void OnInflictDamageOverTimeRef(ref InflictDotInfo _inflictDotInfo);

    internal delegate void OnHealCallback(HealthComponent _healthComponent, ref float _amount, ref ProcChainMask _procChainMask, ref bool _nonRegen);

    internal delegate void CharacterBodyCallback(CharacterBody _body);

    internal delegate void OnPurchaseInteractionBeginCallback(PurchaseInteraction _shop, CharacterMaster _activator);
    internal delegate bool OnPurchaseCanBeAffordedCallback(PurchaseInteraction _shop, CharacterMaster _activator);

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

        // Damage Report callbacks
        protected List<OnIncomingDamageCallback> onIncomingDamageCallbacks = new List<OnIncomingDamageCallback>();
        protected List<DamageReportCallback> onDamageDealtCallbacks = new List<DamageReportCallback>();
        protected List<DamageReportCallback> onCharacterDeathCallbacks = new List<DamageReportCallback>();

        // Buff, debuff and DoT callbacks
        protected List<OnAddBuffCallback> onAddBuffCallbacks = new List<OnAddBuffCallback>();
        protected List<OnAddTimedBuffCallback> onAddTimedBuffCallbacks = new List<OnAddTimedBuffCallback>();
        protected List<OnInflictDamageOverTime> onInflictDamageOverTimeCallbacks = new List<OnInflictDamageOverTime>();
        protected List<OnInflictDamageOverTimeRef> onInflictDamageOverTimeRefCallbacks = new List<OnInflictDamageOverTimeRef>();

        // Heal callbacks
        protected List<OnHealCallback> onHealCallbacks = new List<OnHealCallback>();

        // Character Body callbacks
        protected List<CharacterBodyCallback> onRecalculateStatsCallbacks = new List<CharacterBodyCallback>();

        // Interactable callbacks
        protected List<OnPurchaseInteractionBeginCallback> onPurchaseInteractionBeginCallbacks = new List<OnPurchaseInteractionBeginCallback>();
        protected List<OnPurchaseCanBeAffordedCallback> onPurchaseCanBeAffordedCallbacks = new List<OnPurchaseCanBeAffordedCallback>();

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
            On.RoR2.CharacterBody.Awake += HookCharacterBodyAwake;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += HookAddBuffIndex;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += HookAddTimedBuffDef;
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += HookInflictDamageOverTime;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += HookInflictDamageOverTimeRef;
            On.RoR2.HealthComponent.Awake += HookHealthComponentAwake;
            On.RoR2.HealthComponent.Heal += HookHeal;
            On.RoR2.CharacterBody.RecalculateStats += HookRecalculateStats;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += HookPurchaseInteractionBegin;
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += HookPurchaseCanBeAfforded;
            RecalculateStatsAPI.GetStatCoefficients += HookStatsMod;
            GlobalEventManager.onServerDamageDealt += HookOnDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += HookOnCharacterDeath;

            DebugLog("Behaviour initialised");
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

            DebugLog($"Added stat mods for '{_item.token}' item");
        }

        // Add buff stats mod
        public void AddStatsMod(Buff _buff, StatsModCallback _callback)
        {
            // Add buff stats mod
            buffStatsMods.Add(new BuffStatsMod(_buff, _callback));

            DebugLog($"Added stat mods for '{_buff.token}' buff");
        }

        // Add in Holdout Zone callback
        public void AddInHoldoutZoneCallback(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            DebugLog("Added in Holdout Zone behaviour");
        }

        // Add on Holdout Zone start callback
        public void AddOnHoldoutZoneStartCallback(OnHoldoutZoneStartCallback _callback)
        {
            onHoldoutZoneStartCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone start behaviour");
        }

        // Add on Holdout Zone calc radius callback
        public void AddOnHoldoutZoneCalcRadiusCallback(OnHoldoutZoneCalcRadiusCallback _callback)
        {
            onHoldoutZoneCalcRadiusCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone calc radius behaviour");
        }

        // Add player to player callback
        public void AddPlayerToPlayerCallback(PlayerToPlayerCallback _callback)
        {
            playerToPlayerCallbacks.Add(_callback);

            DebugLog("Added Player to Player behaviour");
        }

        // Add player with item to player callback
        public void AddPlayerToPlayerCallback(Item _requiredItem, PlayerHolderToPlayerCallback _callback)
        {
            playerItemToPlayerCallbacks.Add(new PlayerItemToPlayer(_requiredItem, _callback));

            DebugLog("Added Player to Player behaviour");
        }

        // Add player with buff to player callback
        public void AddPlayerToPlayerCallback(Buff _requiredBuff, PlayerHolderToPlayerCallback _callback)
        {
            playerBuffToPlayerCallbacks.Add(new PlayerBuffToPlayer(_requiredBuff, _callback));

            DebugLog("Added Player to Player behaviour");
        }

        // Add ally with item to ally callback
        public void AddAllyToAllyCallback(Item _requiredItem, AllyHolderToAllyCallback _callback)
        {
            allyItemToAllyCallbacks.Add(new AllyItemToAlly(_requiredItem, _callback));

            DebugLog("Added Ally to Ally behaviour");
        }

        // Add ally with buff to ally callback
        public void AddAllyToAllyCallback(Buff _requiredBuff, AllyHolderToAllyCallback _callback)
        {
            allyBuffToAllyCallbacks.Add(new AllyBuffToAlly(_requiredBuff, _callback));

            DebugLog("Added Ally to Ally behaviour");
        }

        // Add On Incoming Damage callback
        public void AddOnIncomingDamageCallback(OnIncomingDamageCallback _callback)
        {
            onIncomingDamageCallbacks.Add(_callback);

            DebugLog("Added On Incoming Damage behaviour");
        }

        // Add On Damage Dealt callback
        public void AddOnDamageDealtCallback(DamageReportCallback _callback)
        {
            onDamageDealtCallbacks.Add(_callback);

            DebugLog("Added On Damage Dealt behaviour");
        }

        // Add On Character Death callback
        public void AddOnCharacterDeathCallback(DamageReportCallback _callback)
        {
            onCharacterDeathCallbacks.Add(_callback);

            DebugLog("Added On Character Death behaviour");
        }

        // Add On Add Buff callback
        public void AddOnAddBuffCallback(OnAddBuffCallback _callback)
        {
            onAddBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Buff behaviour");
        }

        // Add On Add Timed Buff callback
        public void AddOnAddTimedBuffCallback(OnAddTimedBuffCallback _callback)
        {
            onAddTimedBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Timed Buff behaviour");
        }

        // Add On Inflict Damage Over Time callback
        public void AddOnInflictDamageOverTimeCallback(OnInflictDamageOverTime _callback)
        {
            onInflictDamageOverTimeCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time behaviour");
        }

        // Add On Inflict Damage Over Time Ref callback
        public void AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRef _callback)
        {
            onInflictDamageOverTimeRefCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time Ref behaviour");
        }

        // Add On Heal callback
        public void AddOnHealCallback(OnHealCallback _callback)
        {
            onHealCallbacks.Add(_callback);

            DebugLog("Added On Heal behaviour");
        }

        // Add On Recalculate Stats callback
        public void AddOnRecalculateStatsCallback(CharacterBodyCallback _callback)
        {
            onRecalculateStatsCallbacks.Add(_callback);

            DebugLog("Added On Recalculate Stats behaviour");
        }

        // Add On Purchase Interaction Begin callback
        public void AddOnPurchaseInteractionBeginCallback(OnPurchaseInteractionBeginCallback _callback)
        {
            onPurchaseInteractionBeginCallbacks.Add(_callback);

            DebugLog("Added On Purchase Interaction Begin behaviour");
        }

        // Add On Purchase Can Be Afforded callback
        public void AddOnPurchaseCanBeAffordedCallback(OnPurchaseCanBeAffordedCallback _callback)
        {
            onPurchaseCanBeAffordedCallbacks.Add(_callback);

            DebugLog("Added On Purchase Can Be Afforded behaviour");
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

        protected void HookCharacterBodyAwake(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            // Add custom character body behaviour
            FaithfulCharacterBodyBehaviour component = self.gameObject.AddComponent<FaithfulCharacterBodyBehaviour>();

            // Pass Toolbox reference to custom component
            component.toolbox = toolbox;

            orig(self); // Run normal processes
        }

        protected void HookAddBuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            // Cycle through On Add Buff callbacks
            foreach (OnAddBuffCallback callback in onAddBuffCallbacks)
            {
                // Call
                callback(buffType, self);
            }

            orig(self, buffType); // Run normal processes
        }

        protected void HookAddTimedBuffDef(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            // Cycle through On Add Timed Buff callbacks
            foreach (OnAddTimedBuffCallback callback in onAddTimedBuffCallbacks)
            {
                // Call
                callback(buffDef, duration, self);
            }

            orig(self, buffDef, duration); // Run normal processes
        }

        protected void HookInflictDamageOverTime(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker); // Run normal processes

            // Cycle through On Inflict Damage Over Time callbacks
            foreach (OnInflictDamageOverTime callback in onInflictDamageOverTimeCallbacks)
            {
                // Call
                callback(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);
            }
        }

        protected void HookInflictDamageOverTimeRef(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            // Cycle through On Inflict Damage Over Time Ref callbacks
            foreach (OnInflictDamageOverTimeRef callback in onInflictDamageOverTimeRefCallbacks)
            {
                // Call
                callback(ref inflictDotInfo);
            }

            orig(ref inflictDotInfo); // Run normal processes
        }

        protected void HookHealthComponentAwake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            // Add custom health component behaviour
            FaithfulHealthComponentBehaviour component = self.gameObject.AddComponent<FaithfulHealthComponentBehaviour>();

            // Pass Toolbox reference to custom component
            component.toolbox = toolbox;

            orig(self); // Run normal processes
        }

        protected float HookHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            // Cycle through On Heal callbacks
            foreach (OnHealCallback callback in onHealCallbacks)
            {
                // Call
                callback(self, ref amount, ref procChainMask, ref nonRegen);
            }

            return orig(self, amount, procChainMask, nonRegen); // Run normal processes
        }

        protected void HookRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self); // Run normal processes

            // Cycle through On Recalculate Stats callbacks
            foreach (CharacterBodyCallback callback in onRecalculateStatsCallbacks)
            {
                // Call
                callback(self);
            }

            // Update all temporary visual effects (again)
            self.UpdateAllTemporaryVisualEffects();
        }

        protected void HookPurchaseInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            // Cycle through OnPurchaseInteractionBegin callbacks
            foreach (OnPurchaseInteractionBeginCallback callback in onPurchaseInteractionBeginCallbacks)
            {
                // Check for activator
                CharacterBody body = activator.gameObject.GetComponent<CharacterBody>();
                if (body != null)
                {
                    // Call
                    callback(self, body.master);
                }
            }

            orig(self, activator); // Run normal processes
        }

        protected bool HookPurchaseCanBeAfforded(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            // Cycle through OnPurchaseCanBeAfforded callbacks
            foreach (OnPurchaseCanBeAffordedCallback callback in onPurchaseCanBeAffordedCallbacks)
            {
                // Check for activator
                CharacterBody body = activator.gameObject.GetComponent<CharacterBody>();
                if (body != null)
                {
                    // Call
                    if (callback(self, body.master))
                    {
                        // Force can be afforded if result is true
                        return true;
                    }
                }
            }

            return orig(self, activator); // Run normal processes
        }

        protected void HookOnDamageDealt(DamageReport _report)
        {
            // Cycle through On Damage Dealt callbacks
            foreach (DamageReportCallback callback in onDamageDealtCallbacks)
            {
                // Call
                callback(_report);
            }
        }

        protected void HookOnCharacterDeath(DamageReport _report)
        {
            // Cycle through On Character Death callbacks
            foreach (DamageReportCallback callback in onCharacterDeathCallbacks)
            {
                // Call
                callback(_report);
            }
        }

        public void OnIncomingDamageServer(DamageInfo _damageInfo, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Cycle through On Incoming Damage callbacks
            foreach (OnIncomingDamageCallback callback in onIncomingDamageCallbacks)
            {
                // Call
                callback(_damageInfo, _attacker, _victim);
            }
        }

        public void DebugLog(string _message)
        {
            // Only log behaviour on debug
            if (toolbox.utils.debugMode)
            {
                // Log message
                Log.Debug($"[BEHAVIOUR] - {_message}");
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
