using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

[assembly: DebuggerTypeProxy(typeof(WebAssembly.Value.Display), Target = typeof(WebAssembly.Value))]
#if !ORIG 
namespace WebAssembly {

    
    public static class Globals
    {
        public static Runtime.UnmanagedMemory mem = null;
        public static byte ReadByte(UInt32 index) => Marshal.ReadByte(IntPtr.Add(mem.Start, (int) index));
        public static int ReadInt32(UInt32 index) => Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index));
        public static Tag ReadTag(UInt32 index) => (Tag) Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index));
        public static Value ReadValue(UInt32 index) => (Value) unchecked((uint) Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index)));

        public static string ReadUTF8(UInt32 index, int byteLen) =>
            //@TODO fix me : use PtrToStringUTF8 which is documented but not available  (version skew?)
            Marshal.PtrToStringAnsi(IntPtr.Add(mem.Start, (int)index),byteLen);

    }

    public enum Tag : int { 
    Object = 1,
    ObjInd = 2,
    Array = 3,
    Reference = 4,
    Int = 5,
    MutBox = 6,
    Closure = 7,
    Some = 8,
    Variant = 9,
    Text = 10,
    Indirection = 11,
    SmallWord = 12,
   };

   [DebuggerTypeProxy(typeof(WebAssembly.Value.Display))]
   [DebuggerDisplay("{ToString(),nq}")]
    public struct Value
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public uint value;

        public Value (uint value) { this.value = value; }
        // DebuggerTypeProxy(typeof(WebAssembly.Value.Display), Target = typeof(WebAssembly.Value))
        public static explicit operator Value(uint v) { return new Value(v); }

        public override string ToString()
        {
            return new Display(this).obj.ToString();
        }

        [DebuggerDisplay("{obj}")]
        public class Display
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public object obj = null;
            public Display(Value v)
            {
                var word = v.value;
                switch (v.value)
                {
                    case 0:
                        obj = false;
                        break;
                    case 1:
                        obj = true;
                        break;
                    default:
                        if ((word & 2 ) == 2) {
                            var ptr = word + 1;
                            obj = DecodeObject(ptr);             
                        }
                        else
                        { obj = (int) word >> 2;
                        }// TBR;
                        break;
                }

            }

            private object DecodeObject(uint ptr)
            {
                var tag = Globals.ReadTag(ptr);
                switch (tag)
                {
                    case Tag.Object:
                        {
                            var len = Globals.ReadInt32(ptr + (sizeof(Tag)));
                            var a = new Tuple<Int32,Value>[len];
                            var elem = ptr + (uint)sizeof(Tag) + (uint)sizeof(Int32);
                            for (uint i = 0; i < len; i++)
                            {
                                var hash = Globals.ReadInt32(elem);
                                elem += (uint)sizeof(Int32);
                                var value = Globals.ReadValue(elem);
                                elem += (uint)sizeof(Int32);
                                a[i] = new Tuple<int, Value>(hash, value);
                            }
                            return a;
                        }
                    case Tag.ObjInd:
                        break;
                    case Tag.Array:
                        {
                            var len = Globals.ReadInt32(ptr + (sizeof(Tag)));
                            var a = new Value[len];
                            var elem = ptr + (uint)sizeof(Tag) + (uint)sizeof(Int32);
                            for (uint i = 0; i<len; i++)
                            {   
                                a[i] = Globals.ReadValue(elem);
                                elem += (uint)sizeof(Int32);
                            }
                            return a;
                        }
                    case Tag.Int:
                        break;
                    case Tag.MutBox:
                        break;
                    case Tag.Closure:
                        break;
                    case Tag.Some:
                        {    
                            var v = Globals.ReadValue(ptr+(sizeof(Tag)));
                            return new Nullable<Value>(v);
                        }
                    case Tag.Variant:
                        break;
                    case Tag.Text:
                        {
                            var len = Globals.ReadInt32(ptr + sizeof(Tag));
                            return Globals.ReadUTF8(ptr + sizeof(Tag) + sizeof(Int32),len);
                        }
                    case Tag.Indirection:
                    case Tag.SmallWord:
                        break;
                    default: return "BAD TAG:" + (int) tag;
                }
                return tag;
            }


            public override string ToString()
            {
                return obj.ToString();
            }
        }
    }

}

#endif