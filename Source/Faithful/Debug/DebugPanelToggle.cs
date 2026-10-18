using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugPanelToggle : DebugToggle
    {
        // Store reference to debug panel
        protected DebugPanel panel;

        public void Init(DebugPanel _panel)
        {
            // Initialise base class
            Init(OnToggleOn, OnToggleOff);

            // Assign panel
            panel = _panel;

            // Assign toggle to debug panel buttons
            DebugPanelButtons buttons = panel.gameObject.GetComponent<DebugPanelButtons>();
            if (buttons != null)
            {
                // Assign panel toggle
                buttons.AssignPanelToggle(this);
            }
        }

        protected void OnToggleOn()
        {
            // Open panel
            panel.Open();
        }

        protected void OnToggleOff()
        {
            // Close panel
            panel.Close();
        }
    }
}
