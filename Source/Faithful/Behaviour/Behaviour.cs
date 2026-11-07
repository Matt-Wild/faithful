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
                supportBeaconControllerStartHook,
                holdoutZoneFixedRadiusHook,
                supportBeaconInitialRadiusHook,
                characterBodyAwakeHook,
                characterBodyStartHook,
                bodyDestroyedHook,
                stateMachineUpdateHook,
                stateMachineFixedUpdateHook,
                stateMachineSetStateHook,
                stateMachineDestroyHook,
                findAIEnemyHook,
                aiRetaliationHook,
                enemyDetectionTeamHook,
                lightningOrbTargetHook,
                bullseyeSearchRefreshHook,
                bullseyeSearchResultsHook,
                sphereSearchRefreshHook,
                sphereSearchTeamHook,
                sphereSearchResultsHook,
                sphereSearchArrayHook,
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
                takeDamageFriendlyHitHook,
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
                shrineChancePickupHook,
                purchaseCanBeAffordedHook,
                processJumpHook,
                genericCharacterFixedUpdateHook,
                artificerProcessJumpHook,
                artificerJetpackFixedUpdateHook,
                immuneToFallDamageHook,
                sceneExitControllerSetStateHook,
                onHitEnemyHook,
                processHitEnemyFriendlyHitHook,
                overlapFriendlyHitHook,
                blastFriendlyHitHook,
                damageTrailFriendlyHitHook,
                bulletFriendlyHitHook,
                bulletIgnoreAlliesHook,
                bulletFriendlyFilterHook,
                projectileAllyImpactHook,
                projectileAllyCollisionHook,
                projectileTeamCollisionHook,
                mushroomSporeFilterHook,
                torusFriendlyHitHook,
                sphereProjectileTargetHook,
                wightProjectileTargetHook,
                directionalProjectileTargetHook,
                missileTargetHook,
                statsModHook,
                onDamageDealtHook,
                onCharacterDeathHook,
                prePopulateSceneHook,
                runSetRuleBookHook,
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
