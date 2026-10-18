using TMPro;
using UnityEngine;

namespace Faithful
{
    internal class DebugGameControls : DebugPanel
    {
        // Store reference to time scale input
        protected TMP_InputField timeScaleInput;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find time scale input
            timeScaleInput = transform.Find("TimeScaleInputField").gameObject.GetComponent<TMP_InputField>();

            // Add time scale input behaviour
            timeScaleInput.onValueChanged.AddListener(OnTimeScaleChanged);
        }

        private void OnTimeScaleChanged(string _newTimeScale)
        {
            // Attempt to convert to float
            if (float.TryParse(_newTimeScale, out float result))
            {
                // Apply new time scale
                Time.timeScale = result;

                // Log new time scale
                Log.Debug($"[DEBUG MENU] | Changed time scale to: {result}.");
            }
        }
    }
}
