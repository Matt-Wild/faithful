using UnityEngine;

namespace Faithful
{
    internal class DebugMain : MonoBehaviour
    {
        void Awake()
        {
            // Add stats monitor to stats panel
            transform.Find("StatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
        }
    }
}
