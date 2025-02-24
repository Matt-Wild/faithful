using System.Collections.Generic;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
    internal delegate void OnInflictDamageOverTimeCallback(GameObject _victimObject, GameObject _attackerObject, DotController.DotIndex _dotIndex, float _duration, float _damageMultiplier, uint? _maxStacksFromAttacker);
    internal delegate void OnInflictDamageOverTimeRefCallback(ref InflictDotInfo _inflictDotInfo);

    internal delegate void OnTransferItemCallback(Inventory _inventory, ItemIndex _index, int _count);
    internal delegate void OnInventoryCallback(Inventory _inventory);

    internal delegate void OnHealCallback(HealthComponent _healthComponent, ref float _amount, ref ProcChainMask _procChainMask, ref bool _nonRegen);

    internal delegate void CharacterBodyCallback(CharacterBody _body);

    internal delegate void OnPurchaseInteractionBeginCallback(PurchaseInteraction _shop, CharacterMaster _activator);
    internal delegate bool OnPurchaseCanBeAffordedCallback(PurchaseInteraction _shop, CharacterMaster _activator);
    internal delegate void InteractorCallback(Interactor _interactor);
    internal delegate void FaithfulInteractorCallback(FaithfulInteractableBehaviour _behaviour, Interactor _interactor);

    internal delegate void OnProcessJumpCallback(GenericCharacterMain _character);

    internal delegate void PlayerToPlayerCallback(PlayerCharacterMasterController _player1, PlayerCharacterMasterController _player2);
    internal delegate void PlayerHolderToPlayerCallback(int _count, PlayerCharacterMasterController _holder, PlayerCharacterMasterController _other);

    internal delegate void AllyHolderToAllyCallback(int _count, CharacterMaster _holder, CharacterMaster _other);

    internal delegate void GenericCharacterCallback(GenericCharacterMain _character);

    internal delegate void SceneDirectorCallback(SceneDirector _director);
    internal delegate void SceneExitControllerCallback(SceneExitController _exitController);

    internal static class Behaviour
    {
        // Store if behaviour is enabled
        static bool enabled = false;

        // Store character body helper prefab
        internal static GameObject characterBodyHelperPrefab;

        // Update callbacks
        private static List<Callback> updateCallbacks = new List<Callback>();
        private static List<Callback> debugUpdateCallbacks = new List<Callback>();
        private static List<Callback> fixedUpdateCallbacks = new List<Callback>();
        private static List<Callback> debugFixedUpdateCallbacks = new List<Callback>();

        // Holdout Zone callbacks
        private static List<InHoldoutZoneCallback> inHoldoutZoneCallbacks = new List<InHoldoutZoneCallback>();
        private static List<OnHoldoutZoneStartCallback> onHoldoutZoneStartCallbacks = new List<OnHoldoutZoneStartCallback>();
        private static List<OnHoldoutZoneCalcRadiusCallback> onHoldoutZoneCalcRadiusCallbacks = new List<OnHoldoutZoneCalcRadiusCallback>();

        // Stat modification item and buff callbacks
        static List<ItemStatsMod> itemStatsMods = new List<ItemStatsMod>();
        static List<BuffStatsMod> buffStatsMods = new List<BuffStatsMod>();

        // Damage Report callbacks
        private static List<OnIncomingDamageCallback> onIncomingDamageCallbacks = new List<OnIncomingDamageCallback>();
        private static List<DamageReportCallback> onDamageDealtCallbacks = new List<DamageReportCallback>();
        private static List<DamageReportCallback> onCharacterDeathCallbacks = new List<DamageReportCallback>();

        // Buff, debuff and DoT callbacks
        private static List<OnAddBuffCallback> onAddBuffCallbacks = new List<OnAddBuffCallback>();
        private static List<OnAddTimedBuffCallback> onAddTimedBuffCallbacks = new List<OnAddTimedBuffCallback>();
        private static List<OnInflictDamageOverTimeCallback> onInflictDamageOverTimeCallbacks = new List<OnInflictDamageOverTimeCallback>();
        private static List<OnInflictDamageOverTimeRefCallback> onInflictDamageOverTimeRefCallbacks = new List<OnInflictDamageOverTimeRefCallback>();

        // Item interaction callbacks
        private static List<OnTransferItemCallback> onGiveItemCallbacks = new List<OnTransferItemCallback>();
        private static List<OnTransferItemCallback> onRemoveItemCallbacks = new List<OnTransferItemCallback>();
        private static List<OnInventoryCallback> onInventoryChangedCallbacks = new List<OnInventoryCallback>();
        private static Dictionary<ItemDef, List<OnInventoryCallback>> onItemAddedCallbacks = new Dictionary<ItemDef, List<OnInventoryCallback>>();

        // Heal callbacks
        private static List<OnHealCallback> onHealCallbacks = new List<OnHealCallback>();

        // Character Body callbacks
        private static List<CharacterBodyCallback> onCharacterBodyAwakeCallbacks = new List<CharacterBodyCallback>();
        private static List<CharacterBodyCallback> onCharacterBodyStartCallbacks = new List<CharacterBodyCallback>();
        private static List<CharacterBodyCallback> onRecalculateStatsCallbacks = new List<CharacterBodyCallback>();
        private static List<CharacterBodyCallback> onUpdateVisualEffectsCallbacks = new List<CharacterBodyCallback>();
        private static List<CharacterBodyCallback> onCharacterBodyFixedUpdateCallbacks = new List<CharacterBodyCallback>();

        // Interactable callbacks
        private static List<OnPurchaseInteractionBeginCallback> onPurchaseInteractionBeginCallbacks = new List<OnPurchaseInteractionBeginCallback>();
        private static List<OnPurchaseCanBeAffordedCallback> onPurchaseCanBeAffordedCallbacks = new List<OnPurchaseCanBeAffordedCallback>();

        // Character movement callbacks
        private static List<OnProcessJumpCallback> onProcessJumpCallbacks = new List<OnProcessJumpCallback>();

        // Player to player callbacks
        private static List<PlayerToPlayerCallback> playerToPlayerCallbacks = new List<PlayerToPlayerCallback>();
        private static List<PlayerItemToPlayer> playerItemToPlayerCallbacks = new List<PlayerItemToPlayer>();
        private static List<PlayerBuffToPlayer> playerBuffToPlayerCallbacks = new List<PlayerBuffToPlayer>();

        // Ally to ally callbacks
        private static List<AllyItemToAlly> allyItemToAllyCallbacks = new List<AllyItemToAlly>();
        private static List<AllyBuffToAlly> allyBuffToAllyCallbacks = new List<AllyBuffToAlly>();

        // Generic character callbacks
        private static List<GenericCharacterCallback> genericCharacterFixedUpdateCallbacks = new List<GenericCharacterCallback>();

        // Scene callbacks
        private static List<SceneDirectorCallback> onPrePopulateSceneCallbacks = new List<SceneDirectorCallback>();
        private static List<SceneExitControllerCallback> onPreSceneExitCallbacks = new List<SceneExitControllerCallback>();

        public static void Init()
        {
            // Create prefabs
            CreatePrefabs();

            // Enable behaviour
            Enable();

            DebugLog("Behaviour initialised");
        }

        public static void Enable()
        {
            // Check if already enabled
            if (enabled) return;

            // Update enabled
            enabled = true;

            // Inject hooks
            On.RoR2.HoldoutZoneController.Update += HookHoldoutZoneControllerUpdate;
            On.RoR2.HoldoutZoneController.Start += HookHoldoutZoneControllerStart;
            On.RoR2.CharacterBody.Awake += HookCharacterBodyAwake;
            On.RoR2.CharacterBody.Start += HookCharacterBodyStart;
            On.RoR2.CharacterBody.AddBuff_BuffIndex += HookAddBuffIndex;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += HookAddTimedBuffDef;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += HookServerGiveItem;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int += HookServerRemoveItem;
            On.RoR2.Inventory.HandleInventoryChanged += HookInventoryChanged;
            On.RoR2.Inventory.RpcItemAdded += HookItemAdded;
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += HookInflictDamageOverTime;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += HookInflictDamageOverTimeRef;
            On.RoR2.HealthComponent.Awake += HookHealthComponentAwake;
            On.RoR2.HealthComponent.Heal += HookHeal;
            On.RoR2.CharacterBody.RecalculateStats += HookRecalculateStats;
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += HookUpdateVisualEffects;
            On.RoR2.CharacterBody.FixedUpdate += HookCharacterBodyFixedUpdate;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += HookPurchaseInteractionBegin;
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += HookPurchaseCanBeAfforded;
            On.EntityStates.GenericCharacterMain.ProcessJump += HookProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate += HookGenericCharacterFixedUpdate;
            On.RoR2.SceneExitController.SetState += HookSceneExitControllerSetState;
            RecalculateStatsAPI.GetStatCoefficients += HookStatsMod;
            GlobalEventManager.onServerDamageDealt += HookOnDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += HookOnCharacterDeath;
            SceneDirector.onPrePopulateSceneServer += HookPrePopulateScene;
        }

        public static void Disable()
        {
            // Check if already disabled
            if (!enabled) return;

            // Update enabled
            enabled = false;

            // Inject hooks
            On.RoR2.HoldoutZoneController.Update -= HookHoldoutZoneControllerUpdate;
            On.RoR2.HoldoutZoneController.Start -= HookHoldoutZoneControllerStart;
            On.RoR2.CharacterBody.Awake -= HookCharacterBodyAwake;
            On.RoR2.CharacterBody.Start -= HookCharacterBodyStart;
            On.RoR2.CharacterBody.AddBuff_BuffIndex -= HookAddBuffIndex;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float -= HookAddTimedBuffDef;
            On.RoR2.Inventory.GiveItem_ItemIndex_int -= HookServerGiveItem;
            On.RoR2.Inventory.RemoveItem_ItemIndex_int -= HookServerRemoveItem;
            On.RoR2.Inventory.HandleInventoryChanged -= HookInventoryChanged;
            On.RoR2.Inventory.RpcItemAdded -= HookItemAdded;
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 -= HookInflictDamageOverTime;
            On.RoR2.DotController.InflictDot_refInflictDotInfo -= HookInflictDamageOverTimeRef;
            On.RoR2.HealthComponent.Awake -= HookHealthComponentAwake;
            On.RoR2.HealthComponent.Heal -= HookHeal;
            On.RoR2.CharacterBody.RecalculateStats -= HookRecalculateStats;
            On.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects -= HookUpdateVisualEffects;
            On.RoR2.CharacterBody.FixedUpdate -= HookCharacterBodyFixedUpdate;
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= HookPurchaseInteractionBegin;
            On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor -= HookPurchaseCanBeAfforded;
            On.EntityStates.GenericCharacterMain.ProcessJump -= HookProcessJump;
            On.EntityStates.GenericCharacterMain.FixedUpdate -= HookGenericCharacterFixedUpdate;
            RecalculateStatsAPI.GetStatCoefficients -= HookStatsMod;
            GlobalEventManager.onServerDamageDealt -= HookOnDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal -= HookOnCharacterDeath;
            SceneDirector.onPrePopulateSceneServer -= HookPrePopulateScene;
        }

        private static void CreatePrefabs()
        {
            // We create an empty gameobject to hold all the networked components
            var tempGO = new GameObject("temp GO");

            // Add the NetworkIdentity so that Unity knows which Object it's going to be networking all about
            tempGO.AddComponent<NetworkIdentity>();

            // Use InstantiateClone from the PrefabAPI to make sure we have full control over the GameObject
            characterBodyHelperPrefab = tempGO.InstantiateClone("faithfulCharacterBodyHelper");

            // Delete the now useless temporary GameObject
            Object.Destroy(tempGO);

            // Add a specific components (which can be networked)
            characterBodyHelperPrefab.AddComponent<FaithfulCharacterBodyBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulTJetpackBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulLeadersPennonBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulTargetingMatrixBehaviour>();
            characterBodyHelperPrefab.AddComponent<FaithfulHermitsShawlBehaviour>();
        }

        public static void Update()
        {
            // Cycle through update callbacks
            foreach (Callback callback in updateCallbacks)
            {
                // Call
                callback();
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

        public static void FixedUpdate()
        {
            // Cycle through fixed update callbacks
            foreach (Callback callback in fixedUpdateCallbacks)
            {
                // Call
                callback();
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

        // Add item stats mod
        public static void AddStatsMod(Item _item, StatsModCallback _callback)
        {
            // Add item stats mod
            itemStatsMods.Add(new ItemStatsMod(_item, _callback));

            DebugLog($"Added stat mods for '{_item.token}' item");
        }

        // Add buff stats mod
        public static void AddStatsMod(Buff _buff, StatsModCallback _callback)
        {
            // Add buff stats mod
            buffStatsMods.Add(new BuffStatsMod(_buff, _callback));

            DebugLog($"Added stat mods for '{_buff.token}' buff");
        }

        // Add in Holdout Zone callback
        public static void AddInHoldoutZoneCallback(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            DebugLog("Added in Holdout Zone behaviour");
        }

        // Add on Holdout Zone start callback
        public static void AddOnHoldoutZoneStartCallback(OnHoldoutZoneStartCallback _callback)
        {
            onHoldoutZoneStartCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone start behaviour");
        }

        // Add on Holdout Zone calc radius callback
        public static void AddOnHoldoutZoneCalcRadiusCallback(OnHoldoutZoneCalcRadiusCallback _callback)
        {
            onHoldoutZoneCalcRadiusCallbacks.Add(_callback);

            DebugLog("Added on Holdout Zone calc radius behaviour");
        }

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

        // Add On Incoming Damage callback
        public static void AddOnIncomingDamageCallback(OnIncomingDamageCallback _callback)
        {
            onIncomingDamageCallbacks.Add(_callback);

            DebugLog("Added On Incoming Damage behaviour");
        }

        // Add On Damage Dealt callback
        public static void AddOnDamageDealtCallback(DamageReportCallback _callback)
        {
            onDamageDealtCallbacks.Add(_callback);

            DebugLog("Added On Damage Dealt behaviour");
        }

        // Add On Character Death callback
        public static void AddOnCharacterDeathCallback(DamageReportCallback _callback)
        {
            onCharacterDeathCallbacks.Add(_callback);

            DebugLog("Added On Character Death behaviour");
        }

        // Add On Add Buff callback
        public static void AddOnAddBuffCallback(OnAddBuffCallback _callback)
        {
            onAddBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Buff behaviour");
        }

        // Add On Add Timed Buff callback
        public static void AddOnAddTimedBuffCallback(OnAddTimedBuffCallback _callback)
        {
            onAddTimedBuffCallbacks.Add(_callback);

            DebugLog("Added On Add Timed Buff behaviour");
        }

        // Add On Inflict Damage Over Time callback
        public static void AddOnInflictDamageOverTimeCallback(OnInflictDamageOverTimeCallback _callback)
        {
            onInflictDamageOverTimeCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time behaviour");
        }

        // Add On Inflict Damage Over Time Ref callback
        public static void AddOnInflictDamageOverTimeRefCallback(OnInflictDamageOverTimeRefCallback _callback)
        {
            onInflictDamageOverTimeRefCallbacks.Add(_callback);

            DebugLog("Added On Inflict Damage Over Time Ref behaviour");
        }

        // Add On Give Item callback (SERVER ONLY)
        public static void AddServerOnGiveItemCallback(OnTransferItemCallback _callback)
        {
            onGiveItemCallbacks.Add(_callback);

            DebugLog("Added On Give Item behaviour");
        }

        // Add On Remove Item callback (SERVER ONLY)
        public static void AddServerOnRemoveItemCallback(OnTransferItemCallback _callback)
        {
            onRemoveItemCallbacks.Add(_callback);

            DebugLog("Added On Remove Item behaviour");
        }

        // Add On Inventory Changed callback
        public static void AddOnInventoryChangedCallback(OnInventoryCallback _callback)
        {
            onInventoryChangedCallbacks.Add(_callback);

            DebugLog("Added On Inventory Changed behaviour");
        }

        // Remove On Inventory Changed callback
        public static void RemoveOnInventoryChangedCallback(OnInventoryCallback _callback)
        {
            onInventoryChangedCallbacks.Remove(_callback);

            DebugLog("Removed On Inventory Changed behaviour");
        }

        // Add item added callback
        public static void AddOnItemAddedCallback(ItemDef _itemDef, OnInventoryCallback _callback)
        {
            // Check for dictionary entry for item
            if (onItemAddedCallbacks.ContainsKey(_itemDef))
            {
                // Add to list
                onItemAddedCallbacks[_itemDef].Add(_callback);
            }

            // New entry needed
            else
            {
                // Add new entry
                onItemAddedCallbacks[_itemDef] = new List<OnInventoryCallback>() { _callback };
            }

            DebugLog("Added On Item Added behaviour");
        }

        // Remove item added callback
        public static void RemoveOnItemAddedCallback(ItemDef _itemDef, OnInventoryCallback _callback)
        {
            // Check for dictionary entry for item
            if (onItemAddedCallbacks.ContainsKey(_itemDef))
            {
                // Remove from list
                onItemAddedCallbacks[_itemDef].Remove(_callback);

                // Check if list is empty
                if (onItemAddedCallbacks[_itemDef].Count == 0)
                {
                    // Remove entry
                    onItemAddedCallbacks.Remove(_itemDef);
                }

                DebugLog("Removed On Item Added behaviour");
            }
        }

        // Add On Heal callback
        public static void AddOnHealCallback(OnHealCallback _callback)
        {
            onHealCallbacks.Add(_callback);

            DebugLog("Added On Heal behaviour");
        }

        // Add On Character Body Awake callback
        public static void AddOnCharacterBodyAwakeCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyAwakeCallbacks.Add(_callback);

            DebugLog("Added On Character Body Awake behaviour");
        }

        // Add On Character Body Start callback
        public static void AddOnCharacterBodyStartCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyStartCallbacks.Add(_callback);

            DebugLog("Added On Character Body Start behaviour");
        }

        // Add On Recalculate Stats callback
        public static void AddOnRecalculateStatsCallback(CharacterBodyCallback _callback)
        {
            onRecalculateStatsCallbacks.Add(_callback);

            DebugLog("Added On Recalculate Stats behaviour");
        }

        // Add On Update Visual Effects callback
        public static void AddOnUpdateVisualEffectsCallback(CharacterBodyCallback _callback)
        {
            onUpdateVisualEffectsCallbacks.Add(_callback);

            DebugLog("Added On Update Visual Effects behaviour");
        }

        public static void AddOnCharacterBodyFixedUpdateCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyFixedUpdateCallbacks.Add(_callback);

            DebugLog("Added On Character Body Fixed Update behaviour");
        }

        public static void RemoveOnCharacterBodyFixedUpdateCallback(CharacterBodyCallback _callback)
        {
            onCharacterBodyFixedUpdateCallbacks.Remove(_callback);

            DebugLog("Removed On Character Body Fixed Update behaviour");
        }

        // Add On Purchase Interaction Begin callback
        public static void AddOnPurchaseInteractionBeginCallback(OnPurchaseInteractionBeginCallback _callback)
        {
            onPurchaseInteractionBeginCallbacks.Add(_callback);

            DebugLog("Added On Purchase Interaction Begin behaviour");
        }

        // Add On Purchase Can Be Afforded callback
        public static void AddOnPurchaseCanBeAffordedCallback(OnPurchaseCanBeAffordedCallback _callback)
        {
            onPurchaseCanBeAffordedCallbacks.Add(_callback);

            DebugLog("Added On Purchase Can Be Afforded behaviour");
        }

        // Add On Process Jump callback
        public static void AddOnProcessJumpCallback(OnProcessJumpCallback _callback)
        {
            onProcessJumpCallbacks.Add(_callback);

            DebugLog("Added On Process Jump behaviour");
        }

        // Add generic character fixed update callback
        public static void AddGenericCharacterFixedUpdateCallback(GenericCharacterCallback _callback)
        {
            genericCharacterFixedUpdateCallbacks.Add(_callback);

            DebugLog("Added Generic Character Fixed Update behaviour");
        }

        public static void AddOnPrePopulateSceneCallback(SceneDirectorCallback _callback)
        {
            onPrePopulateSceneCallbacks.Add(_callback);

            DebugLog("Added On Pre-Populate Scene behaviour");
        }

        public static void AddOnPreSceneExitCallback(SceneExitControllerCallback _callback)
        {
            onPreSceneExitCallbacks.Add(_callback);

            DebugLog("Added On Pre-Scene Exit behaviour");
        }

        // Fixed update for checking player to player interactions
        private static void PlayerOnPlayerFixedUpdate()
        {
            // Get list of players
            List<PlayerCharacterMasterController> players = Utils.GetPlayers();

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
        private static void AllyOnAllyFixedUpdate()
        {
            // Get list of allies
            List<CharacterMaster> allies = Utils.GetCharactersForTeam(TeamIndex.Player);

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
        private static void HookStatsMod(CharacterBody _body, RecalculateStatsAPI.StatHookEventArgs _stats)
        {
            // Cycle through item stats mod callbacks
            foreach (ItemStatsMod itemStatsMod in itemStatsMods)
            {
                // Process item stats mod
                itemStatsMod.Process(_body, _stats);
            }

            // Cycle through buff stats mod callbacks
            foreach (BuffStatsMod buffStatsMod in buffStatsMods)
            {
                // Proess buff stats mod
                buffStatsMod.Process(_body, _stats);
            }
        }

        private static void HookHoldoutZoneControllerUpdate(On.RoR2.HoldoutZoneController.orig_Update orig, HoldoutZoneController self)
        {
            // Get Character Bodies in holdout zone
            CharacterBody[] characterBodies = Utils.GetCharacterBodiesInHoldoutZone(self);

            // Cycle through Character Bodies
            foreach (CharacterBody current in characterBodies)
            {
                // Cycle through InHoldoutZone callbacks and call with Character Body
                foreach (InHoldoutZoneCallback callback in inHoldoutZoneCallbacks)
                {
                    callback(current, self);
                }
            }

            orig(self); // Run normal processes
        }

        private static void HookHoldoutZoneControllerStart(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
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

        private static void HookCharacterBodyAwake(On.RoR2.CharacterBody.orig_Awake orig, CharacterBody self)
        {
            orig(self); // Run normal processes

            // Cycle through On Character Body Awake callbacks
            foreach (CharacterBodyCallback callback in onCharacterBodyAwakeCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookCharacterBodyStart(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            //Log.Message($"Character body start for character '{self.name}' with netID {self.GetComponent<NetworkIdentity>().netId}");

            // Check if server and valid character
            if (NetworkServer.active && self.master != null)
            {
                // Add character body behaviour object
                GameObject characterBodyBehaviourObj = Object.Instantiate(characterBodyHelperPrefab);

                // Spawn object for clients
                NetworkServer.Spawn(characterBodyBehaviourObj);

                // Link the character body ID
                characterBodyBehaviourObj.GetComponent<FaithfulCharacterBodyBehaviour>().characterID = self.GetComponent<NetworkIdentity>().netId;
            }

            orig(self); // Run normal processes

            // Cycle through On Character Body Start callbacks
            foreach (CharacterBodyCallback callback in onCharacterBodyStartCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookAddBuffIndex(On.RoR2.CharacterBody.orig_AddBuff_BuffIndex orig, CharacterBody self, BuffIndex buffType)
        {
            // Cycle through On Add Buff callbacks
            foreach (OnAddBuffCallback callback in onAddBuffCallbacks)
            {
                // Call
                callback(buffType, self);
            }

            orig(self, buffType); // Run normal processes
        }

        private static void HookAddTimedBuffDef(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            // Cycle through On Add Timed Buff callbacks
            foreach (OnAddTimedBuffCallback callback in onAddTimedBuffCallbacks)
            {
                // Call
                callback(buffDef, duration, self);
            }

            orig(self, buffDef, duration); // Run normal processes
        }

        private static void HookInflictDamageOverTime(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker); // Run normal processes

            // Cycle through On Inflict Damage Over Time callbacks
            foreach (OnInflictDamageOverTimeCallback callback in onInflictDamageOverTimeCallbacks)
            {
                // Call
                callback(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);
            }
        }

        private static void HookInflictDamageOverTimeRef(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            // Cycle through On Inflict Damage Over Time Ref callbacks
            foreach (OnInflictDamageOverTimeRefCallback callback in onInflictDamageOverTimeRefCallbacks)
            {
                // Call
                callback(ref inflictDotInfo);
            }

            orig(ref inflictDotInfo); // Run normal processes
        }

        private static void HookServerGiveItem(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            // Cycle through On Give Item callbacks
            foreach (OnTransferItemCallback callback in onGiveItemCallbacks)
            {
                // Call
                callback(self, itemIndex, count);
            }

            orig(self, itemIndex, count); // Run normal processes
        }

        private static void HookServerRemoveItem(On.RoR2.Inventory.orig_RemoveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            // Cycle through On Remove Item callbacks
            foreach (OnTransferItemCallback callback in onRemoveItemCallbacks)
            {
                // Call
                callback(self, itemIndex, count);
            }

            orig(self, itemIndex, count); // Run normal processes
        }

        private static void HookInventoryChanged(On.RoR2.Inventory.orig_HandleInventoryChanged orig, Inventory self)
        {
            orig(self); // Run normal processes

            // Cycle through On Inventory Changed callbacks
            foreach (OnInventoryCallback callback in onInventoryChangedCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookItemAdded(On.RoR2.Inventory.orig_RpcItemAdded orig, Inventory self, ItemIndex itemIndex)
        {
            orig(self, itemIndex); // Run normal processes

            // Get item def
            ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
            if (itemDef == null) return;

            // Check for item added callbacks
            if (onItemAddedCallbacks.ContainsKey(itemDef))
            {
                // Cycle through item added callbacks
                foreach (OnInventoryCallback callback in onItemAddedCallbacks[itemDef])
                {
                    // Call
                    callback(self);
                }
            }
        }

        private static void HookHealthComponentAwake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            // Add custom health component behaviour
            FaithfulHealthComponentBehaviour component = self.gameObject.AddComponent<FaithfulHealthComponentBehaviour>();

            orig(self); // Run normal processes
        }

        private static float HookHeal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            // Cycle through On Heal callbacks
            foreach (OnHealCallback callback in onHealCallbacks)
            {
                // Call
                callback(self, ref amount, ref procChainMask, ref nonRegen);
            }

            return orig(self, amount, procChainMask, nonRegen); // Run normal processes
        }

        private static void HookRecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
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

        private static void HookUpdateVisualEffects(On.RoR2.CharacterBody.orig_UpdateAllTemporaryVisualEffects orig, CharacterBody self)
        {
            orig(self); // Run normal processes

            // Cycle through On Update Visual Effects callbacks
            foreach (CharacterBodyCallback callback in onUpdateVisualEffectsCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookCharacterBodyFixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self); // Run normal processes

            // Cycle through callbacks
            foreach (CharacterBodyCallback callback in onCharacterBodyFixedUpdateCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookPurchaseInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
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

        private static bool HookPurchaseCanBeAfforded(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
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

        private static void HookProcessJump(On.EntityStates.GenericCharacterMain.orig_ProcessJump orig, EntityStates.GenericCharacterMain self)
        {
            orig(self); // Run normal processes first

            // Cycle through On Process Jump callbacks
            foreach (OnProcessJumpCallback callback in onProcessJumpCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookGenericCharacterFixedUpdate(On.EntityStates.GenericCharacterMain.orig_FixedUpdate orig, EntityStates.GenericCharacterMain self)
        {
            orig(self); // Run normal processes first

            // Cycle through Generic Character Fixed Update callbacks
            foreach (GenericCharacterCallback callback in genericCharacterFixedUpdateCallbacks)
            {
                // Call
                callback(self);
            }
        }

        private static void HookSceneExitControllerSetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            // Check if state is finished (ready to change scenes)
            if (newState == SceneExitController.ExitState.TeleportOut)
            {
                // Cycle through on scene exit callbacks
                foreach (SceneExitControllerCallback callback in onPreSceneExitCallbacks)
                {
                    // Call
                    callback(self);
                }
            }

            orig(self, newState); // Run normal processes
        }

        private static void HookOnDamageDealt(DamageReport _report)
        {
            // Cycle through On Damage Dealt callbacks
            foreach (DamageReportCallback callback in onDamageDealtCallbacks)
            {
                // Call
                callback(_report);
            }
        }

        private static void HookOnCharacterDeath(DamageReport _report)
        {
            // Cycle through On Character Death callbacks
            foreach (DamageReportCallback callback in onCharacterDeathCallbacks)
            {
                // Call
                callback(_report);
            }
        }

        private static void HookPrePopulateScene(SceneDirector _director)
        {
            // Cycle through callbacks
            foreach (SceneDirectorCallback callback in onPrePopulateSceneCallbacks)
            {
                // Call
                callback(_director);
            }
        }

        public static void OnIncomingDamageServer(DamageInfo _damageInfo, CharacterMaster _attacker, CharacterMaster _victim)
        {
            // Cycle through On Incoming Damage callbacks
            foreach (OnIncomingDamageCallback callback in onIncomingDamageCallbacks)
            {
                // Call
                callback(_damageInfo, _attacker, _victim);
            }
        }

        public static void DebugLog(string _message)
        {
            // Only log behaviour on debug
            if (Utils.debugMode)
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
