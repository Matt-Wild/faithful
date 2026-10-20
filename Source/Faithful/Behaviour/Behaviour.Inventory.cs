using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace Faithful
{
    internal delegate void OnTransferItemCallback(Inventory _inventory, ItemIndex _index, int _count);

    internal delegate void OnTransferItemFloatCallback(Inventory _inventory, ItemIndex _index, float _count);

    internal delegate void OnInventoryCallback(Inventory _inventory);

    internal delegate void PushPickupNotificationCallback(CharacterMaster characterMaster, UniquePickup uniquePickup);

    internal static partial class Behaviour
    {
        // Permanent items: server callbacks run before the original method, late callbacks after
        private static readonly List<OnTransferItemCallback> onGiveItemPermanentCallbacks = new List<OnTransferItemCallback>();

        private static readonly List<OnTransferItemCallback> onGiveItemPermanentLateCallbacks = new List<OnTransferItemCallback>();

        private static readonly HookRegistration serverGiveItemPermanentHook = new("Inventory.GiveItemPermanent_ItemIndex_int",
            () => On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += HookServerGiveItemPermanent,
            () => On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int -= HookServerGiveItemPermanent);

        // Add On Give Item Permanent callback (SERVER ONLY)
        public static void AddServerOnGiveItemPermanentCallback(OnTransferItemCallback _callback)
        {
            onGiveItemPermanentCallbacks.Add(_callback);

            DebugLog("Added On Give Item Permanent behaviour");
        }

        // Add On Give Item Permanent Late callback (SERVER ONLY)
        public static void AddServerOnGiveItemPermanentLateCallback(OnTransferItemCallback _callback)
        {
            onGiveItemPermanentLateCallbacks.Add(_callback);

            DebugLog("Added On Give Item Permanent Late behaviour");
        }

        private static void HookServerGiveItemPermanent(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            if (!CanRunHook(serverGiveItemPermanentHook))
            {
                orig(self, itemIndex, countToAdd);
                return;
            }

            serverGiveItemPermanentHook.StartProfiling();
            try
            {
                // Cycle through On Give Item callbacks
                foreach (OnTransferItemCallback callback in onGiveItemPermanentCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemPermanentHook.StopProfiling();
            }

            orig(self, itemIndex, countToAdd); // Run normal processes

            serverGiveItemPermanentHook.StartProfiling();
            try
            {
                // Cycle through On Give Item Late callbacks
                foreach (OnTransferItemCallback callback in onGiveItemPermanentLateCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemPermanentHook.StopProfiling();
            }
        }

        // Temporary items: server callbacks run before the original method, late callbacks after
        private static readonly List<OnTransferItemFloatCallback> onGiveItemTemporaryCallbacks = new List<OnTransferItemFloatCallback>();

        private static readonly List<OnTransferItemFloatCallback> onGiveItemTemporaryLateCallbacks = new List<OnTransferItemFloatCallback>();

        private static readonly HookRegistration serverGiveItemTemporaryHook = new("Inventory.GiveItemTemp",
            () => On.RoR2.Inventory.GiveItemTemp += HookServerGiveItemTemporary,
            () => On.RoR2.Inventory.GiveItemTemp -= HookServerGiveItemTemporary);

        // Add On Give Item Temporary callback (SERVER ONLY)
        public static void AddServerOnGiveItemTemporaryCallback(OnTransferItemFloatCallback _callback)
        {
            onGiveItemTemporaryCallbacks.Add(_callback);

            DebugLog("Added On Give Item Temporary behaviour");
        }

        // Add On Give Item Temporary Late callback (SERVER ONLY)
        public static void AddServerOnGiveItemTemporaryLateCallback(OnTransferItemFloatCallback _callback)
        {
            onGiveItemTemporaryLateCallbacks.Add(_callback);

            DebugLog("Added On Give Item Temporary Late behaviour");
        }

        private static void HookServerGiveItemTemporary(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float countToAdd)
        {
            if (!CanRunHook(serverGiveItemTemporaryHook))
            {
                orig(self, itemIndex, countToAdd);
                return;
            }

            serverGiveItemTemporaryHook.StartProfiling();
            try
            {
                // Cycle through On Give Item callbacks
                foreach (OnTransferItemFloatCallback callback in onGiveItemTemporaryCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemTemporaryHook.StopProfiling();
            }

            orig(self, itemIndex, countToAdd); // Run normal processes

            serverGiveItemTemporaryHook.StartProfiling();
            try
            {
                // Cycle through On Give Item Late callbacks
                foreach (OnTransferItemFloatCallback callback in onGiveItemTemporaryLateCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemTemporaryHook.StopProfiling();
            }
        }

        // Ethereal items: server callbacks run before the original method, late callbacks after
        private static readonly List<OnTransferItemCallback> onGiveItemRentedCallbacks = new List<OnTransferItemCallback>();

        private static readonly List<OnTransferItemCallback> onGiveItemRentedLateCallbacks = new List<OnTransferItemCallback>();

        private static readonly HookRegistration serverGiveItemRentedHook = new("Inventory.GiveItemRented",
            () => On.RoR2.Inventory.GiveItemRented += HookServerGiveItemRented,
            () => On.RoR2.Inventory.GiveItemRented -= HookServerGiveItemRented);

        // Add On Give Item Rented callback (SERVER ONLY)
        public static void AddServerOnGiveItemRentedCallback(OnTransferItemCallback _callback)
        {
            onGiveItemRentedCallbacks.Add(_callback);

            DebugLog("Added On Give Item Rented behaviour");
        }

        // Add On Give Item Rented Late callback (SERVER ONLY)
        public static void AddServerOnGiveItemRentedLateCallback(OnTransferItemCallback _callback)
        {
            onGiveItemRentedLateCallbacks.Add(_callback);

            DebugLog("Added On Give Item Rented Late behaviour");
        }

        private static void HookServerGiveItemRented(On.RoR2.Inventory.orig_GiveItemRented orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            if (!CanRunHook(serverGiveItemRentedHook) || !NetworkServer.active)
            {
                orig(self, itemIndex, countToAdd);
                return;
            }

            serverGiveItemRentedHook.StartProfiling();
            try
            {
                // Cycle through On Give Item callbacks
                foreach (OnTransferItemCallback callback in onGiveItemRentedCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemRentedHook.StopProfiling();
            }

            orig(self, itemIndex, countToAdd); // Run normal processes

            serverGiveItemRentedHook.StartProfiling();
            try
            {
                // Cycle through On Give Item Late callbacks
                foreach (OnTransferItemCallback callback in onGiveItemRentedLateCallbacks)
                {
                    // Call
                    callback(self, itemIndex, countToAdd);
                }
            }
            finally
            {
                serverGiveItemRentedHook.StopProfiling();
            }
        }

        // Permanent item removal: server callbacks run before the original method
        private static readonly List<OnTransferItemCallback> onRemoveItemCallbacks = new List<OnTransferItemCallback>();

        private static readonly HookRegistration serverRemoveItemHook = new("Inventory.RemoveItemPermanent_ItemIndex_int",
            () => On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int += HookServerRemoveItem,
            () => On.RoR2.Inventory.RemoveItemPermanent_ItemIndex_int -= HookServerRemoveItem);

        // Add On Remove Item callback (SERVER ONLY)
        public static void AddServerOnRemoveItemCallback(OnTransferItemCallback _callback)
        {
            onRemoveItemCallbacks.Add(_callback);

            DebugLog("Added On Remove Item behaviour");
        }

        private static void HookServerRemoveItem(On.RoR2.Inventory.orig_RemoveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            if (!CanRunHook(serverRemoveItemHook))
            {
                orig(self, itemIndex, count);
                return;
            }

            serverRemoveItemHook.StartProfiling();
            try
            {
                // Cycle through On Remove Item callbacks
                foreach (OnTransferItemCallback callback in onRemoveItemCallbacks)
                {
                    // Call
                    callback(self, itemIndex, count);
                }
            }
            finally
            {
                serverRemoveItemHook.StopProfiling();
            }

            orig(self, itemIndex, count); // Run normal processes
        }

        // Inventory changes: callbacks run after the original method and helper notification
        private static readonly List<OnInventoryCallback> onInventoryChangedCallbacks = new List<OnInventoryCallback>();

        private static readonly HookRegistration inventoryChangedHook = new("Inventory.HandleInventoryChanged",
            () => On.RoR2.Inventory.HandleInventoryChanged += HookInventoryChanged,
            () => On.RoR2.Inventory.HandleInventoryChanged -= HookInventoryChanged);

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

        private static void HookInventoryChanged(On.RoR2.Inventory.orig_HandleInventoryChanged orig, Inventory self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(inventoryChangedHook)) return;

            inventoryChangedHook.StartProfiling();
            try
            {
                // Notify only the character which owns this inventory
                CharacterBody body = Utils.GetInventoryBody(self);
                FaithfulCharacterBodyBehaviour helper = Utils.FindCharacterBodyHelper(body, _warnIfMissing: false);
                if (helper != null) helper.OnInventoryChanged(self);

                // Cycle through On Inventory Changed callbacks
                foreach (OnInventoryCallback callback in onInventoryChangedCallbacks)
                {
                    // Call
                    callback(self);
                }
            }
            finally
            {
                inventoryChangedHook.StopProfiling();
            }
        }

        // Body inventory changes: callbacks run after the original method
        private static readonly List<CharacterBodyCallback> onBodyInventoryChangedCallbacks = new List<CharacterBodyCallback>();

        private static readonly HookRegistration bodyInventoryChangedHook = new("CharacterBody.OnInventoryChanged",
            () => On.RoR2.CharacterBody.OnInventoryChanged += HookBodyInventoryChanged,
            () => On.RoR2.CharacterBody.OnInventoryChanged -= HookBodyInventoryChanged);

        public static void AddOnBodyInventoryChangedCallback(CharacterBodyCallback _callback)
        {
            onBodyInventoryChangedCallbacks.Add(_callback);

            DebugLog("Added CharacterBody.OnInventoryChanged behaviour");
        }

        private static void HookBodyInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(bodyInventoryChangedHook)) return;

            bodyInventoryChangedHook.StartProfiling();
            try
            {
                // Cycle through body inventory changed callbacks after the body has processed the change
                foreach (CharacterBodyCallback callback in onBodyInventoryChangedCallbacks)
                {
                    callback(self);
                }
            }
            finally
            {
                bodyInventoryChangedHook.StopProfiling();
            }
        }

        // Inventory destruction: callbacks run after the original method
        private static readonly List<OnInventoryCallback> onInventoryDestroyedCallbacks = new List<OnInventoryCallback>();

        private static readonly HookRegistration inventoryDestroyedHook = new("Inventory.OnDestroy",
            () => On.RoR2.Inventory.OnDestroy += HookInventoryDestroyed,
            () => On.RoR2.Inventory.OnDestroy -= HookInventoryDestroyed);

        public static void AddOnInventoryDestroyedCallback(OnInventoryCallback _callback)
        {
            onInventoryDestroyedCallbacks.Add(_callback);

            DebugLog("Added Inventory.OnDestroy behaviour");
        }

        private static void HookInventoryDestroyed(On.RoR2.Inventory.orig_OnDestroy orig, Inventory self)
        {
            orig(self); // Run normal processes
            if (!CanRunHook(inventoryDestroyedHook)) return;

            inventoryDestroyedHook.StartProfiling();
            try
            {
                // Pass the inventory reference through even when Unity considers it destroyed
                foreach (OnInventoryCallback callback in onInventoryDestroyedCallbacks)
                {
                    callback(self);
                }
            }
            finally
            {
                inventoryDestroyedHook.StopProfiling();
            }
        }

        // Item notifications: callbacks run after the original method
        private static readonly Dictionary<ItemDef, List<OnInventoryCallback>> onItemAddedCallbacks = new Dictionary<ItemDef, List<OnInventoryCallback>>();

        private static readonly HookRegistration itemAddedHook = new("Inventory.RpcItemAdded",
            () => On.RoR2.Inventory.RpcItemAdded += HookItemAdded,
            () => On.RoR2.Inventory.RpcItemAdded -= HookItemAdded);

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

        private static void HookItemAdded(On.RoR2.Inventory.orig_RpcItemAdded orig, Inventory self, ItemIndex itemIndex)
        {
            orig(self, itemIndex); // Run normal processes
            if (!CanRunHook(itemAddedHook)) return;

            itemAddedHook.StartProfiling();
            try
            {
                // Get item def
                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                if (itemDef == null) return;

                // Check if quality is enabled
                if (Utils.qualityEnabled)
                {
                    // Ensure to check for base item instead of quality item
                    itemDef = QualityCompat.GetBaseItem(itemDef);   // This returns the original item def is no match is found
                }

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
            finally
            {
                itemAddedHook.StopProfiling();
            }
        }

        // Pickup notifications: callbacks run after the original method
        private static readonly List<PushPickupNotificationCallback> pushPickupNotificationCallbacks = new List<PushPickupNotificationCallback>();

        private static readonly HookRegistration pushPickupNotificationHook = new("RoR2.CharacterMasterNotificationQueue.PushPickupNotification_CharacterMaster_UniquePickup",
            () => On.RoR2.CharacterMasterNotificationQueue.PushPickupNotification_CharacterMaster_UniquePickup += HookPushPickupNotification,
            () => On.RoR2.CharacterMasterNotificationQueue.PushPickupNotification_CharacterMaster_UniquePickup -= HookPushPickupNotification);

        public static void AddPushPickupNotificationCallback(PushPickupNotificationCallback _callback)
        {
            pushPickupNotificationCallbacks.Add(_callback);

            DebugLog("Added PushPickupNotification behaviour");
        }

        private static void HookPushPickupNotification(On.RoR2.CharacterMasterNotificationQueue.orig_PushPickupNotification_CharacterMaster_UniquePickup orig, CharacterMaster characterMaster, UniquePickup uniquePickup)
        {
            orig(characterMaster, uniquePickup); // Run normal processes
            if (!CanRunHook(pushPickupNotificationHook)) return;

            pushPickupNotificationHook.StartProfiling();
            try
            {
                // Cycle through PushPickupNotification callbacks
                foreach (PushPickupNotificationCallback callback in pushPickupNotificationCallbacks)
                {
                    callback(characterMaster, uniquePickup);
                }
            }
            finally
            {
                pushPickupNotificationHook.StopProfiling();
            }
        }
    }
}
