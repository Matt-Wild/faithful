using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugToggle : MonoBehaviour
    {
        // Store reference to toggle
        protected Toggle toggle;

        // Store toggle colours
        protected ColorBlock normalColours;
        protected ColorBlock selectedColours;

        // Store callbacks for when toggle is toggled on or off
        protected Callback onCallback;
        protected Callback offCallback;

        public void Init(Callback _onCallback, Callback _offCallback)
        {
            // Assign callbacks
            onCallback = _onCallback;
            offCallback = _offCallback;
        }

        private void Awake()
        {
            // Get toggle
            toggle = GetComponent<Toggle>();

            // Create colour blocks
            normalColours = toggle.colors;
            normalColours.selectedColor = normalColours.normalColor;
            selectedColours = toggle.colors;
            selectedColours.normalColor = selectedColours.selectedColor;

            // Link on toggle changed behaviour
            toggle.onValueChanged.AddListener(SetState);
        }

        public void SetState(bool _state)
        {
            // Update the toggle without triggering the same callback again
            SetStateWithoutNotify(_state);

            // Check toggle state
            if (_state)
            {
                // Call on callback
                onCallback();
            }
            else
            {
                // Call off callback
                offCallback();
            }
        }

        public void SetStateWithoutNotify(bool _state)
        {
            // Update the state and colours without running the associated callbacks
            toggle.SetIsOnWithoutNotify(_state);
            toggle.colors = _state ? selectedColours : normalColours;
        }
    }
}
