using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvh.Kaleidoscope
{
    [Flags]
    public enum FlipMode
    {
        None = 0,
        X = 1,
        Y = 2,
        XY = 4,

    }
}
