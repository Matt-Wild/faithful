using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;

namespace Faithful
{
    internal delegate void ModifyAIEnemySearchCallback(BaseAI _ai, BullseyeSearch _search);

    internal delegate bool TrySelectAIEnemyCallback(BaseAI _ai, IEnumerable<HurtBox> _candidates, out HurtBox _target);

    internal static partial class Behaviour
    {
        // Enemy acquisition: modify the search before collecting candidates, then optionally select a target
        private static readonly List<ModifyAIEnemySearchCallback> modifyAIEnemySearchCallbacks = new();
        private static readonly List<TrySelectAIEnemyCallback> trySelectAIEnemyCallbacks = new();

        private static readonly HookRegistration findAIEnemyHook = new("BaseAI.FindEnemyHurtBox",
            () => IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += HookFindAIEnemy,
            () => IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox -= HookFindAIEnemy);

        public static void AddModifyAIEnemySearchCallback(ModifyAIEnemySearchCallback _callback)
        {
            modifyAIEnemySearchCallbacks.Add(_callback);
            DebugLog("Added Modify AI Enemy Search behaviour");
        }

        public static void AddTrySelectAIEnemyCallback(TrySelectAIEnemyCallback _callback)
        {
            trySelectAIEnemyCallbacks.Add(_callback);
            DebugLog("Added Try Select AI Enemy behaviour");
        }

        private static void HookFindAIEnemy(ILContext _il)
        {
            ILCursor cursor = new(_il);

            // Match both injection points before changing the method
            if (!cursor.TryGotoNext(MoveType.Before, instruction => instruction.MatchCallOrCallvirt<BullseyeSearch>("RefreshCandidates")))
            {
                throw new System.InvalidOperationException("Could not find the enemy search refresh in FindEnemyHurtBox.");
            }
            Instruction refresh = cursor.Next;

            if (!cursor.TryGotoNext(MoveType.After, instruction =>
                instruction.MatchCallOrCallvirt<BullseyeSearch>("GetResults") &&
                instruction.Operand is MethodReference method && method.Parameters.Count == 0))
            {
                throw new System.InvalidOperationException("Could not find the enemy search results in FindEnemyHurtBox.");
            }
            Instruction vanillaSelectionStart = cursor.Next;

            VariableDefinition selectedTarget = new(_il.Method.Module.ImportReference(typeof(HurtBox)));
            _il.Body.Variables.Add(selectedTarget);

            // Keep vanilla's search reference for RefreshCandidates after callbacks have modified it
            cursor.Goto(refresh, MoveType.Before);
            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ModifyAIEnemySearch);

            // Keep the original results for vanilla if no callback handles target selection
            cursor.Goto(vanillaSelectionStart, MoveType.Before);
            ILLabel vanillaSelection = cursor.DefineLabel();
            cursor.Emit(OpCodes.Dup);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloca, selectedTarget);
            cursor.EmitDelegate<SelectAIEnemy>(TrySelectAIEnemy);
            cursor.Emit(OpCodes.Brfalse, vanillaSelection);
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldloc, selectedTarget);
            cursor.Emit(OpCodes.Ret);
            cursor.MarkLabel(vanillaSelection);
        }

        private static void ModifyAIEnemySearch(BullseyeSearch _search, BaseAI _ai)
        {
            if (!CanRunHook(findAIEnemyHook) || modifyAIEnemySearchCallbacks.Count == 0) return;

            findAIEnemyHook.StartProfiling();
            try
            {
                foreach (ModifyAIEnemySearchCallback callback in modifyAIEnemySearchCallbacks)
                {
                    callback(_ai, _search);
                }
            }
            finally
            {
                findAIEnemyHook.StopProfiling();
            }
        }

        private delegate bool SelectAIEnemy(IEnumerable<HurtBox> _candidates, BaseAI _ai, out HurtBox _target);

        private static bool TrySelectAIEnemy(IEnumerable<HurtBox> _candidates, BaseAI _ai, out HurtBox _target)
        {
            _target = null;
            if (!CanRunHook(findAIEnemyHook) || trySelectAIEnemyCallbacks.Count == 0) return false;

            findAIEnemyHook.StartProfiling();
            try
            {
                // The first handled result wins, including a deliberately empty target
                foreach (TrySelectAIEnemyCallback callback in trySelectAIEnemyCallbacks)
                {
                    if (callback(_ai, _candidates, out HurtBox target))
                    {
                        _target = target;
                        return true;
                    }
                }
            }
            finally
            {
                findAIEnemyHook.StopProfiling();
            }

            return false;
        }
    }
}
