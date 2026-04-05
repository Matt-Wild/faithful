using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Faithful
{
    internal static class ILHelper
    {
        internal static bool MatchFieldLoad(Instruction _instruction, string _declaringTypeFullName, string _fieldName)
        {
            if (_instruction == null)
            {
                return false;
            }

            if (_instruction.OpCode == OpCodes.Ldfld && _instruction.Operand is FieldReference fieldReference)
            {
                return fieldReference.Name == _fieldName
                    && fieldReference.DeclaringType != null
                    && fieldReference.DeclaringType.FullName == _declaringTypeFullName;
            }

            return false;
        }

        internal static bool MatchMethodCall(Instruction _instruction, string _declaringTypeFullName, string _methodName)
        {
            if (_instruction == null)
            {
                return false;
            }

            if ((_instruction.OpCode == OpCodes.Call || _instruction.OpCode == OpCodes.Callvirt) &&
                _instruction.Operand is MethodReference methodReference)
            {
                return methodReference.Name == _methodName
                    && methodReference.DeclaringType != null
                    && methodReference.DeclaringType.FullName == _declaringTypeFullName;
            }

            return false;
        }

        internal static bool MatchFieldLoadOrPropertyGetter(Instruction _instruction, string _declaringTypeFullName, string _memberName)
        {
            return MatchFieldLoad(_instruction, _declaringTypeFullName, _memberName)
                || MatchMethodCall(_instruction, _declaringTypeFullName, $"get_{_memberName}");
        }

        internal static bool TryGotoNext(ILCursor _cursor, string _hookName, params Func<Instruction, bool>[] _predicates)
        {
            if (_cursor.TryGotoNext(MoveType.After, _predicates))
            {
                return true;
            }

            Log.Error($"[{_hookName}] Failed to find IL pattern.");
            return false;
        }

        internal static void EmitFloatModifier<T1, T2>(ILCursor _cursor, OpCode _arg1Load, OpCode _arg2Load, Func<float, T1, T2, float> _modifier)
        {
            _cursor.Emit(_arg1Load);
            _cursor.Emit(_arg2Load);
            _cursor.EmitDelegate(_modifier);
        }

        internal static void DumpInstructions(ILContext _il, string _hookName)
        {
            Log.Info($"[{_hookName}] IL dump start");

            for (int i = 0; i < _il.Body.Instructions.Count; i++)
            {
                Log.Info($"[{_hookName}] {i}: {_il.Body.Instructions[i]}");
            }

            Log.Info($"[{_hookName}] IL dump end");
        }
    }
}