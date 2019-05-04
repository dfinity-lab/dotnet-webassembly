using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace WebAssembly.Instructions
{
    /// <summary>
    /// An instruction which always traps.
    /// </summary>
    /// <remarks>It is intended to be used for example after calls to functions which are known by the producer not to return.</remarks>
    public class Unreachable : SimpleInstruction
    {
        /// <summary>
        /// Always <see cref="OpCode.Unreachable"/>.
        /// </summary>
        public sealed override OpCode OpCode => OpCode.Unreachable;

        /// <summary>
        /// Creates a new  <see cref="Unreachable"/> instance.
        /// </summary>
        public Unreachable()
        {
        }

        internal sealed override void Compile(CompilationContext context)
        {
#if ORIG
#else

            var blockType = context.Depth.Count == 0 ? BlockType.Empty : context.Depth.Peek();

            if (blockType != BlockType.Empty)
            {
                context.Stack.Push((ValueType)blockType);
            }

            context.Emit(OpCodes.Break);
#endif
            context.Emit(OpCodes.Newobj, typeof(UnreachableException).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
            context.Emit(OpCodes.Throw);
        }
    }
}