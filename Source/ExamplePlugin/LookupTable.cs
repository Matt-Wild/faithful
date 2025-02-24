using System.Collections.Generic;

namespace Faithful
{
    internal static class LookupTable
    {
        // Dictionary for the int lookup table
        private static Dictionary<string, int> m_intLookup = new Dictionary<string, int>();

        public static int GetInt(string _lookup, int _fallback = 0)
        {
            // Check for lookup
            if (!m_intLookup.ContainsKey(_lookup)) return _fallback;

            // Return value from lookup
            return m_intLookup[_lookup];
        }

        public static void SetInt(string _lookup, int _value)
        {
            // Set value in lookup
            m_intLookup[_lookup] = _value;
        }
    }
}
