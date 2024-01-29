using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal class Buffs
    {
        // Toolbox
        protected Toolbox toolbox;

        // List of buffs
        List<Buff> buffs;

        // Constructor
        public Buffs(Toolbox _toolbox)
        {
            toolbox = _toolbox;

            // Initialise items list
            buffs = new List<Buff>();
        }

        public Buff AddBuff(string _token, string _iconDir, Color _colour, bool _canStack = true, bool _isDebuff = false, bool _isHidden = false)
        {
            // Create buff
            Buff newBuff = new Buff(toolbox, _token, _iconDir, _colour, _canStack, _isDebuff, _isHidden);

            // Add buff to buffs list
            buffs.Add(newBuff);

            // Return new buff
            return newBuff;
        }

        public Buff GetBuff(string _token)
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
            return null;
        }
    }
}
