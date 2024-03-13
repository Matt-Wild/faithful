using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class DebugController : MonoBehaviour
    {
        // Toolbox
        protected Toolbox toolbox;

        // Store references to debug panels
        public DebugMain debugMain;
        public DebugStatsMonitor debugStatsMonitor;
        public DebugSpawnMenu debugSpawnMenu;

        public void Init(Toolbox _toolbox)
        {
            // Assign toolbox
            toolbox = _toolbox;

            // Add main behaviour to main panel
            debugMain = transform.Find("DebugMainPanel").gameObject.AddComponent<DebugMain>();
            debugMain.Init(toolbox, this, true);

            // Add stats monitor to stats panel
            debugStatsMonitor = transform.Find("DebugStatsPanel").gameObject.AddComponent<DebugStatsMonitor>();
            debugStatsMonitor.Init(toolbox, this);

            // Add spawn menu behaviour
            debugSpawnMenu = transform.Find("DebugSpawnPanel").gameObject.AddComponent<DebugSpawnMenu>();
            debugSpawnMenu.Init(toolbox, this);

            // Create panel toggles
            debugMain.CreateToggles();
        }

        public CharacterBody localBody
        {
            get
            {
                // Return local player body from utils
                return toolbox.utils.localPlayerBody;
            }
        }
    }
}
