namespace Faithful
{
    internal static partial class Behaviour
    {
        // Gameplay hooks follow expansion enable/disable and successful initialization
        // Menu hooks are managed separately by Utils; registration-driven hooks enable on first use
        private static bool enabled = false;

        // Preserve installation order after every partial file's hook fields have been initialized
        private static readonly HookRegistration[] gameplayHooks;

        static Behaviour()
        {
            gameplayHooks =
            [
                holdoutZoneControllerUpdateHook,
                holdoutZoneControllerStartHook,
                characterBodyAwakeHook,
                characterBodyStartHook,
                addBuffIndexHook,
                addTimedBuffDefHook,
                addTimedBuffDefMaxStacksHook,
                extendTimedBuffsHook,
                setTimedBuffDurationHook,
                serverGiveItemPermanentHook,
                serverGiveItemTemporaryHook,
                serverGiveItemRentedHook,
                serverRemoveItemHook,
                bodyInventoryChangedHook,
                inventoryDestroyedHook,
                canSelectItemEntryHook,
                huntressSeekingArrowEnterHook,
                droneHeadbuttEnterHook,
                pushPickupNotificationHook,
                getUserNameHook,
                mapTeleportBodyHook,
                inventoryChangedHook,
                itemAddedHook,
                inflictDamageOverTimeHook,
                inflictDamageOverTimeRefHook,
                healthComponentAwakeHook,
                healHook,
                takeDamageProcessHook,
                recalculateStatsHook,
                characterLevelHook,
                buffDisplayValueHook,
                updateOverlaysHook,
                buffFirstStackGainedHook,
                buffFinalStackLostHook,
                updateVisualEffectsHook,
                characterBodyFixedUpdateHook,
                characterBodyRollCritHook,
                interactionBeginHook,
                purchaseInteractionBeginHook,
                purchaseCanBeAffordedHook,
                processJumpHook,
                genericCharacterFixedUpdateHook,
                artificerProcessJumpHook,
                artificerJetpackFixedUpdateHook,
                immuneToFallDamageHook,
                sceneExitControllerSetStateHook,
                onHitEnemyHook,
                statsModHook,
                onDamageDealtHook,
                onCharacterDeathHook,
                prePopulateSceneHook,
            ];
        }

        public static void Init()
        {
            // Create prefabs
            CreatePrefabs();

            // Enable behaviour
            Enable();

            DebugLog("Behaviour initialised");
        }

        private static bool CanRunHook(HookRegistration hook) =>
            enabled && Faithful.InitializationSucceeded && hook.IsActive;

        public static void Enable()
        {
            if (enabled) return;
            enabled = true;

            foreach (HookRegistration hook in gameplayHooks)
            {
                hook.TryEnable();
            }
        }

        public static void Disable()
        {
            enabled = false;

            foreach (HookRegistration hook in gameplayHooks)
            {
                hook.TryDisable();
            }
        }

        public static void DebugLog(string _message)
        {
            // Only log behaviour on verbose console
            if (Utils.verboseConsole)
            {
                // Log message
                Log.Debug($"[BEHAVIOUR] - {_message}");
            }
        }
    }
}
