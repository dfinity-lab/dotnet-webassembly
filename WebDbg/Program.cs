
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;
using System;
using System.IO;

using SourcemapToolkit.SourcemapParser;

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

   
           
            // We now have enough for a usable WASM file, which we could save with module.WriteToBinary().
            // Below, we show how the Compile feature can be used for .NET-based execution.
            // For stream-based compilation, WebAssembly.Compile should be used.
            var instanceCreator = module.Compile<Sample>();

            // Instances should be wrapped in a "using" block for automatic disposal.
            using (var instance = instanceCreator())
            {
                // FYI, instanceCreator can be used multiple times to create independent instances.
               //Console.WriteLine(instance.Exports.Fac(1));// Binary 0, result 0

              
               
            } // Automatically release the WebAssembly instance here.
        }
    }
}

