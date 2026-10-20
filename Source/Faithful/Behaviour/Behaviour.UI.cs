using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace Faithful
{
    // Return false to hide an entry; true leaves the normal selection rules in place
    internal delegate bool CanSelectItemEntryCallback(ItemDef _itemDef, Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> _expansionAvailability);

    internal delegate void AttemptPingCallback(PingerController self, ref Ray aimRay, GameObject bodyObject);

    internal delegate void MainMenuEnterCallback(RoR2.UI.MainMenu.BaseMainMenuScreen self, RoR2.UI.MainMenu.MainMenuController mainMenuController);

    internal delegate void LoadoutPanelEnableCallback(RoR2.UI.LoadoutPanelController self);

    internal static partial class Behaviour
    {
        // Menu hooks have their own lifetime, independently of expansion gameplay hooks
        // Menu hooks remain available independently of expansion gameplay hooks
        internal static void EnableMenuHooks()
        {
            mainMenuEnterHook.TryEnable();
            loadoutPanelEnableHook.TryEnable();
        }

        internal static void DisableMenuHooks()
        {
            mainMenuEnterHook.TryDisable();
            loadoutPanelEnableHook.TryDisable();
        }

        // Main menu entry: callbacks run after the original method
        private static readonly List<MainMenuEnterCallback> mainMenuEnterCallbacks = new List<MainMenuEnterCallback>();

        private static readonly HookRegistration mainMenuEnterHook = new("RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter",
            () => On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += HookMainMenuEnter,
            () => On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter -= HookMainMenuEnter);

        public static void AddMainMenuEnterCallback(MainMenuEnterCallback _callback)
        {
            mainMenuEnterCallbacks.Add(_callback);

            DebugLog("Added MainMenuEnter behaviour");
        }

        private static void HookMainMenuEnter(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_OnEnter orig, RoR2.UI.MainMenu.BaseMainMenuScreen self, RoR2.UI.MainMenu.MainMenuController mainMenuController)
        {
            orig(self, mainMenuController); // Run normal processes
            if (!(Faithful.InitializationSucceeded && mainMenuEnterHook.IsActive)) return;

            mainMenuEnterHook.StartProfiling();
            try
            {
                foreach (MainMenuEnterCallback callback in mainMenuEnterCallbacks)
                {
                    callback(self, mainMenuController);
                }
            }
            catch (System.Exception exception)
            {
                mainMenuEnterHook.Fail("execute", exception);
            }
            finally
            {
                mainMenuEnterHook.StopProfiling();
            }
        }

        // Loadout panel enable: callbacks run after the original method
        private static readonly List<LoadoutPanelEnableCallback> loadoutPanelEnableCallbacks = new List<LoadoutPanelEnableCallback>();

        private static readonly HookRegistration loadoutPanelEnableHook = new("RoR2.UI.LoadoutPanelController.OnEnable",
            () => On.RoR2.UI.LoadoutPanelController.OnEnable += HookLoadoutPanelEnable,
            () => On.RoR2.UI.LoadoutPanelController.OnEnable -= HookLoadoutPanelEnable);

        public static void AddLoadoutPanelEnableCallback(LoadoutPanelEnableCallback _callback)
        {
            loadoutPanelEnableCallbacks.Add(_callback);

            DebugLog("Added LoadoutPanelEnable behaviour");
        }

        private static void HookLoadoutPanelEnable(On.RoR2.UI.LoadoutPanelController.orig_OnEnable orig, RoR2.UI.LoadoutPanelController self)
        {
            orig(self); // Run normal processes
            if (!(Faithful.InitializationSucceeded && loadoutPanelEnableHook.IsActive)) return;

            loadoutPanelEnableHook.StartProfiling();
            try
            {
                foreach (LoadoutPanelEnableCallback callback in loadoutPanelEnableCallbacks)
                {
                    callback(self);
                }
            }
            catch (System.Exception exception)
            {
                loadoutPanelEnableHook.Fail("execute", exception);
            }
            finally
            {
                loadoutPanelEnableHook.StopProfiling();
            }
        }

        // Logbook selection: a false callback result hides the entry
        private static readonly List<CanSelectItemEntryCallback> canSelectItemEntryCallbacks = new List<CanSelectItemEntryCallback>();

        private static readonly HookRegistration canSelectItemEntryHook = new("LogBookController.CanSelectItemEntry",
            () => On.RoR2.UI.LogBook.LogBookController.CanSelectItemEntry += HookCanSelectItemEntry,
            () => On.RoR2.UI.LogBook.LogBookController.CanSelectItemEntry -= HookCanSelectItemEntry);

        public static void AddCanSelectItemEntryCallback(CanSelectItemEntryCallback _callback)
        {
            canSelectItemEntryCallbacks.Add(_callback);

            DebugLog("Added LogBookController.CanSelectItemEntry behaviour");
        }

        private static bool HookCanSelectItemEntry(On.RoR2.UI.LogBook.LogBookController.orig_CanSelectItemEntry orig, ItemDef itemDef, Dictionary<RoR2.ExpansionManagement.ExpansionDef, bool> expansionAvailability)
        {
            if (CanRunHook(canSelectItemEntryHook))
            {
                canSelectItemEntryHook.StartProfiling();
                try
                {
                    // Allow callbacks to hide entries without bypassing the normal selection rules
                    foreach (CanSelectItemEntryCallback callback in canSelectItemEntryCallbacks)
                    {
                        if (!callback(itemDef, expansionAvailability)) return false;
                    }
                }
                finally
                {
                    canSelectItemEntryHook.StopProfiling();
                }
            }

            return orig(itemDef, expansionAvailability);
        }

        // Pinging: callbacks modify the ray before the original method, newest registration first
        private static readonly List<AttemptPingCallback> attemptPingCallbacks = new List<AttemptPingCallback>();

        private static AttemptPingCallback[] attemptPingCallbackSnapshot = System.Array.Empty<AttemptPingCallback>();

        private static readonly HookRegistration attemptPingHook = new("RoR2.PingerController.AttemptPing",
            () => On.RoR2.PingerController.AttemptPing += HookAttemptPing,
            () => On.RoR2.PingerController.AttemptPing -= HookAttemptPing);

        public static void AddAttemptPingCallback(AttemptPingCallback _callback)
        {
            if (attemptPingCallbacks.Contains(_callback)) return;
            attemptPingCallbacks.Add(_callback);
            attemptPingCallbackSnapshot = attemptPingCallbacks.ToArray();
            attemptPingHook.TryEnable();

            DebugLog("Added AttemptPing behaviour");
        }

        public static void RemoveAttemptPingCallback(AttemptPingCallback _callback)
        {
            if (!attemptPingCallbacks.Remove(_callback)) return;
            attemptPingCallbackSnapshot = attemptPingCallbacks.ToArray();
            if (attemptPingCallbacks.Count == 0) attemptPingHook.TryDisable();
        }

        private static void HookAttemptPing(On.RoR2.PingerController.orig_AttemptPing orig, PingerController self, Ray aimRay, GameObject bodyObject)
        {
            if (attemptPingHook.IsActive)
            {
                attemptPingHook.StartProfiling();
                try
                {
                    // Keep panel changes during a ping from modifying the active callback collection
                    AttemptPingCallback[] callbacks = attemptPingCallbackSnapshot;

                    // Run the most recently enabled panel first, matching the original hook order
                    for (int i = callbacks.Length - 1; i >= 0; i--)
                    {
                        callbacks[i](self, ref aimRay, bodyObject);
                    }
                }
                finally
                {
                    attemptPingHook.StopProfiling();
                }
            }
            orig(self, aimRay, bodyObject); // Run normal processes
        }
    }
}
