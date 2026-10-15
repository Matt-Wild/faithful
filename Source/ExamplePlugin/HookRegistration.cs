using System;
using System.Collections.Generic;

namespace Faithful
{
    internal sealed class HookRegistration
    {
        private static readonly List<HookRegistration> registrations = new();

        private readonly Action subscribe;
        private readonly Action unsubscribe;
        private bool mayBeInstalled;
        private bool removalFailed;

        internal string Name { get; }
        internal bool IsActive { get; private set; }
        internal bool HasFailed { get; private set; }

        internal HookRegistration(string name, Action subscribe, Action unsubscribe)
        {
            Name = name;
            this.subscribe = subscribe;
            this.unsubscribe = unsubscribe;
            registrations.Add(this);
        }

        internal void TryEnable()
        {
            // A failed hook stays unavailable until restart
            if (IsActive || HasFailed || mayBeInstalled) return;

            mayBeInstalled = true;
            try
            {
                subscribe();
                IsActive = true;
            }
            catch (Exception exception)
            {
                Fail("install", exception);
            }
        }

        internal void TryDisable()
        {
            // Callbacks must check IsActive in case detaching the hook also fails
            IsActive = false;
            if (!mayBeInstalled || removalFailed) return;

            try
            {
                unsubscribe();
                mayBeInstalled = false;
            }
            catch (Exception exception)
            {
                HasFailed = true;
                removalFailed = true;
                Log.Error($"Failed to remove Faithful hook '{Name}'.\n{exception}");
            }
        }

        internal void Fail(string operation, Exception exception)
        {
            if (!HasFailed)
            {
                HasFailed = true;
                Log.Error($"Failed to {operation} Faithful hook '{Name}'. Features using this hook may not work.\n{exception}");
            }

            TryDisable();
        }

        internal static string[] GetFailedHookNames()
        {
            List<string> names = new();
            foreach (HookRegistration registration in registrations)
            {
                if (registration.HasFailed) names.Add(registration.Name);
            }

            return names.ToArray();
        }
    }
}
