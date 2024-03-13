namespace Faithful
{
    internal class DebugMain : DebugPanel
    {
        public override void Awake()
        {
            // Call base class Awake
            base.Awake();
        }

        public void CreateToggles()
        {
            // Add stats monitor toggle
            DebugPanelToggle toggle1 = transform.Find("StatsMonitorToggle").gameObject.AddComponent<DebugPanelToggle>();
            toggle1.Init(debugController.debugStatsMonitor);

            // Add spawn menu toggle
            DebugPanelToggle toggle2 = transform.Find("SpawnMenuToggle").gameObject.AddComponent<DebugPanelToggle>();
            toggle2.Init(debugController.debugSpawnMenu);
        }
    }
}
