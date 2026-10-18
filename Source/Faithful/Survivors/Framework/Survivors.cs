using System;
using System.Collections.Generic;
using System.Text;

namespace Faithful
{
    internal class Survivors
    {
        // List of all survivors
        private static List<Survivor> m_survivors = new List<Survivor>();

        public static void RegisterSurvivor(Survivor _survivor)
        {
            // Add to survivors list
            m_survivors.Add(_survivor);
        }

        public static Survivor FetchSurvivor(string _token)
        {
            // Cycle through survivors
            foreach (Survivor survivor in m_survivors)
            {
                // Check token
                if (survivor.token == _token) return survivor;
            }

            // Didn't find survivor
            return null;
        }
    }
}
