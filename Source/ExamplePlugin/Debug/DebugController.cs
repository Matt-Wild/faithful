using UnityEngine;

namespace Faithful
{
    internal class DebugController : MonoBehaviour
    {
        // Store references to debug panels
        public DebugMain debugMain;
        public DebugStatsMonitor debugStatsMonitor;

        void Awake()
        {
            // Add main behaviour to main panel
            debugMain = transform.Find("DebugMainPanel").gameObject.AddComponent<DebugMain>();
            debugMain.Init(this, true);

            // Add stats monitor to stats panel
            debugStatsMonitor = transform.Find("DebugStatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
            debugStatsMonitor.Init(this);

            // Create panel toggles
            debugMain.CreateToggles();
        }
    }
}
