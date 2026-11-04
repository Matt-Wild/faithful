using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine;

namespace Faithful
{
    internal delegate void OnInteractionBeginProcCallback(Interactor _interactor, IInteractable _interactable, GameObject _interactableObject);

    internal delegate void OnPurchaseInteractionBeginCallback(PurchaseInteraction _shop, CharacterMaster _activator);

    internal delegate bool OnPurchaseCanBeAffordedCallback(PurchaseInteraction _shop, CharacterMaster _activator);

    internal delegate void ShrineChancePurchaseCallback(ShrineChanceBehavior _shrine, Interactor _activator, ShrineChancePurchaseResult _result);

    internal delegate void InteractorCallback(Interactor _interactor);

    internal delegate void FaithfulInteractorCallback(FaithfulInteractableBehaviour _behaviour, Interactor _interactor);

    internal sealed class ShrineChancePurchaseResult
    {
        public UniquePickup PrimaryPickup { get; set; }
        public System.Action<ShrineChanceBehavior> AfterPrimaryDrop { get; set; }
        public string ChatTokenOverride { get; set; }
        public bool? UseJackpotEffectOverride { get; set; }

        public ShrineChancePurchaseResult(UniquePickup _pickup)
        {
            PrimaryPickup = _pickup;
        }
    }

    internal static partial class Behaviour
    {
        private delegate ShrineChancePurchaseResult ApplyShrineChancePurchaseCallbacks(ref UniquePickup _pickup, ShrineChanceBehavior _shrine, Interactor _activator);

        private static readonly List<ShrineChancePurchaseCallback> shrineChancePurchaseCallbacks = new List<ShrineChancePurchaseCallback>();

        private static readonly HookRegistration shrineChancePickupHook = new("ShrineChanceBehavior.AddShrineStack (selected pickup)",
            () => IL.RoR2.ShrineChanceBehavior.AddShrineStack += HookShrineChancePickup,
            () => IL.RoR2.ShrineChanceBehavior.AddShrineStack -= HookShrineChancePickup);

        public static void AddShrineChancePurchaseCallback(ShrineChancePurchaseCallback _callback)
        {
            shrineChancePurchaseCallbacks.Add(_callback);

            DebugLog("Added Shrine Chance Purchase behaviour");
        }

        private static void HookShrineChancePickup(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Both reward paths meet at the validity check for the selected pickup
            if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCallOrCallvirt<UniquePickup>("get_isValid")))
            {
                throw new System.InvalidOperationException("Could not find the selected pickup validity check in AddShrineStack.");
            }

            // Find every insertion point before modifying the method
            Instruction pickupCheck = cursor.Next;
            if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCall<PickupDropletController>(nameof(PickupDropletController.CreatePickupDroplet))))
            {
                throw new System.InvalidOperationException("Could not find the primary reward drop in AddShrineStack.");
            }

            Instruction afterPrimaryDrop = cursor.Next;
            if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchStfld<SubjectChatMessage>(nameof(SubjectChatMessage.baseToken))))
            {
                throw new System.InvalidOperationException("Could not find the chat token assignment in AddShrineStack.");
            }

            Instruction chatTokenAssignment = cursor.Next;
            if (!cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchLdfld<ShrineChanceBehavior>("chanceDollWin")))
            {
                throw new System.InvalidOperationException("Could not find the reward effect check in AddShrineStack.");
            }

            Instruction afterEffectCheck = cursor.Next;

            // Store the result for this purchase and keep the pickup address for the validity check
            VariableDefinition purchaseResult = new(_il.Method.Module.ImportReference(typeof(ShrineChancePurchaseResult)));
            _il.Body.Variables.Add(purchaseResult);
            cursor.Goto(pickupCheck);
            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_1);
            cursor.EmitDelegate<ApplyShrineChancePurchaseCallbacks>(ModifyShrineChancePurchase);
            cursor.Emit(OpCodes.Stloc, purchaseResult);

            // Run purchase actions after vanilla spawns the primary reward
            cursor.Goto(afterPrimaryDrop);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc, purchaseResult);
            cursor.EmitDelegate<System.Action<ShrineChanceBehavior, ShrineChancePurchaseResult>>((shrine, result) => result?.AfterPrimaryDrop?.Invoke(shrine));

            // Let vanilla broadcast and localize the final token
            cursor.Goto(chatTokenAssignment);
            cursor.Emit(OpCodes.Ldloc, purchaseResult);
            cursor.EmitDelegate<System.Func<string, ShrineChancePurchaseResult, string>>((token, result) => result?.ChatTokenOverride ?? token);

            // Choose the effect independently of vanilla's Chance Doll state
            cursor.Goto(afterEffectCheck);
            cursor.Emit(OpCodes.Ldloc, purchaseResult);
            cursor.EmitDelegate<System.Func<bool, ShrineChancePurchaseResult, bool>>((useJackpot, result) => result?.UseJackpotEffectOverride ?? useJackpot);
        }

        private static ShrineChancePurchaseResult ModifyShrineChancePurchase(ref UniquePickup _pickup, ShrineChanceBehavior _shrine, Interactor _activator)
        {
            if (!CanRunHook(shrineChancePickupHook)) return null;

            ShrineChancePurchaseResult result = new(_pickup);

            shrineChancePickupHook.StartProfiling();
            try
            {
                foreach (ShrineChancePurchaseCallback callback in shrineChancePurchaseCallbacks)
                {
                    callback(_shrine, _activator, result);
                }

                _pickup = result.PrimaryPickup;
                return result;
            }
            finally
            {
                shrineChancePickupHook.StopProfiling();
            }
        }

        // Interaction procs: callbacks run for eligible interaction events
        private static readonly List<OnInteractionBeginProcCallback> onInteractionBeginProcCallbacks = new List<OnInteractionBeginProcCallback>();

        private static readonly HookRegistration interactionBeginHook = new("GlobalEventManager.OnInteractionsGlobal",
            () => GlobalEventManager.OnInteractionsGlobal += HookInteractionBegin,
            () => GlobalEventManager.OnInteractionsGlobal -= HookInteractionBegin);

        // Add On Interaction Begin Proc callback
        public static void AddOnInteractionBeginProcCallback(OnInteractionBeginProcCallback _callback)
        {
            onInteractionBeginProcCallbacks.Add(_callback);

            DebugLog("Added On Interaction Begin Proc behaviour");
        }

        private static void HookInteractionBegin(Interactor _interactor, IInteractable _interactable, GameObject _interactableObject)
        {
            if (!CanRunHook(interactionBeginHook)) return;

            interactionBeginHook.StartProfiling();
            try
            {
                // Validate input
                if (_interactor == null || _interactable == null || !_interactableObject) return;

                // Check if this interactable is allowed to trigger interaction-based item procs
                if (!CanRunInteractionBeginProc(_interactable, _interactableObject)) return;

                // Cycle through On Interaction Begin Proc callbacks
                foreach (OnInteractionBeginProcCallback callback in onInteractionBeginProcCallbacks)
                {
                    // Call
                    callback(_interactor, _interactable, _interactableObject);
                }
            }
            finally
            {
                interactionBeginHook.StopProfiling();
            }
        }

        private static bool CanRunInteractionBeginProc(IInteractable _interactable, GameObject _interactableObject)
        {
            MonoBehaviour interactableBehaviour = _interactable as MonoBehaviour;
            if (!interactableBehaviour) return false;

            // Match base game Fireworks/Squid Polyp interaction proc gate
            InteractionProcFilter interactionProcFilter = _interactableObject.GetComponent<InteractionProcFilter>();
            if (interactionProcFilter) return interactionProcFilter.shouldAllowOnInteractionBeginProc;

            PurchaseInteraction purchaseInteraction = _interactable as PurchaseInteraction;
            if (purchaseInteraction) return !purchaseInteraction.disableSpawnOnInteraction;

            DelusionChestController delusionChestController = interactableBehaviour.GetComponent<DelusionChestController>();
            if (delusionChestController)
            {
                PickupPickerController pickupPickerController = interactableBehaviour.GetComponent<PickupPickerController>();
                return pickupPickerController == null || !pickupPickerController.enabled;
            }

            if (interactableBehaviour.GetComponent<GenericPickupController>()) return false;
            if (interactableBehaviour.GetComponent<VehicleSeat>()) return false;
            if (interactableBehaviour.GetComponent<NetworkUIPromptController>()) return false;

            PowerPedestal powerPedestal = interactableBehaviour.GetComponent<PowerPedestal>();
            if (powerPedestal) return powerPedestal.CanTriggerFireworks;

            AccessCodesNodeController accessCodesNodeController = interactableBehaviour.GetComponent<AccessCodesNodeController>();
            if (accessCodesNodeController) return accessCodesNodeController.CheckInteractionOrder();

            return true;
        }

        // Purchases: callbacks run before the original method
        private static readonly List<OnPurchaseInteractionBeginCallback> onPurchaseInteractionBeginCallbacks = new List<OnPurchaseInteractionBeginCallback>();

        private static readonly HookRegistration purchaseInteractionBeginHook = new("PurchaseInteraction.OnInteractionBegin",
            () => On.RoR2.PurchaseInteraction.OnInteractionBegin += HookPurchaseInteractionBegin,
            () => On.RoR2.PurchaseInteraction.OnInteractionBegin -= HookPurchaseInteractionBegin);

        // Add On Purchase Interaction Begin callback
        public static void AddOnPurchaseInteractionBeginCallback(OnPurchaseInteractionBeginCallback _callback)
        {
            onPurchaseInteractionBeginCallbacks.Add(_callback);

            DebugLog("Added On Purchase Interaction Begin behaviour");
        }

        private static void HookPurchaseInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!CanRunHook(purchaseInteractionBeginHook))
            {
                orig(self, activator);
                return;
            }

            purchaseInteractionBeginHook.StartProfiling();
            try
            {
                // Cycle through OnPurchaseInteractionBegin callbacks
                foreach (OnPurchaseInteractionBeginCallback callback in onPurchaseInteractionBeginCallbacks)
                {
                    // Check for activator
                    CharacterBody body = activator.gameObject.GetComponent<CharacterBody>();
                    if (body != null)
                    {
                        // Call
                        callback(self, body.master);
                    }
                }
            }
            finally
            {
                purchaseInteractionBeginHook.StopProfiling();
            }

            orig(self, activator); // Run normal processes
        }

        // Affordability: a true callback result bypasses the original check
        private static readonly List<OnPurchaseCanBeAffordedCallback> onPurchaseCanBeAffordedCallbacks = new List<OnPurchaseCanBeAffordedCallback>();

        private static readonly HookRegistration purchaseCanBeAffordedHook = new("PurchaseInteraction.CanBeAffordedByInteractor",
            () => On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor += HookPurchaseCanBeAfforded,
            () => On.RoR2.PurchaseInteraction.CanBeAffordedByInteractor -= HookPurchaseCanBeAfforded);

        // Add On Purchase Can Be Afforded callback
        public static void AddOnPurchaseCanBeAffordedCallback(OnPurchaseCanBeAffordedCallback _callback)
        {
            onPurchaseCanBeAffordedCallbacks.Add(_callback);

            DebugLog("Added On Purchase Can Be Afforded behaviour");
        }

        private static bool HookPurchaseCanBeAfforded(On.RoR2.PurchaseInteraction.orig_CanBeAffordedByInteractor orig, PurchaseInteraction self, Interactor activator)
        {
            if (!CanRunHook(purchaseCanBeAffordedHook)) return orig(self, activator);

            purchaseCanBeAffordedHook.StartProfiling();
            try
            {
                // Cycle through OnPurchaseCanBeAfforded callbacks
                foreach (OnPurchaseCanBeAffordedCallback callback in onPurchaseCanBeAffordedCallbacks)
                {
                    // Check for activator
                    CharacterBody body = activator.gameObject.GetComponent<CharacterBody>();
                    if (body != null)
                    {
                        // Call
                        if (callback(self, body.master))
                        {
                            // Force can be afforded if result is true
                            return true;
                        }
                    }
                }
            }
            finally
            {
                purchaseCanBeAffordedHook.StopProfiling();
            }

            return orig(self, activator); // Run normal processes
        }
    }
}
