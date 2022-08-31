using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Unity.Exporter.Aset.Motion {
    internal class UtwexMotionSel {
        public static SingleMotion Sel(int k1, List<SingleMotion> al1) => al1.FirstOrDefault(o => o.k1 == k1);
    }
}
