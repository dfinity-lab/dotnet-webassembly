using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

[assembly: DebuggerTypeProxy(typeof(WebAssembly.Value.Display), Target = typeof(WebAssembly.Value))]
#if !ORIG 
namespace WebAssembly
{
   [DebuggerTypeProxy(typeof(WebAssembly.Value.Display))]
   [DebuggerDisplay("")]
    public struct Value
    {
        public uint value;

        // DebuggerTypeProxy(typeof(WebAssembly.Value.Display), Target = typeof(WebAssembly.Value))

        [DebuggerDisplay("{obj}")]
        public class Display
        {

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
                        if ((word & 0b11 ) == word) {
                            obj = (word >> 2) << 2;
                        }
                        else
                        { obj = (int) word >> 2;
                        }// TBR;
                        break;
                }

            }

            public override string ToString()
            {
                return obj.ToString();
            }
        }
    }

}

#endif