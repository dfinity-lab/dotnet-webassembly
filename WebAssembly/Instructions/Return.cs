using System.Reflection.Emit;
using static System.Diagnostics.Debug;
using System.Linq;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// Return zero or more values from this function.
    /// </summary>
    public class Return : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Return"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Return;

        /// <summary>
        /// Creates a new  <see cref="Return"/> instance.
        /// </summary>
        public Return()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            Assert(context != null);

            var returns = context.Signature.RawReturnTypes;
            var stack = context.Stack;
            Assert(stack != null);

            var returnsLength = returns.Length;
            Assert(returnsLength == 0 || returnsLength == 1); //WebAssembly doesn't currently offer multiple returns, which should be blocked earlier.

            var stackCount = stack.Count;

            var blockEntry = context.Depth.Count == 0 ? new BlockEntry(0, new ValueType[] { }, context) : context.Depth.Last();

            var locals = blockEntry.Locals;

            if (stackCount < returnsLength)
                throw new StackTooSmallException(OpCode.Return, returnsLength, 0);

            System.Diagnostics.Debug.Assert(returnsLength <= 1);

            if (stackCount > returnsLength)
            {
                if (returnsLength == 0)
                {
                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);
                }
                else
                {
                    

                    for (int i = returnsLength - 1; i > 0; i--)
                    {
                        var type = stack.Pop();
                        if (type != returns[i])
                            throw new StackTypeInvalidException(OpCode.End, returns[i], type);

                        context.Emit(OpCodes.Stloc, locals[i].LocalIndex);
                        context.Emit(OpCodes.Ldarg, context.Signature.RawParameterTypes.Length + 1 + (i - 1));
                        context.Emit(OpCodes.Ldloc, locals[i].LocalIndex);
                        switch (context.Signature.RawReturnTypes[i])
                        {
                            case ValueType.Int32:
                                context.Emit(OpCodes.Stind_I4);
                                break;
                            case ValueType.Int64:
                                context.Emit(OpCodes.Stind_I8);
                                break;
                            case ValueType.Float32:
                                context.Emit(OpCodes.Stind_R4);
                                break;
                            case ValueType.Float64:
                                context.Emit(OpCodes.Stind_R8);
                                break;
                        }

                    }

                    context.Emit(OpCodes.Stloc, locals[0].LocalIndex);

                    for (var i = 0; i < stackCount - returnsLength; i++)
                        context.Emit(OpCodes.Pop);

                    context.Emit(OpCodes.Ldloc, locals[0].LocalIndex);
                }
            }
            else if (returnsLength == 1)
            {
                var type = stack.Pop();
                if (type != returns[0])
                    throw new StackTypeInvalidException(OpCode.Return, returns[0], type);
            }
            else if (returnsLength > 1)
            {
                for (int i = returnsLength - 1; i > 0; i--)
                {
                    var type_i = stack.Pop();
                    if (type_i != returns[i])
                        throw new StackTypeInvalidException(OpCode.End, returns[i], type_i);

                    context.Emit(OpCodes.Stloc, locals[i].LocalIndex);
                    context.Emit(OpCodes.Ldarg, context.Signature.RawParameterTypes.Length + 1 + (i - 1));
                    context.Emit(OpCodes.Ldloc, locals[i].LocalIndex);
                    switch (context.Signature.RawReturnTypes[i])
                    {
                        case ValueType.Int32:
                            context.Emit(OpCodes.Stind_I4);
                            break;
                        case ValueType.Int64:
                            context.Emit(OpCodes.Stind_I8);
                            break;
                        case ValueType.Float32:
                            context.Emit(OpCodes.Stind_R4);
                            break;
                        case ValueType.Float64:
                            context.Emit(OpCodes.Stind_R8);
                            break;
                    }

                }
                var type = stack.Pop();
                if (type != returns[0])
                    throw new StackTypeInvalidException(OpCode.Return, returns[0], type);

            }

            context.Emit(OpCodes.Ret);
        }
    }
}