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

            // Add stage controls toggle
            DebugPanelToggle toggle3 = transform.Find("StageControlsToggle").gameObject.AddComponent<DebugPanelToggle>();
            toggle3.Init(debugController.debugStageControls);

            // Add object analysis toggle
            DebugPanelToggle toggle4 = transform.Find("ObjectAnalysisToggle").gameObject.AddComponent<DebugPanelToggle>();
            toggle4.Init(debugController.debugObjectAnalysis);
        }
    }
}
