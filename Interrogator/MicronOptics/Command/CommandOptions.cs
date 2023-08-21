using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dock_Examples.Interrogator
{
    [Flags]
    public enum CommandOptions : byte
    {
        None = 0,
        SuppressMessage = 1,
        SuppressContent = 2,
        UseCompression = 4
    }
}
