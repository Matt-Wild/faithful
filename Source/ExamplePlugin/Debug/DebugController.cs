using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class DebugController : MonoBehaviour
    {
        // Store references to debug panels
        public DebugMain debugMain;
        public DebugStatsMonitor debugStatsMonitor;
        public DebugSpawnMenu debugSpawnMenu;
        public DebugStageControls debugStageControls;
        public DebugObjectAnalysis debugObjectAnalysis;
        public DebugComponentAnalysis debugComponentAnalysis;

        public void Init()
        {
            // Add main behaviour to main panel
            debugMain = transform.Find("DebugMainPanel").gameObject.AddComponent<DebugMain>();
            debugMain.Init(this, true);

            // Add stats monitor to stats panel
            debugStatsMonitor = transform.Find("DebugStatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
            debugStatsMonitor.Init(this);

            // Add spawn menu behaviour
            debugSpawnMenu = transform.Find("DebugSpawnPanel").gameObject.AddComponent<DebugSpawnMenu>();
            debugSpawnMenu.Init(this);

            // Add stage controls menu behaviour
            debugStageControls = transform.Find("DebugStagePanel").gameObject.AddComponent<DebugStageControls>();
            debugStageControls.Init(this);

            // Add object analysis behaviour
            debugObjectAnalysis = transform.Find("DebugObjectAnalysisPanel").gameObject.AddComponent<DebugObjectAnalysis>();
            debugObjectAnalysis.Init(this);

            // Add object component behaviour
            debugComponentAnalysis = transform.Find("DebugComponentAnalysisPanel").gameObject.AddComponent<DebugComponentAnalysis>();
            debugComponentAnalysis.Init(this);

            // Link debug object analysis and debug component analysis
            debugObjectAnalysis.AssignComponentAnalyser(debugComponentAnalysis);
            debugComponentAnalysis.AssignObjectAnalyser(debugObjectAnalysis);

            // Create panel toggles
            debugMain.CreateToggles();
        }

        public CharacterBody localBody
        {
            get
            {
                // Return local player body from utils
                return Utils.localPlayerBody;
            }
        }
    }
}
