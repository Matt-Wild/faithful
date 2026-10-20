using System.Collections.Generic;
using RoR2;

namespace Faithful
{
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
            int itemCount = inventory.GetItemCountEffective(item.itemDef);

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
            int itemCount = inventory.GetItemCountEffective(item.itemDef);

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
