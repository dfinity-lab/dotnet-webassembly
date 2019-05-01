
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using SourcemapToolkit.SourcemapParser;


[assembly: DebuggerTypeProxy(typeof(cstest.Display), Target = typeof(cstest.Val))]

namespace cstest
{

    //[ DebuggerTypeProxy(typeof(cstest.Display)) ]
    [DebuggerDisplay("")]
    public enum Val
    {

    }


    public class Memory
    {
        public static Val[] state = new Val[8] { (Val)0, (Val)1, (Val)2, (Val)3, (Val)0, (Val)1, (Val)2, (Val)3 };

    }
    public class Display
    {

        public object value;
        public Display(int v)
        {
            switch ((int)v)
            {
                case 0:
                    value = "one";
                    break;
                case 1:
                    value = true;
                    break;
                default:
                    value = Memory.state[((int)v) - 2];
                    break;
            }

        }

        public override string ToString()
        {
            switch (value)
            {
                case 0: return "I";
                case 1: return "B";
            };
            return value.ToString();
        }
    }



    [DebuggerTypeProxy(typeof(Value.Display))]
    public struct Value
    {
        public Int32 v;
        public Value(Int32 v)
        {
            this.v = v;

        }

        private class Display
        {

            public Int64 value;
            public Display(Value v)
            {
                this.value = v.v;
            }

            public override string ToString()
            {
                switch (value)
                {
                    case 0: return "I";
                    case 1: return "B";
                };
                return value.ToString();
            }
        }

    }
    class Program
    {
        public static void Maine(string[] args)
        {
            Val[] mem = Memory.state;
            Val w = (Val)0;

            Value v = new Value(1);
            Console.WriteLine("{0}{0}", v, w);

            w = (Val)1;
            w = (Val)2;
            w = (Val)3;
            w = (Val)4;
            Console.WriteLine("{0}{0}", v, w);
            foo(args);
        }

        static void foo(string[] args)
        {

            
            int[] f = new int[] { 1, 2, 3 };

            Console.WriteLine("Hello World! {0}", f
            );
        }

    }
}


namespace WebDbg { 

// We need this later to call the code we're generating.
public abstract class Sample
    {
        // Sometimes you can use C# dynamic instead of building an abstract class like this.
       // public abstract int Fac(int i);
    }


    static class Program
    {

        


        static void Main()
        {
            
            // Module can be used to create, read, modify, and write WebAssembly files.

            SourceMapParser parser = new SourceMapParser();
            SourceMap sourceMap;
            using (FileStream stream = new FileStream(@"../../fac.wasm.map", FileMode.Open))
            {
                sourceMap = parser.ParseSourceMap(new StreamReader(stream));
            }

            var parsedMapping = sourceMap.ParsedMappings;

            var module = Module.ReadFromBinary("../../fac.wasm");

            var nameSection = module.CustomSections.FirstOrDefault(cs => cs.Name == "name");

            var NameSection = new NameSection(nameSection);

            var actorScriptSection = module.CustomSections.FirstOrDefault(cs => cs.Name == "actorScript");

            Meta.actorScriptSection = new ActorScriptSection(actorScriptSection);

            // We now have enough for a usable WASM file, which we could save with module.WriteToBinary().
            // Below, we show how the Compile feature can be used for .NET-based execution.
            // For stream-based compilation, WebAssembly.Compile should be used.
            // var instanceCreator = module.Compile<Sample>();
            var instanceCreator = Compile.FromBinary<Sample>(@"../../fac.wasm");
            // Instances should be wrapped in a "using" block for automatic disposal.
            using (var instance = instanceCreator())
            {
                // FYI, instanceCreator can be used multiple times to create independent instances.
               //Console.WriteLine(instance.Exports.Fac(1));// Binary 0, result 0

              
               
            } // Automatically release the WebAssembly instance here.
        }
    }
}

