using System.Collections.Generic;
using RoR2;

namespace Faithful
{
    internal class CharacterScopedCallbacks<T> where T : System.Delegate
    {
        private readonly Dictionary<CharacterBody, T[]> callbacks = new();

        public void Add(CharacterBody _body, T _callback)
        {
            if (_body == null || _callback == null) return;
            T[] current = Get(_body);
            if (System.Array.IndexOf(current, _callback) >= 0) return;

            // Replace the snapshot only when registrations change
            T[] updated = new T[current.Length + 1];
            System.Array.Copy(current, updated, current.Length);
            updated[current.Length] = _callback;
            callbacks[_body] = updated;
        }

        public void Remove(CharacterBody _body, T _callback)
        {
            // Destroyed Unity bodies must still be removable from the dictionary
            if (ReferenceEquals(_body, null) || !callbacks.TryGetValue(_body, out T[] current)) return;
            int index = System.Array.IndexOf(current, _callback);
            if (index < 0) return;
            if (current.Length == 1)
            {
                callbacks.Remove(_body);
                return;
            }

            T[] updated = new T[current.Length - 1];
            System.Array.Copy(current, 0, updated, 0, index);
            System.Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
            callbacks[_body] = updated;
        }

        public T[] Get(CharacterBody _body)
        {
            // Stable snapshots allow registration changes during callbacks without per-call allocations
            return _body != null && callbacks.TryGetValue(_body, out T[] current) ? current : System.Array.Empty<T>();
        }
    }
}
