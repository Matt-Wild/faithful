using System.Collections.Generic;
using RoR2;

namespace Faithful
{
    internal delegate void ProfileSavedCallback(UserProfile _profile);

    internal delegate void UserSignInCallback(LocalUser _user);

    internal static partial class Behaviour
    {
        // Profile hooks remain available independently of expansion gameplay hooks
        private static readonly List<ProfileSavedCallback> profileSavedCallbacks = new();

        private static readonly HookRegistration profileSaveHook = new("SaveSystem.Save",
            () => On.RoR2.SaveSystem.Save += HookProfileSave,
            () => On.RoR2.SaveSystem.Save -= HookProfileSave);

        public static void AddProfileSavedCallback(ProfileSavedCallback _callback)
        {
            profileSavedCallbacks.Add(_callback);
            profileSaveHook.TryEnable();

            DebugLog("Added ProfileSaved behaviour");
        }

        private static bool HookProfileSave(On.RoR2.SaveSystem.orig_Save orig, SaveSystem self, UserProfile data, bool blocking)
        {
            bool result = orig(self, data, blocking); // Run normal processes
            if (!result || !(Faithful.InitializationSucceeded && profileSaveHook.IsActive)) return result;

            // A non-blocking save has been started, but may not have reached disk yet
            profileSaveHook.StartProfiling();
            try
            {
                foreach (ProfileSavedCallback callback in profileSavedCallbacks)
                {
                    callback(data);
                }
            }
            catch (System.Exception exception)
            {
                profileSaveHook.Fail("execute", exception);
            }
            finally
            {
                profileSaveHook.StopProfiling();
            }

            return result;
        }

        private static readonly List<UserSignInCallback> userSignInCallbacks = new();

        private static readonly HookRegistration userSignInHook = new("LocalUserManager.onUserSignIn",
            () => LocalUserManager.onUserSignIn += HookUserSignIn,
            () => LocalUserManager.onUserSignIn -= HookUserSignIn);

        public static void AddUserSignInCallback(UserSignInCallback _callback)
        {
            userSignInCallbacks.Add(_callback);
            userSignInHook.TryEnable();

            DebugLog("Added UserSignIn behaviour");
        }

        private static void HookUserSignIn(LocalUser _user)
        {
            if (!(Faithful.InitializationSucceeded && userSignInHook.IsActive)) return;

            userSignInHook.StartProfiling();
            try
            {
                foreach (UserSignInCallback callback in userSignInCallbacks)
                {
                    callback(_user);
                }
            }
            catch (System.Exception exception)
            {
                userSignInHook.Fail("execute", exception);
            }
            finally
            {
                userSignInHook.StopProfiling();
            }
        }
    }
}
