using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Faithful
{
    internal class ItemBase
    {
        // Toolbox
        protected Toolbox toolbox;

        // Constructor
        public ItemBase(Toolbox _toolbox)
        {
            // Assign toolbox
            toolbox = _toolbox;

            // Register with utils
            Utils.RegisterItemBehaviour(this);
        }

        protected virtual void CreateSettings()
        {
            // Check if debug mode
            if (Utils.debugMode)
            {
                // Log warning
                Log.Warning($"[ITEM] - Item missing 'CreateSettings' method.");
            }
        }

        public virtual void FetchSettings()
        {
            // Check if debug mode
            if (Utils.debugMode)
            {
                // Log warning
                Log.Warning($"[ITEM] - Item missing 'FetchSettings' method.");
            }
        }
    }
}
