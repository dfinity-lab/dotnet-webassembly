using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

using System.IO;


[assembly: DebuggerTypeProxy(typeof(ActorScript.Value.Display), Target = typeof(ActorScript.Value))]
#if !ORIG 


namespace WebAssembly
{

    using LabelMap = Dictionary<uint, string>;
    internal enum ActorScriptSubsection : byte
    {
        Labels = 0
    }
    /// <summary>
    /// 
    /// </summary>
    public class ActorScriptSection
    {
        // c.f. https://webassembly.org/docs/binary-encoding/
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public LabelMap Labels;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        public ActorScriptSection(CustomSection cs)
        {
            var stream = new MemoryStream((byte[])cs.Content);
            var reader = new Reader(stream);
            var previousSection = ActorScriptSubsection.Labels;
            var preSectionOffset = reader.Offset;
            while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
            {
                if (id != 0 && (ActorScriptSubsection)id < previousSection)
                    throw new ModuleLoadException($"Sections out of order; section {(ActorScriptSubsection)id} encounterd after {previousSection}.", preSectionOffset);
                var payloadLength = reader.ReadVarUInt32();

                switch ((ActorScriptSubsection)id)
                {

                    case ActorScriptSubsection.Labels:
                        {

                            var count = reader.ReadVarUInt32();
                            Labels = new LabelMap((int)count);
                            for (int i = 0; i < count; i++)
                            {
                                var index = reader.ReadVarUInt32();
                                var nameLength = reader.ReadVarUInt32();
                                var name = reader.ReadString(nameLength);
                                Labels.Add(index, name);
                            }
                        }
                        break;

                    
                }

                previousSection = (ActorScriptSubsection)id;
            }
        }



    }
}


namespace ActorScript
{

    public static class Meta
    {
        public static WebAssembly.ActorScriptSection actorScriptSection = null;
    }
    public static class Globals
    {
        public static WebAssembly.Runtime.UnmanagedMemory mem = null;
        public static byte ReadByte(UInt32 index) => Marshal.ReadByte(IntPtr.Add(mem.Start, (int)index));
        public static int ReadInt32(UInt32 index) => Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index));

        public static long ReadInt64(UInt32 index) => Marshal.ReadInt64(IntPtr.Add(mem.Start, (int)index));
        public static Tag ReadTag(UInt32 index) => (Tag)Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index));
        public static Value ReadValue(UInt32 index) => (Value)unchecked((uint)Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index)));

        public static string ReadUTF8(UInt32 index, int byteLen) =>
            //@TODO fix me : use PtrToStringUTF8 which is documented but not available  (version skew?)
            Marshal.PtrToStringAnsi(IntPtr.Add(mem.Start, (int)index), byteLen);

        public static string ReadLabel(UInt32 index) =>
            Meta.actorScriptSection.Labels[
               unchecked((uint)Marshal.ReadInt32(IntPtr.Add(mem.Start, (int)index)))];

    }

    public enum Tag : int
    {
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

    [DebuggerDisplay("{ToString(),nq}")]
    public struct Field
    {
        public string lab;
        public Value val;
        public Field(string lab, Value val) { this.lab = lab; this.val = val; }

        public override string ToString() => lab + "=" + val;

    }

    [DebuggerDisplay("{ToString(),nq}")]
    public struct Variant
    {
        public string lab;
        public Value val;
        public Variant(string lab, Value val) { this.lab = lab; this.val = val; }

        public override string ToString() => "#" + lab + " " + val;
    }


    [DebuggerDisplay("{ToString(),nq}")]
    public struct Some
    {
        public Value val;
        public Some(Value val) { this.val = val; }
        public override string ToString() => "? " + val;
    }


    [DebuggerTypeProxy(typeof(Value.Display))]
    [DebuggerDisplay("{ToString(),nq}")]
    public struct Value
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public uint value;



        public Value(uint value) { this.value = value; }
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
                        if ((word & 2) == 2)
                        {
                            var ptr = word + 1;
                            obj = DecodeObject(ptr);
                        }
                        else
                        {
                            obj = (int)word >> 2;
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
                            var a = new Field[len];
                            var elem = ptr + (uint)sizeof(Tag) + (uint)sizeof(Int32);
                            for (uint i = 0; i < len; i++)
                            {
                                var label = Globals.ReadLabel(elem);
                                elem += (uint)sizeof(Int32);
                                var value = Globals.ReadValue(elem);
                                elem += (uint)sizeof(Int32);
                                a[i] = new Field(label, value);
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
                            for (uint i = 0; i < len; i++)
                            {
                                a[i] = Globals.ReadValue(elem);
                                elem += (uint)sizeof(Int32);
                            }
                            return a;
                        }
                    case Tag.Int:
                        {
                            var i = Globals.ReadInt64(ptr + (sizeof(Tag)));
                            return i;
                        }
                    case Tag.MutBox:
                        break;
                    case Tag.Closure:
                        break;
                    case Tag.Some:
                        {
                            var v = Globals.ReadValue(ptr + (sizeof(Tag)));
                            return new Some(v);
                        }
                    case Tag.Variant:
                        {
                            var elem = ptr + (uint)sizeof(Tag);
                            var label = Globals.ReadLabel(elem);
                            elem += (uint)sizeof(Int32);
                            var value = Globals.ReadValue(elem);
                            elem += (uint)sizeof(Int32);
                            var variant = new Variant(label, value);
                            return variant;
                        }
                    case Tag.Text:
                        {
                            var len = Globals.ReadInt32(ptr + sizeof(Tag));
                            return Globals.ReadUTF8(ptr + sizeof(Tag) + sizeof(Int32), len);
                        }
                    case Tag.Indirection:
                    case Tag.SmallWord:
                        break;
                    default: return "BAD TAG:" + (int)tag;
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