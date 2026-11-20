using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
        Dictionary<string, string> stageLookup = new();

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
            // Discover registered scenes and remember expansion-locked destinations
            List<string> stageNames = new();
            HashSet<string> knownScenes = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> blockedScenes = new(StringComparer.OrdinalIgnoreCase);
            stageLookup.Clear();
            foreach (SceneDef scene in SceneCatalog.allSceneDefs)
            {
                // Remember filtered scenes so the built-in scene pass cannot add them back
                knownScenes.Add(scene.cachedName);
                if (scene.requiredExpansion && (!Run.instance || !Run.instance.IsExpansionEnabled(scene.requiredExpansion)))
                {
                    blockedScenes.Add(scene.baseSceneName);
                    continue;
                }

                // Keep existing display names and distinguish scenes which share a name
                string name = string.IsNullOrEmpty(scene.nameToken) ? scene.cachedName : Language.GetString(scene.nameToken);
                if (stageLookup.ContainsKey(name)) name = $"{name} [{scene.cachedName}]";
                stageNames.Add(name);
                stageLookup[name] = scene.cachedName;
            }

            // Include built-in scenes which do not have a registered scene definition
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                if (string.IsNullOrEmpty(sceneName) || !knownScenes.Add(sceneName) || blockedScenes.Contains(sceneName)) continue;

                string name = stageLookup.ContainsKey(sceneName) ? $"{sceneName} [{sceneName}]" : sceneName;
                stageNames.Add(name);
                stageLookup[name] = sceneName;
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
            // Check for host authority and a selected destination
            if (!NetworkServer.active || !Run.instance || !Stage.instance || stageDropdown.options.Count == 0) return;
            if (!stageLookup.TryGetValue(stageDropdown.options[stageDropdown.value].text, out string sceneName)) return;

            SceneDef scene = SceneCatalog.FindSceneDef(sceneName);
            if (scene && scene.requiredExpansion && !Run.instance.IsExpansionEnabled(scene.requiredExpansion)) return;

            // Keep the normal stage transition where a scene definition is available
            if (scene) Stage.instance.BeginAdvanceStage(scene);
            else NetworkManager.singleton.ServerChangeScene(sceneName);

            // Log selected stage
            Log.Debug($"[DEBUG MENU] | Going to stage: {sceneName}.");
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
