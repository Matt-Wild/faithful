using System.Collections.Generic;
using R2API;
using RoR2;

namespace Faithful
{
    internal delegate void Callback();

    internal delegate void InHoldoutZoneCallback(CharacterBody _contained, HoldoutZoneController _zone);

    internal delegate void StatsModCallback(int _count, RecalculateStatsAPI.StatHookEventArgs _stats);

    internal delegate void OnDamageDealtCallback(DamageReport _report);

    internal class Behaviour
    {
        // Toolbox
        protected Toolbox toolbox;

        // Update callbacks
        protected List<Callback> updateCallbacks = new List<Callback>();
        protected List<Callback> debugUpdateCallbacks = new List<Callback>();
        protected List<Callback> fixedUpdateCallbacks = new List<Callback>();
        protected List<Callback> debugFixedUpdateCallbacks = new List<Callback>();

        // In Holdout Zone callback functions
        protected List<InHoldoutZoneCallback> inHoldoutZoneCallbacks = new List<InHoldoutZoneCallback>();

        // Stat modification item and buff callbacks
        List<ItemStatsMod> itemStatsMods = new List<ItemStatsMod>();
        List<BuffStatsMod> buffStatsMods = new List<BuffStatsMod>();

        // On Damage Dealt callback functions
        protected List<OnDamageDealtCallback> onDamageDealtCallbacks = new List<OnDamageDealtCallback>();

        // Constructor
        public Behaviour(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Inject hooks
            On.RoR2.HoldoutZoneController.FixedUpdate += HookHoldoutZoneControllerFixedUpdate;
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

        // Add In Holdout Zone callback
        public void AddInHoldoutZoneCallback(InHoldoutZoneCallback _callback)
        {
            inHoldoutZoneCallbacks.Add(_callback);

            Log.Debug("Added Holdout Zone behaviour");
        }

        // Add On Damage Dealt callback
        public void AddOnDamageDealtCallback(OnDamageDealtCallback _callback)
        {
            onDamageDealtCallbacks.Add(_callback);

            Log.Debug("Added On Damage Dealt behaviour");
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
}
