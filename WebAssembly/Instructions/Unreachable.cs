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
#if !ORIG  
            
            //@TODO REVISIT ME: needs better stack typing with ellipeses
            var blockEntry = context.Depth.Count == 0 ? context.BlockEntry(context.Signature.RawReturnTypes) : context.Depth.Peek();


            foreach (var type in blockEntry.Types)
            {
                context.Stack.Push(type);
            }
            
            context.Emit(OpCodes.Break);
#endif
            context.Emit(OpCodes.Newobj, typeof(UnreachableException).GetTypeInfo().DeclaredConstructors.First(c => c.GetParameters().Length == 0));
            context.Emit(OpCodes.Throw);

#if !ORIG
            
            foreach (var type in blockEntry.Types) //this is wrong
            {
                var local = context.DeclareLocal(((ValueType) type).ToSystemType());

                context.Emit(OpCodes.Ldloc, local.LocalIndex);
            }

           /*

            
            var label = context.DefineLabel();
            context.MarkLabel(label);
            context.Emit(OpCodes.Br, label); 
            */
#endif
        }
    }
}