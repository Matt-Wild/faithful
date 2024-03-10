using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugPanelToggle : MonoBehaviour
    {
        // Store reference to toggle and debug panel
        protected Toggle toggle;
        protected DebugPanel panel;

        // Store toggle colours
        protected ColorBlock normalColours;
        protected ColorBlock selectedColours;

        public void Init(DebugPanel _panel)
        {
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

        void Awake()
        {
            // Get toggle
            toggle = GetComponent<Toggle>();

            // Create colour blocks
            normalColours = toggle.colors;
            normalColours.selectedColor = normalColours.normalColor;
            selectedColours = toggle.colors;
            selectedColours.normalColor = selectedColours.selectedColor;

            // Link on panel toggle changed behaviour
            toggle.onValueChanged.AddListener(SetState);
        }

        public void SetState(bool _state)
        {
            // Ensure state
            toggle.isOn = _state;

            // Check toggle state
            if (_state)
            {
                // Open panel
                panel.Open();

                // Set toggle colours
                toggle.colors = selectedColours;
            }
            else
            {
                // Close panel
                panel.Close();

                // Set toggle colours
                toggle.colors = normalColours;
            }
        }
    }
}
