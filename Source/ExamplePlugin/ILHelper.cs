using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Faithful
{
    internal static class ILHelper
    {
        #region Instruction Matchers

        /// <summary>
        /// Returns true if the instruction loads a field with the given declaring type and field name.
        /// Useful when matching normal instance fields where the declaring type is stable.
        /// </summary>
        /// <param name="_instruction">Instruction to check.</param>
        /// <param name="_declaringTypeFullName">Full Cecil type name, e.g. "RoR2.DamageInfo".</param>
        /// <param name="_fieldName">Name of the field to match.</param>
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

        /// <summary>
        /// Returns true if the instruction loads a field with the given field name, ignoring declaring type.
        /// Useful for compiler-generated display-class fields where the declaring type is unstable or ugly.
        /// </summary>
        /// <param name="_instruction">Instruction to check.</param>
        /// <param name="_fieldName">Name of the field to match.</param>
        internal static bool MatchFieldLoadByName(Instruction _instruction, string _fieldName)
        {
            if (_instruction == null) return false;

            if (_instruction.OpCode == OpCodes.Ldfld && _instruction.Operand is FieldReference fieldReference)
            {
                return fieldReference.Name == _fieldName;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the instruction is a call or callvirt to the given method on the given declaring type.
        /// Useful for matching normal method calls and property getters.
        /// </summary>
        /// <param name="_instruction">Instruction to check.</param>
        /// <param name="_declaringTypeFullName">Full Cecil type name, e.g. "RoR2.HealthComponent".</param>
        /// <param name="_methodName">Name of the method to match, e.g. "TakeDamage" or "get_health".</param>
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

        /// <summary>
        /// Returns true if the instruction loads a field or calls a property getter with the given member name.
        /// Useful when game code may use either a field or property depending on version or compilation.
        /// </summary>
        /// <param name="_instruction">Instruction to check.</param>
        /// <param name="_declaringTypeFullName">Full Cecil type name containing the field or property.</param>
        /// <param name="_memberName">Field or property name without "get_".</param>
        internal static bool MatchFieldLoadOrPropertyGetter(Instruction _instruction, string _declaringTypeFullName, string _memberName)
        {
            return MatchFieldLoad(_instruction, _declaringTypeFullName, _memberName)
                || MatchMethodCall(_instruction, _declaringTypeFullName, $"get_{_memberName}");
        }

        /// <summary>
        /// Returns true if the instruction loads a local variable using any ldloc opcode variant.
        /// Useful when matching local-variable patterns without caring which local slot the compiler used.
        /// </summary>
        /// <param name="_instruction">Instruction to check.</param>
        internal static bool MatchLoadLocal(Instruction _instruction)
        {
            if (_instruction == null) return false;

            return _instruction.OpCode == OpCodes.Ldloc
                || _instruction.OpCode == OpCodes.Ldloc_S
                || _instruction.OpCode == OpCodes.Ldloc_0
                || _instruction.OpCode == OpCodes.Ldloc_1
                || _instruction.OpCode == OpCodes.Ldloc_2
                || _instruction.OpCode == OpCodes.Ldloc_3;
        }

        #endregion

        #region Cursor Navigation

        /// <summary>
        /// Moves the cursor after the next matching instruction pattern.
        /// Logs a warning and returns false if the pattern cannot be found.
        /// </summary>
        /// <param name="_cursor">Cursor to move.</param>
        /// <param name="_hookName">Name used in warning logs.</param>
        /// <param name="_predicates">Instruction predicates that make up the pattern.</param>
        internal static bool TryGotoNext(ILCursor _cursor, string _hookName, params Func<Instruction, bool>[] _predicates)
        {
            return TryGotoNext(_cursor, _hookName, MoveType.After, _predicates);
        }

        /// <summary>
        /// Moves the cursor to the next matching instruction pattern using the supplied move type.
        /// Logs a warning and returns false if the pattern cannot be found.
        /// </summary>
        /// <param name="_cursor">Cursor to move.</param>
        /// <param name="_hookName">Name used in warning logs.</param>
        /// <param name="_moveType">Where the cursor should land relative to the matched pattern.</param>
        /// <param name="_predicates">Instruction predicates that make up the pattern.</param>
        internal static bool TryGotoNext(ILCursor _cursor, string _hookName, MoveType _moveType, params Func<Instruction, bool>[] _predicates)
        {
            if (_cursor.TryGotoNext(_moveType, _predicates)) return true;

            Log.Warning($"[{_hookName}] Failed to find IL pattern.");
            return false;
        }

        /// <summary>
        /// Moves the cursor after the next call to a specific method.
        /// Useful for anchoring an injection point after a known method call.
        /// </summary>
        /// <param name="_cursor">Cursor to move.</param>
        /// <param name="_hookName">Name used in warning logs.</param>
        /// <param name="_declaringTypeFullName">Full Cecil type name containing the method.</param>
        /// <param name="_methodName">Name of the method to find.</param>
        internal static bool TryGotoAfterMethodCall(ILCursor _cursor, string _hookName, string _declaringTypeFullName, string _methodName)
        {
            return TryGotoNext(
                _cursor,
                _hookName,
                MoveType.After,
                x => MatchMethodCall(x, _declaringTypeFullName, _methodName));
        }

        /// <summary>
        /// Moves the cursor after a matched member access, then scans forward for the next expected opcode.
        /// Restores the original cursor index and returns false if the expected opcode is not found.
        /// </summary>
        /// <param name="_cursor">Cursor to move.</param>
        /// <param name="_hookName">Name used in warning logs.</param>
        /// <param name="_memberPredicate">Predicate used to find the initial field, property, or method anchor.</param>
        /// <param name="_nextOpCode">Opcode expected shortly after the anchor.</param>
        /// <param name="_maxLookAhead">Maximum number of instructions to scan after the anchor.</param>
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

        #endregion

        #region Emit Helpers

        /// <summary>
        /// Emits two argument loads followed by a delegate that modifies a float already on the stack.
        /// Expected stack before call: float.
        /// Expected stack after call: modified float.
        /// </summary>
        /// <typeparam name="T1">Type of the first extra delegate argument.</typeparam>
        /// <typeparam name="T2">Type of the second extra delegate argument.</typeparam>
        /// <param name="_cursor">Cursor to emit at.</param>
        /// <param name="_arg1Load">Opcode used to load the first extra argument.</param>
        /// <param name="_arg2Load">Opcode used to load the second extra argument.</param>
        /// <param name="_modifier">Delegate that receives the original float and two extra arguments, then returns the modified float.</param>
        internal static void EmitFloatModifier<T1, T2>(ILCursor _cursor, OpCode _arg1Load, OpCode _arg2Load, Func<float, T1, T2, float> _modifier)
        {
            _cursor.Emit(_arg1Load);
            _cursor.Emit(_arg2Load);
            _cursor.EmitDelegate(_modifier);
        }

        #endregion

        #region Debug Helpers

        /// <summary>
        /// Safely logs the beginning of an IL method body without throwing if an operand cannot be formatted.
        /// Useful when a hook fails and you need to inspect nearby IL from player logs.
        /// </summary>
        /// <param name="_il">IL context to dump.</param>
        /// <param name="_hookName">Name used in log lines.</param>
        /// <param name="_maxInstructions">Maximum number of instructions to log from the start of the method.</param>
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

        /// <summary>
        /// Converts an IL operand to log-safe text.
        /// </summary>
        /// <param name="_operand">Operand to format.</param>
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

        #endregion
    }
}