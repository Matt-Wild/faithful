using UnityEngine;

namespace Faithful
{
    internal class DebugController : MonoBehaviour
    {
        void Awake()
        {
            // Add main behaviour to main panel
            DebugMain debugMain = transform.Find("DebugMainPanel").gameObject.AddComponent<DebugMain>();
            debugMain.Init(this);

            // Add stats monitor to stats panel
            DebugStatsMonitor debugStatsMonitor = transform.Find("DebugStatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
            debugStatsMonitor.Init(this);
        }
    }
}
