namespace WebAssembly.Instructions
{
	/// <summary>
	/// Extend an unsigned 32-bit integer to a 64-bit integer.
	/// </summary>
	public class Int64ExtendUnsignedInt32 : Instruction
	{
		/// <summary>
		/// Always <see cref="OpCode.Int64ExtendUnsignedInt32"/>.
		/// </summary>
		public sealed override OpCode OpCode => OpCode.Int64ExtendUnsignedInt32;

		/// <summary>
		/// Creates a new  <see cref="Int64ExtendUnsignedInt32"/> instance.
		/// </summary>
		public Int64ExtendUnsignedInt32()
		{
		}
	}
}