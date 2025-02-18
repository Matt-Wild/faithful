using System.Collections.Generic;

namespace Faithful
{
    internal static class Interactables
    {
        // List of all interactables
        private static List<Interactable> m_interactables = new List<Interactable>();

        public static void RegisterInteractable(Interactable _interactable)
        {
            // Add to interactables list
            m_interactables.Add(_interactable);
        }

        public static Interactable FetchInteractable(string _token)
        {
            // Cycle through interactables
            foreach (Interactable interactable in m_interactables)
            {
                // Check token
                if (interactable.token == _token) return interactable;
            }

            // Didn't find interactable
            return null;
        }

        // Accessors
        public static List<Interactable> interactables { get { return m_interactables; } }
    }
}
