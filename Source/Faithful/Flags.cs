using IL.RoR2.Mecanim;
using System;
using System.Collections.Generic;
using System.Text;

namespace Faithful
{
    internal class Flags
    {
        // Store list of flags
        private List<Flag> flags = new List<Flag>();

        // Reset all flags
        public void Reset()
        {
            // Clear flags
            flags.Clear();
        }

        // Get a flag
        public bool Get(string _token)
        {
            // Cycle through flags
            foreach (Flag flag in flags)
            {
                // Check if correct flag
                if (flag.token == _token)
                {
                    // Return flag state
                    return flag.state;
                }
            }

            // Return false if not found
            return false;
        }

        // Set a flag
        public void Set(string _token, bool _state = true)
        {
            // Cycle through flags
            foreach (Flag flag in flags)
            {
                // Check if correct flag
                if (flag.token == _token)
                {
                    // Set flag state
                    flag.state = _state;
                }
            }

            // If not found create new flag
            flags.Add(new Flag(_token, _state));
        }
    }

    internal class Flag
    {
        // Store token and state
        public string token;
        public bool state;

        // Constructor
        public Flag(string _token, bool _state = false)
        {
            // Assign token and state
            token = _token;
            state = _state;
        }
    }
}
