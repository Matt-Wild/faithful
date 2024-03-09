using UnityEngine;

namespace Faithful
{
    internal class DebugPanel : MonoBehaviour
    {
        public virtual void Awake()
        {
            // Add Debug Tab Buttons behaviour
            gameObject.AddComponent<DebugPanelButtons>();
        }
    }
}
