using System.Collections.Generic;

namespace SquidLib.SquidMath {
    public interface IOrdered<T> {
        List<T> Ordering {
            get;
        }
    }
}
