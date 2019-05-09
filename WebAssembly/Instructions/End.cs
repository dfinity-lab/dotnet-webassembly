using System.Reflection.Emit;
using static System.Diagnostics.Debug;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// An instruction that marks the end of a block, loop, if, or function.
    /// </summary>
    public class End : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.End"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.End;

        /// <summary>
        /// Creates a new <see cref="End"/> instance.
        /// </summary>
        public End()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
            Assert(context != null);
            Assert(context.Depth != null);

            var stack = context.Stack;
            Assert(stack != null);

            var blockEntry = context.Depth.Count == 0 ? new BlockEntry(0, new ValueType[] { }, context) : context.Depth.Pop();
            // var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Pop();


            if (context.Depth.Count == 0)
            {
                if (context.Previous == OpCode.Return)
                    return; //WebAssembly requires functions to end on "end", but an immediately previous return is allowed.

                var returns = context.Signature.RawReturnTypes;
                var returnsLength = returns.Length;

                //System.Diagnostics.Debug.Assert(returnsLength <= 1);
#if !ORIG
                // if (returnsLength != stack.Count)
                //     throw new StackSizeIncorrectException(OpCode.End, returnsLength, stack.Count);

                //Assert(returnsLength == 0 || returnsLength == 1); //WebAssembly doesn't currently offer multiple returns, which should be blocked earlier.


                if (returnsLength == 1)
                {
                    var type = stack.Pop();
                    if (type != returns[0])
                        throw new StackTypeInvalidException(OpCode.End, returns[0], type);
                }

                if (returnsLength > 1)
                {
                    var locals = blockEntry.Locals;
                    for (int i = returnsLength - 1; i >= 0; i--)
                    {
                        var type = stack.Pop();
                        if (type != returns[i])
                            throw new StackTypeInvalidException(OpCode.End, returns[i], type);
                        if (i > 0) // set byref-return parameters
                        {
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
                    }

                }
#endif
                context.Emit(OpCodes.Ret);
            }
            else
            {
                var expectedTypes = blockEntry.Types;
                var stackItems = stack.GetEnumerator();
                /*
                for (int i = expectedTypes.Length - 1; i >= 0; i--) {
                    if (!stackItems.MoveNext())
                        throw new StackSizeIncorrectException(OpCode.End, expectedTypes.Length, stack.Count);
                    if (expectedTypes[i] != stackItems.Current)
                        throw new StackTypeInvalidException(OpCode.End, expectedTypes[i], stackItems.Current);
                   
                };
                */

              
                var depth = checked((uint)context.Depth.Count);
                var label = context.Labels[depth];

                if (!context.LoopLabels.Contains(label))
                { //Loop labels are marked where defined.
                    context.MarkLabel(label);
                }
                else
                    context.LoopLabels.Remove(label);


                while (stack.Count > blockEntry.StackSize)
                {
                    stack.Pop();
                }

                for (int i = 0; i < expectedTypes.Length; i++)
                {
                    context.Stack.Push(expectedTypes[i]);
                }

                context.Labels.Remove(depth);

  
            }
        }
    }
}