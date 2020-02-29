using System;
using System.Collections.Generic;
using System.Text;

namespace SquidLib.SquidMath {
    public interface IOrdered<T> {
        List<T> Ordering {
            get;
        }
    }
}
