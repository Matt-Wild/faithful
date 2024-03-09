using UnityEngine;

namespace Faithful
{
    internal class DebugController : MonoBehaviour
    {
        void Awake()
        {
            // Add main behaviour to main panel
            transform.Find("DebugMainPanel").gameObject.AddComponent<DebugMain>();

            // Add stats monitor to stats panel
            transform.Find("DebugStatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
        }
    }
}
