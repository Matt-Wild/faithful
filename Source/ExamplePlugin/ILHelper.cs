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
            if (_instruction == null) return false;

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
            if (_instruction == null) return false;

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
            if (_cursor.TryGotoNext(MoveType.After, _predicates)) return true;

            Log.Warning($"[{_hookName}] Failed to find IL pattern.");
            return false;
        }

        internal static bool TryGotoAfterMemberThenNextOp(ILCursor _cursor, string _hookName, Func<Instruction, bool> _memberPredicate, OpCode _nextOpCode, int _maxLookAhead = 8)
        {
            int originalIndex = _cursor.Index;

            if (!_cursor.TryGotoNext(MoveType.After, _memberPredicate))
            {
                Log.Warning($"[{_hookName}] Failed to find IL anchor.");
                return false;
            }

            for (int i = 0; i < _maxLookAhead && _cursor.Next != null; i++)
            {
                if (_cursor.Next.OpCode == _nextOpCode)
                {
                    _cursor.Goto(_cursor.Next, MoveType.After);
                    return true;
                }

                _cursor.Index++;
            }

            _cursor.Index = originalIndex;
            Log.Warning($"[{_hookName}] Found anchor, but did not find expected '{_nextOpCode}' within {_maxLookAhead} instructions.");
            return false;
        }

        internal static void EmitFloatModifier<T1, T2>(ILCursor _cursor, OpCode _arg1Load, OpCode _arg2Load, Func<float, T1, T2, float> _modifier)
        {
            _cursor.Emit(_arg1Load);
            _cursor.Emit(_arg2Load);
            _cursor.EmitDelegate(_modifier);
        }

        internal static void SafeDumpInstructions(ILContext _il, string _hookName, int _maxInstructions = 120)
        {
            try
            {
                Log.Info($"[{_hookName}] IL dump start");

                int count = Math.Min(_il.Body.Instructions.Count, _maxInstructions);

                for (int i = 0; i < count; i++)
                {
                    Instruction instruction = _il.Body.Instructions[i];

                    string operandText;
                    try
                    {
                        operandText = FormatOperand(instruction.Operand);
                    }
                    catch (Exception e)
                    {
                        operandText = $"<operand unreadable: {e.GetType().Name}>";
                    }

                    Log.Info($"[{_hookName}] {i}: IL_{instruction.Offset:X4}: {instruction.OpCode} {operandText}");
                }

                if (_il.Body.Instructions.Count > count)
                {
                    Log.Info($"[{_hookName}] IL dump truncated at {_maxInstructions} instructions.");
                }

                Log.Info($"[{_hookName}] IL dump end");
            }
            catch (Exception e)
            {
                Log.Error($"[{_hookName}] SafeDumpInstructions failed: {e}");
            }
        }

        private static string FormatOperand(object _operand)
        {
            if (_operand == null) return string.Empty;

            try
            {
                return _operand.ToString();
            }
            catch
            {
                return $"<{_operand.GetType().FullName}>";
            }
        }
    }
}