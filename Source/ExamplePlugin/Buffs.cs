using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal static class Buffs
    {
        // List of buffs
        static List<Buff> buffs;

        public static void Init()
        {
            // Initialise items list
            buffs = new List<Buff>();
        }

        public static Buff AddBuff(string _token, string _iconDir, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false)
        {
            // Create buff
            Buff newBuff = new Buff(_token, _iconDir, _colour, _canStack, _isDebuff, _isHidden);

            // Add buff to buffs list
            buffs.Add(newBuff);

            // Return new buff
            return newBuff;
        }

        public static Buff GetBuff(string _token)
        {
            // Cycle through buffs
            foreach (Buff buff in buffs)
            {
                // Check if correct token
                if (buff.token == _token)
                {
                    // Return buff
                    return buff;
                }
            }

            // Return null if not found
            Log.Error($"Attempted to fetch buff '{_token}' but couldn't find it");
            return null;
        }
    }
}
