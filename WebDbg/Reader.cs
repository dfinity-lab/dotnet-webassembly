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

    public enum NameSubsection : byte
    {
        Module,
        Function,
        Local
    }
    
    public class NameSection
    {
       
        public string Name;

        public FunctionMap Functions;

        public LocalMap Locals;

        public NameSection(CustomSection cs)
        {
            var stream = new MemoryStream((byte[])cs.Content);
            var reader = new Reader(stream);
            var previousSection = NameSubsection.Module;
            var preSectionOffset = reader.Offset;
            while (reader.TryReadVarUInt7(out var id)) //At points where TryRead is used, the stream can safely end.
            {
                if (id != 0 && (NameSubsection) id < previousSection)
                    throw new ModuleLoadException($"Sections out of order; section {(NameSubsection)id} encounterd after {previousSection}.", preSectionOffset);
                var payloadLength = reader.ReadVarUInt32();

                switch ((NameSubsection)id)
                {
                    case NameSubsection.Module:
                    {
                      var nameLength = reader.ReadVarUInt32();
                      var Name = reader.ReadString(nameLength);
                    }
                    break;

                    case NameSubsection.Function:
                        {
                            
                            var count = reader.ReadVarUInt32();
                            Functions = new FunctionMap((int) count);
                            for (int i = 0;  i < count; i++) {
                                var index = reader.ReadVarUInt32();
                                var nameLength = reader.ReadVarUInt32();
                                var name = reader.ReadString(nameLength);
                                Functions.Add(index, name);
                            }
                        }
                        break;

                    case NameSubsection.Local :
                        {
                            var fun_count = reader.ReadVarUInt32();
                            Locals = new LocalMap((int) fun_count);
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

    internal sealed class Reader : IDisposable
    {
        private readonly UTF8Encoding utf8 = new UTF8Encoding(false, false);
        private BinaryReader reader;
        private long offset;

        public Reader(Stream input)
        {
            //The UTF8 encoding parameter is not actually used; it's just there so that the leaveOpen parameter can be reached.
            this.reader = new BinaryReader(input, utf8, true);
        }

        public long Offset => this.offset;

        public uint ReadUInt32()
        {
            Assert(this.reader != null);

            var result = this.reader.ReadUInt32();
            this.offset += 4;
            return result;
        }

        public byte ReadVarUInt1() => (Byte)(this.ReadVarUInt32() & 0b1);

        public bool TryReadVarUInt7(out byte result)
        {
            try
            {
                result = this.ReadVarUInt7();
                return true;
            }
            catch (EndOfStreamException)
            {
                result = 0;
                return false;
            }
        }

        public byte ReadVarUInt7() => (byte)(this.ReadVarUInt32() & 0b1111111);

        public sbyte ReadVarInt7() => (sbyte)(this.ReadVarInt32() & 0b11111111);

        public uint ReadVarUInt32()
        {
            var result = 0u;
            var shift = 0;
            while (true)
            {
                uint value = this.ReadByte();
                result |= ((value & 0x7F) << shift);
                if ((value & 0x80) == 0)
                    break;
                shift += 7;
            }

            return result;
        }

        public int ReadVarInt32()
        {
            var result = 0;
            int current;
            var count = 0;
            var signBits = -1;
            var initialOffset = this.offset;
            do
            {
                current = this.ReadByte();
                result |= (current & 0x7F) << (count * 7);
                signBits <<= 7;
                count++;
            } while (((current & 0x80) == 0x80) && count < 5);

            if ((current & 0x80) == 0x80)
                throw new ModuleLoadException("Invalid LEB128 sequence.", initialOffset);

            if (((signBits >> 1) & result) != 0)
                result |= signBits;

            return result;
        }

        public long ReadVarInt64()
        {
            var result = 0L;
            long current;
            var count = 0;
            var signBits = -1L;
            var initialOffset = this.offset;
            do
            {
                current = this.ReadByte();
                result |= (current & 0x7F) << (count * 7);
                signBits <<= 7;
                count++;
            } while (((current & 0x80) == 0x80) && count < 10);

            if ((current & 0x80) == 0x80)
                throw new ModuleLoadException("Invalid LEB128 sequence.", initialOffset);

            if (((signBits >> 1) & result) != 0)
                result |= signBits;

            return result;
        }

        public float ReadFloat32()
        {
            Assert(this.reader != null);

            var result = this.reader.ReadSingle();
            this.offset += 4;
            return result;
        }

        public double ReadFloat64()
        {
            Assert(this.reader != null);

            var result = this.reader.ReadDouble();
            this.offset += 8;
            return result;
        }

        public string ReadString(uint length) => utf8.GetString(this.ReadBytes(length));

        public byte[] ReadBytes(uint length)
        {
            Assert(this.reader != null);

            var result = this.reader.ReadBytes(checked((int)length));
            this.offset += length;
            return result;
        }

        public byte ReadByte()
        {
            Assert(this.reader != null);

            var result = this.reader.ReadByte();
            this.offset++;
            return result;
        }

        #region IDisposable Support
        void Dispose(bool disposing)
        {
            if (this.reader == null)
                return;

            try //Tolerate bad dispose implementations.
            {
                this.reader.Dispose();
            }
            catch
            {
            }

            this.reader = null;
        }

        ~Reader() => Dispose(false);

        /// <summary>
        /// Releases unmanaged resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}