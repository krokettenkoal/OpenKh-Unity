using System.Collections.Generic;
using System.Linq;
using OpenKh.Unity.MdlxMset.Motion;
using OpenKh.Unity.MdlxMset.Texture;

namespace OpenKh.Unity.MdlxMset.Model {
    internal class SelTexfacUt {
        public static PatTexSel[] Sel(List<FacePatch> alp, float tick, FaceModifier fm) {
            var sel = new PatTexSel[alp.Count];
            
            foreach (var f1 in fm.faces.Where(f1 => f1.v2 != -1 && f1.v0 <= tick && tick < f1.v2))
            {
                for (var x = 0; x < alp.Count; x++) {
                    var curt = (int)(tick - f1.v0) / 8;
                    
                    foreach (var tf in alp[x].faceTextureList)
                    {
                        if (tf.i0 != f1.v6)
                            continue;
                        
                        if (curt <= 0) {
                            if (sel[x] == null) {
                                sel[x] = new PatTexSel((byte)alp[x].textureIndex, (byte)tf.v6);
                                break;
                            }
                        }

                        curt -= tf.v2;
                    }
                }
            }
            return sel;
        }
    }
}
