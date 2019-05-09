
using WebAssembly; // Acquire from https://www.nuget.org/packages/WebAssembly
using WebAssembly.Instructions;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

using SourcemapToolkit.SourcemapParser;


/*
    status: fib and quicksort debuggable.

    todo: 
    * actually set results from multivalue returns
    * multivalue block type reader and general support (multivalue block returns should just work, inputs not)
    * round out display of remaining values (closures, boxed mutables)
    * figure out polymorphic stack typing for unreachable (and others?)
    * cleanup
    * remove hardcoded filenames fac.as[.wasm]
    * get existing tests working again
    * display symbolic names for closure function pointers.
    * figure out whether we need more info for closure environments (if compiler loads to named locals on entry, we don't).
    * perhaps abstract unverifiable code to reduce peverify errors
    * for dotnetcore debugger, make value an enum, not struct or figure out if new (byref) structs can contain byrefs.
    * emit portable pdbs, not windows pdbs
 */

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

            ActorScript.Meta.actorScriptSection = new ActorScriptSection(actorScriptSection);

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

