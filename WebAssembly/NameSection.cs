using System;
using System.IO;
using System.Text;
using static System.Diagnostics.Debug;
using System.Collections.Generic;

namespace WebAssembly
{

    using FunctionMap = Dictionary<uint, string>;
    using NameMap = Dictionary<uint, string>;
    using LocalMap = Dictionary<uint, Dictionary<uint, string>>;

    internal enum NameSubsection : byte
    {
        Module,
        Function,
        Local
    }
    /// <summary>
    /// 
    /// </summary>
    public class NameSection
    {
        // c.f. https://webassembly.org/docs/binary-encoding/
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public FunctionMap Functions;
        /// <summary>
        /// 
        /// </summary>
        public LocalMap Locals;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        public NameSection(CustomSection cs)
        {
            var stream = new MemoryStream((byte[])cs.Content);
            var reader = new Reader(stream);
            var previousSection = NameSubsection.Module;
            var preSectionOffset = reader.Offset;
            while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
            {
                if (id != 0 && (NameSubsection)id < previousSection)
                    throw new ModuleLoadException($"Sections out of order; section {(NameSubsection)id} encounterd after {previousSection}.", preSectionOffset);
                var payloadLength = reader.ReadVarUInt32();

                switch ((NameSubsection)id)
                {
                    case NameSubsection.Module:
                        {
                            var nameLength = reader.ReadVarUInt32();
                            Name = reader.ReadString(nameLength);
                        }
                        break;

                    case NameSubsection.Function:
                        {

                            var count = reader.ReadVarUInt32();
                            Functions = new FunctionMap((int)count);
                            for (int i = 0; i < count; i++)
                            {
                                var index = reader.ReadVarUInt32();
                                var nameLength = reader.ReadVarUInt32();
                                var name = reader.ReadString(nameLength);
                                Functions.Add(index, name);
                            }
                        }
                        break;

                    case NameSubsection.Local:
                        {
                            var fun_count = reader.ReadVarUInt32();
                            Locals = new LocalMap((int)fun_count);
                            for (int f = 0; f < fun_count; f++)
                            {
                                var fun_index = reader.ReadVarUInt32();

                                var count = reader.ReadVarUInt32();
                                var nameMap = new NameMap((int)count);
                                Locals.Add(fun_index, nameMap);
                                for (int i = 0; i < count; i++)
                                {
                                    var index = reader.ReadVarUInt32();
                                    var nameLength = reader.ReadVarUInt32();
                                    var name = reader.ReadString(nameLength);
                                    nameMap.Add(index, name);
                                }
                            }
                        }
                        break;

                }

                previousSection = (NameSubsection)id;
            }
        }



    }
}