using RoR2;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Faithful
{
    internal class DebugGameControls : DebugPanel
    {
        // Store reference to time scale input
        protected TMP_InputField timeScaleInput;

        // Store reference to run time input
        protected TMP_InputField runTimeInput;
        bool runTimeChanged;

        // Store stage selection controls and destinations
        protected TMP_Dropdown stageDropdown;
        protected UnityEngine.UI.Button stageGoButton;
        Dictionary<string, SceneDef> stageLookup = new();

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Find time scale input
            timeScaleInput = transform.Find("TimeScaleInputField").gameObject.GetComponent<TMP_InputField>();

            // Add time scale input behaviour
            timeScaleInput.onValueChanged.AddListener(OnTimeScaleChanged);

            // Find run time input and apply changes after editing
            runTimeInput = transform.Find("RunTimeInputField").gameObject.GetComponent<TMP_InputField>();
            runTimeInput.onValueChanged.AddListener(_value => runTimeChanged = true);
            runTimeInput.onEndEdit.AddListener(OnRunTimeChanged);

            // Find stage selection controls
            stageDropdown = DebugSelectionDropdown.Create(transform.Find("StageDropdown").gameObject.GetComponent<TMP_Dropdown>());
            stageGoButton = transform.Find("StageGoButton").gameObject.GetComponent<UnityEngine.UI.Button>();
            stageGoButton.onClick.AddListener(OnGoToStage);
            CreateStageOptions();
        }

        private void CreateStageOptions()
        {
            // Discover gameplay scenes, including hidden realms and stage variants
            List<string> stageNames = new();
            foreach (SceneDef scene in SceneCatalog.allSceneDefs)
            {
                if (scene.requiredExpansion && (!Run.instance || !Run.instance.IsExpansionEnabled(scene.requiredExpansion))) continue;

                // Include the internal name to distinguish scenes with the same display name
                string title = string.IsNullOrEmpty(scene.nameToken) ? scene.cachedName : Language.GetString(scene.nameToken);
                string name = title;
                stageNames.Add(name);
                stageLookup[name] = scene;
            }

            // Keep the list alphabetical and reuse the existing selection dropdown
            stageNames.Sort();
            stageDropdown.ClearOptions();
            stageDropdown.AddOptions(stageNames);
        }

        private void Update()
        {
            // Only the host can change run time or travel to another stage
            bool canChange = NetworkServer.active && Run.instance;
            runTimeInput.interactable = canChange;
            stageDropdown.interactable = canChange && Stage.instance && stageDropdown.options.Count > 0;
            stageGoButton.interactable = stageDropdown.interactable;

            // Display run time in minutes without interrupting editing
            if (!runTimeInput.isFocused)
            {
                string time = Run.instance ? (Run.instance.GetRunStopwatch() / 60.0f).ToString("0.##", CultureInfo.InvariantCulture) : "";
                if (runTimeInput.text != time) runTimeInput.SetTextWithoutNotify(time);
            }
        }

        private void OnRunTimeChanged(string _newRunTime)
        {
            // Do not change the timer unless the input was edited
            if (!runTimeChanged) return;

            runTimeChanged = false;
            if (runTimeInput.wasCanceled || !NetworkServer.active || !Run.instance) return;
            if (!float.TryParse(_newRunTime, NumberStyles.Float, CultureInfo.InvariantCulture, out float minutes)) return;

            // Convert minutes to seconds and reject invalid run times
            float seconds = minutes * 60.0f;
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0.0f) return;

            Run.instance.SetRunStopwatch(seconds);

            // Log new run time
            Log.Debug($"[DEBUG MENU] | Changed run time to: {minutes} minutes.");
        }

        private void OnGoToStage()
        {
            // Teleport to the selected stage using the same transition as Skip Stage
            if (!NetworkServer.active || !Run.instance || !Stage.instance || stageDropdown.options.Count == 0) return;
            if (!stageLookup.TryGetValue(stageDropdown.options[stageDropdown.value].text, out SceneDef scene)) return;
            if (scene.requiredExpansion && !Run.instance.IsExpansionEnabled(scene.requiredExpansion)) return;

            Stage.instance.BeginAdvanceStage(scene);

            // Log selected stage
            Log.Debug($"[DEBUG MENU] | Going to stage: {scene.cachedName}.");
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
