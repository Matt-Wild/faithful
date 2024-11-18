using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Faithful
{
    internal class DebugAudioAnalysis : DebugPanel
    {
        // Store reference to toggles
        DebugToggle logAudioToggle;
        DebugToggle displayAudioToggle;

        public override void Awake()
        {
            // Call base class Awake
            base.Awake();

            // Get toggles
            logAudioToggle = transform.Find("LogAudioToggle").gameObject.AddComponent<DebugToggle>();
            displayAudioToggle = transform.Find("DisplayAudioToggle").gameObject.AddComponent<DebugToggle>();

            // Add toggle actions
            logAudioToggle.Init(OnLogEnable, OnLogDisable);
            displayAudioToggle.Init(OnDisplayEnable, OnDisplayDisable);
        }

        private void OnDisable()
        {
            // Disable audio logging
            AkSoundEngineDynamicPatcher.logAudio = false;

            // Disable audio analysis
            AkSoundEngineDynamicPatcher.analyseAudio = false;

            // Disable toggles
            logAudioToggle.SetState(false);
            displayAudioToggle.SetState(false);
        }

        private void OnLogEnable()
        {
            // Enable audio logging
            AkSoundEngineDynamicPatcher.logAudio = true;
        }

        private void OnLogDisable()
        {
            // Disable audio logging
            AkSoundEngineDynamicPatcher.logAudio = false;
        }

        private void OnDisplayEnable()
        {
            // Enable audio analysis
            AkSoundEngineDynamicPatcher.analyseAudio = true;
        }

        private void OnDisplayDisable()
        {
            // Disable audio analysis
            AkSoundEngineDynamicPatcher.analyseAudio = false;
        }
    }

    internal static class AkSoundEngineDynamicPatcher
    {
        // Store reference to audio analysis canvas prefab
        private static GameObject audioAnalysisPrefab;

        // Store if audio is being logged or analysed
        public static bool logAudio = false;
        public static bool analyseAudio = false;

        // Store list of audio analysis owners
        private static List<AudioAnalysisOwner> audioAnalysisOwnerList = new List<AudioAnalysisOwner>();

        public static void Init()
        {
            // Get audio analysis canvas prefab
            audioAnalysisPrefab = Assets.GetObject("DebugAudioInstanceCanvas");
        }

        public static void PatchAll(Harmony _harmony)
        {
            // Get sound engine methods
            MethodInfo[] methods = typeof(AkSoundEngine).GetMethods(BindingFlags.Public | BindingFlags.Static);

            // Cycle through methods
            foreach (MethodInfo method in methods)
            {
                // Check if needs to patch method
                if (method.Name == "PostEvent")
                {
                    // Patch method
                    _harmony.Patch(method, prefix: new HarmonyMethod(typeof(AkSoundEngineDynamicPatcher).GetMethod(nameof(PostEventPrefix))));
                }
            }
        }

        public static void PostEventPrefix(MethodBase __originalMethod, object[] __args)
        {
            // Check for string and game object
            if (__args.Length == 2 && __args[0] is string && __args[1] is GameObject)
            {
                // Analyse audio
                AnalyseAudio(__args[0] as string, __args[1] as GameObject);
                return;
            }

            // Check for unint and game object
            if (__args.Length == 2 && __args[0] is uint && __args[1] is GameObject)
            {
                // Analyse audio
                AnalyseAudio(Convert.ToUInt32(__args[0]), __args[1] as GameObject);
                return;
            }

            // Check for unint and game object
            if (__args.Length == 2 && __args[0] is string && __args[1] is ulong)
            {
                // Analyse audio
                AnalyseAudio(__args[0] as string, Convert.ToUInt64(__args[1]));
                return;
            }

            // Otherwise generate string for unmanaged post event call
            string message = "";

            // Cycle through arguments
            foreach (ParameterInfo arg in __originalMethod.GetParameters())
            {
                // Check if first entry
                if (message == "")
                {
                    // Begin message
                    message = $"Unmanaged PostEvent called with args: {arg.ParameterType.Name} {arg.Name}";
                }
                else
                {
                    // Add to message
                    message += $", {arg.ParameterType.Name} {arg.Name}";
                }
            }

            // Log message
            Log.Info(message + ".");
        }

        private static void AnalyseAudio(string _eventName, GameObject _source)
        {
            // Check if should log audio event
            if (logAudio)
            {
                // Log audio
                Log.Info($"Sound event '{_eventName}' playing on game object '{_source.name}'.");
            }
            
            // Check if should display audio event
            if (analyseAudio)
            {
                // Create audio analysis canvas to show where audio is coming from
                CreateAudioAnalysisCanvas($"Event Name:\n{_eventName}", _source);
            }
        }

        private static void AnalyseAudio(uint _eventID, GameObject _source)
        {
            // Check if should log audio event
            if (logAudio)
            {
                // Log audio
                Log.Info($"Sound event with ID {_eventID} playing on game object '{_source.name}'.");
            }

            // Check if should display audio event
            if (analyseAudio)
            {
                // Create audio analysis canvas to show where audio is coming from
                CreateAudioAnalysisCanvas($"Event ID:\n{_eventID}", _source);
            }
        }

        private static void AnalyseAudio(string _eventName, ulong _sourceID)
        {
            // Check if should log audio event
            if (logAudio)
            {
                // Log audio
                Log.Info($"Sound event '{_eventName}' playing on game object with ID {_sourceID}.");
            }
        }

        private static void CreateAudioAnalysisCanvas(string _eventID, GameObject _owner)
        {
            // Skip if game object is null
            if (_owner == null) return;

            // Create new audio analysis canvas
            GameObject canvas = UnityEngine.Object.Instantiate(audioAnalysisPrefab);

            // Add audio analysis canvas behaviour
            AudioAnalysisCanvas behaviour = canvas.AddComponent<AudioAnalysisCanvas>();

            // Initialise audio canvas behaviour
            behaviour.Init(_eventID);

            // Add to audio analysis owner for game object
            GetAudioAnalysisOwner(_owner).AddAudioAnalysis(behaviour);
        }

        public static void ForgetAudioAnalysisOwner(AudioAnalysisOwner _owner)
        {
            // Remove from list
            audioAnalysisOwnerList.Remove(_owner);
        }

        private static AudioAnalysisOwner GetAudioAnalysisOwner(GameObject _owner)
        {
            // Cycle through audio analysis owners
            foreach (AudioAnalysisOwner owner in audioAnalysisOwnerList)
            {
                // Check if correct owner
                if (owner.ownerObject == _owner)
                {
                    // Return owner
                    return owner;
                }
            }

            // Create new audio analysis owner object
            GameObject newOwner = new GameObject("FaithfulAudioAnalysis");

            // Add behaviour
            AudioAnalysisOwner behaviour = newOwner.AddComponent<AudioAnalysisOwner>();

            // Init behaviour
            behaviour.Init(_owner);

            // Return new owner behaviour
            return behaviour;
        }
    }

    internal class AudioAnalysisOwner : MonoBehaviour
    {
        // Store reference to main camera transform
        Transform mainCameraTransform;

        // Store reference to owner
        GameObject owner;

        // Store list of audio analysis
        List<AudioAnalysisCanvas> audioAnalysisList = new List<AudioAnalysisCanvas>();

        public void Init(GameObject _owner)
        {
            // Assign owner
            owner = _owner;

            // Set layer
            gameObject.layer = LayerMask.NameToLayer("UI, WorldSpace");

            // Update position
            UpdatePosition();

            // Update rotation
            UpdateRotation();
        }

        private void OnDestroy()
        {
            // Forget this owner
            AkSoundEngineDynamicPatcher.ForgetAudioAnalysisOwner(this);
        }

        private void LateUpdate()
        {
            // Update position
            UpdatePosition();

            // Update rotation
            UpdateRotation();

            // Update state
            UpdateState();
        }

        private void UpdatePosition()
        {
            // Check if owner exists
            if (owner != null)
            {
                // Get target position
                Vector3 target = owner.transform.position;

                // Get direction to camera
                Vector3 dir = Camera.main.transform.position - target;

                // Get distance to camera
                float distance = dir.magnitude;

                // Subtract short amount from distance and position from target to camera and reposition
                transform.position = target + dir.normalized * (distance - 1.0f);
            }
        }

        private void UpdateRotation()
        {
            // Check for main camera transform
            if (mainCameraTransform == null) mainCameraTransform = Camera.main.transform;

            // Check again
            if (mainCameraTransform == null) return;

            // Face towards main camera
            transform.LookAt(mainCameraTransform);

            // Invert
            transform.Rotate(0, 180, 0);
        }

        private void UpdateState()
        {
            // Check if no audio analysis exists or owning game object is null
            if (owner == null || audioAnalysisList.Count == 0)
            {
                // Destroy self
                Destroy(gameObject);
            }
        }

        public void AddAudioAnalysis(AudioAnalysisCanvas _audioAnalysis)
        {
            // Add to list
            audioAnalysisList.Add(_audioAnalysis);

            // Pass self as owner
            _audioAnalysis.PassOwner(this);
        }

        public void ForgetAudioAnalysis(AudioAnalysisCanvas _audioAnalysis)
        {
            // Remove from list
            audioAnalysisList.Remove(_audioAnalysis);
        }

        public GameObject ownerObject
        {
            get
            {
                // Return game object this behaviour follows
                return owner;
            }
        }
    }

    internal class AudioAnalysisCanvas : MonoBehaviour
    {
        // Store reference to audio analysis owner
        AudioAnalysisOwner audioAnalysisOwner;

        // Store reference to text
        Text audioText;

        public void Init(string _audioID)
        {
            // Get audio text
            audioText = transform.Find("Panel").Find("AudioText").GetComponent<Text>();

            // Set audio text
            audioText.text = _audioID;

            // Destroy the audio analysis canvas after a delay
            Invoke("DestroyObject", 4.0f);
        }

        private void Awake()
        {
            // Set layer
            Utils.SetLayer(gameObject, "UI, WorldSpace");
        }

        private void OnDestroy()
        {
            // Check for owner
            if (audioAnalysisOwner != null)
            {
                // Make owner forget this audio analysis
                audioAnalysisOwner.ForgetAudioAnalysis(this);
            }
        }

        public void PassOwner(AudioAnalysisOwner _audioAnalysisOwner)
        {
            // Assign owner
            audioAnalysisOwner = _audioAnalysisOwner;

            // Set parent, position and rotation
            transform.SetParent(audioAnalysisOwner.transform);
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
        }

        private void DestroyObject()
        {
            // Destroy self
            Destroy(gameObject);
        }
    }
}
