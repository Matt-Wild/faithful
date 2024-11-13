using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugToolsMenu : DebugPanel
    {
        public override void Init(DebugController _debugController, bool _startOpen = false)
        {
            // Call base init
            base.Init(_debugController, _startOpen);

            // Add object analysis toggle
            DebugPanelToggle toggle = transform.Find("ObjectAnalysisToggle").gameObject.AddComponent<DebugPanelToggle>();
            toggle.Init(debugController.debugObjectAnalysis);
        }
    }
}
