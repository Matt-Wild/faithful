using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal delegate void SceneDirectorCallback(SceneDirector _director);

    internal delegate void SceneExitControllerCallback(SceneExitController _exitController);

    internal delegate void StageStartCallback(Stage self);

    internal delegate void InfiniteTowerRuleChoicesCallback(InfiniteTowerRun self, RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed);

    internal delegate void SpawnCardAwakeCallback(CharacterSpawnCard self);

    internal delegate void SpawnCardSpawnCallback(CharacterSpawnCard self, Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnCard.SpawnResult result);

    internal static partial class Behaviour
    {
        // Scene population event
        private static readonly List<SceneDirectorCallback> onPrePopulateSceneCallbacks = new List<SceneDirectorCallback>();

        private static readonly HookRegistration prePopulateSceneHook = new("SceneDirector.onPrePopulateSceneServer",
            () => SceneDirector.onPrePopulateSceneServer += HookPrePopulateScene,
            () => SceneDirector.onPrePopulateSceneServer -= HookPrePopulateScene);

        public static void AddOnPrePopulateSceneCallback(SceneDirectorCallback _callback)
        {
            onPrePopulateSceneCallbacks.Add(_callback);

            DebugLog("Added On Pre-Populate Scene behaviour");
        }

        private static void HookPrePopulateScene(SceneDirector _director)
        {
            if (!CanRunHook(prePopulateSceneHook)) return;

            prePopulateSceneHook.StartProfiling();
            try
            {
                // Cycle through callbacks
                foreach (SceneDirectorCallback callback in onPrePopulateSceneCallbacks)
                {
                    // Call
                    callback(_director);
                }
            }
            finally
            {
                prePopulateSceneHook.StopProfiling();
            }
        }

        // Scene exit: callbacks run before the original method
        private static readonly List<SceneExitControllerCallback> onPreSceneExitCallbacks = new List<SceneExitControllerCallback>();

        private static readonly HookRegistration sceneExitControllerSetStateHook = new("SceneExitController.SetState",
            () => On.RoR2.SceneExitController.SetState += HookSceneExitControllerSetState,
            () => On.RoR2.SceneExitController.SetState -= HookSceneExitControllerSetState);

        public static void AddOnPreSceneExitCallback(SceneExitControllerCallback _callback)
        {
            onPreSceneExitCallbacks.Add(_callback);

            DebugLog("Added On Pre-Scene Exit behaviour");
        }

        private static void HookSceneExitControllerSetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            if (!CanRunHook(sceneExitControllerSetStateHook))
            {
                orig(self, newState);
                return;
            }

            sceneExitControllerSetStateHook.StartProfiling();
            try
            {
                // Check if state is finished (ready to change scenes)
                if (newState == SceneExitController.ExitState.TeleportOut)
                {
                    // Cycle through on scene exit callbacks
                    foreach (SceneExitControllerCallback callback in onPreSceneExitCallbacks)
                    {
                        // Call
                        callback(self);
                    }
                }
            }
            finally
            {
                sceneExitControllerSetStateHook.StopProfiling();
            }

            orig(self, newState); // Run normal processes
        }

        // Stage start: callbacks run before the original method returns its enumerator
        private static readonly List<StageStartCallback> stageStartCallbacks = new List<StageStartCallback>();

        private static readonly HookRegistration stageStartHook = new("RoR2.Stage.Start",
            () => On.RoR2.Stage.Start += HookStageStart,
            () => On.RoR2.Stage.Start -= HookStageStart);

        public static void AddStageStartCallback(StageStartCallback _callback)
        {
            stageStartCallbacks.Add(_callback);
            stageStartHook.TryEnable();

            DebugLog("Added StageStart behaviour");
        }

        private static System.Collections.IEnumerator HookStageStart(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            if (stageStartHook.IsActive)
            {
                stageStartHook.StartProfiling();
                try
                {
                    foreach (StageStartCallback callback in stageStartCallbacks)
                    {
                        callback(self);
                    }
                }
                finally
                {
                    stageStartHook.StopProfiling();
                }
            }
            return orig(self); // Run normal processes
        }

        // Simulacrum rules: callbacks run before the original method
        private static readonly List<InfiniteTowerRuleChoicesCallback> infiniteTowerRuleChoicesCallbacks = new List<InfiniteTowerRuleChoicesCallback>();

        private static readonly HookRegistration infiniteTowerRuleChoicesHook = new("RoR2.InfiniteTowerRun.OverrideRuleChoices",
            () => On.RoR2.InfiniteTowerRun.OverrideRuleChoices += HookInfiniteTowerRuleChoices,
            () => On.RoR2.InfiniteTowerRun.OverrideRuleChoices -= HookInfiniteTowerRuleChoices);

        public static void AddInfiniteTowerRuleChoicesCallback(InfiniteTowerRuleChoicesCallback _callback)
        {
            infiniteTowerRuleChoicesCallbacks.Add(_callback);
            infiniteTowerRuleChoicesHook.TryEnable();

            DebugLog("Added InfiniteTowerRuleChoices behaviour");
        }

        private static void HookInfiniteTowerRuleChoices(On.RoR2.InfiniteTowerRun.orig_OverrideRuleChoices orig, InfiniteTowerRun self, RuleChoiceMask mustInclude, RuleChoiceMask mustExclude, ulong runSeed)
        {
            if (infiniteTowerRuleChoicesHook.IsActive)
            {
                infiniteTowerRuleChoicesHook.StartProfiling();
                try
                {
                    foreach (InfiniteTowerRuleChoicesCallback callback in infiniteTowerRuleChoicesCallbacks)
                    {
                        callback(self, mustInclude, mustExclude, runSeed);
                    }
                }
                finally
                {
                    infiniteTowerRuleChoicesHook.StopProfiling();
                }
            }
            orig(self, mustInclude, mustExclude, runSeed); // Apply normal rules after callbacks update the blacklist
        }

        // Spawn card awake: callbacks run after the original method
        private static readonly List<SpawnCardAwakeCallback> spawnCardAwakeCallbacks = new List<SpawnCardAwakeCallback>();

        private static readonly HookRegistration spawnCardAwakeHook = new("RoR2.CharacterSpawnCard.Awake",
            () => On.RoR2.CharacterSpawnCard.Awake += HookSpawnCardAwake,
            () => On.RoR2.CharacterSpawnCard.Awake -= HookSpawnCardAwake);

        public static void AddSpawnCardAwakeCallback(SpawnCardAwakeCallback _callback)
        {
            spawnCardAwakeCallbacks.Add(_callback);
            spawnCardAwakeHook.TryEnable();

            DebugLog("Added SpawnCardAwake behaviour");
        }

        private static void HookSpawnCardAwake(On.RoR2.CharacterSpawnCard.orig_Awake orig, CharacterSpawnCard self)
        {
            orig(self); // Run normal processes
            if (!spawnCardAwakeHook.IsActive) return;

            spawnCardAwakeHook.StartProfiling();
            try
            {
                foreach (SpawnCardAwakeCallback callback in spawnCardAwakeCallbacks)
                {
                    callback(self);
                }
            }
            finally
            {
                spawnCardAwakeHook.StopProfiling();
            }
        }

        // Spawn card spawn: callbacks can modify the result after the original method
        private static readonly List<SpawnCardSpawnCallback> spawnCardSpawnCallbacks = new List<SpawnCardSpawnCallback>();

        private static readonly HookRegistration spawnCardSpawnHook = new("RoR2.CharacterSpawnCard.Spawn",
            () => On.RoR2.CharacterSpawnCard.Spawn += HookSpawnCardSpawn,
            () => On.RoR2.CharacterSpawnCard.Spawn -= HookSpawnCardSpawn);

        public static void AddSpawnCardSpawnCallback(SpawnCardSpawnCallback _callback)
        {
            spawnCardSpawnCallbacks.Add(_callback);
            spawnCardSpawnHook.TryEnable();

            DebugLog("Added SpawnCardSpawn behaviour");
        }

        private static void HookSpawnCardSpawn(On.RoR2.CharacterSpawnCard.orig_Spawn orig, CharacterSpawnCard self, Vector3 position, Quaternion rotation, DirectorSpawnRequest directorSpawnRequest, ref SpawnCard.SpawnResult result)
        {
            orig(self, position, rotation, directorSpawnRequest, ref result); // Run normal processes
            if (!spawnCardSpawnHook.IsActive) return;

            spawnCardSpawnHook.StartProfiling();
            try
            {
                foreach (SpawnCardSpawnCallback callback in spawnCardSpawnCallbacks)
                {
                    callback(self, position, rotation, directorSpawnRequest, ref result);
                }
            }
            finally
            {
                spawnCardSpawnHook.StopProfiling();
            }
        }
    }
}
