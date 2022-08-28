using System.Runtime.InteropServices;
using UnityEngine;

namespace OpenKh.Unity.MdlxMset.Model
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PTex3 {
        public const int Size = 4 + 4 + 4 + (4 * 2 * 3);

        public float X;
        public float Y;
        public float Z;
        public float Tu1, Tv1;
        public float Tu2, Tv2;
        public float Tu3, Tv3;

        public PTex3(Vector3 pos, Vector2 tex) {
            X = pos.x;
            Y = pos.y;
            Z = pos.z;
            Tu1 = tex.x;
            Tv1 = tex.y;
            Tu2 = tex.x;
            Tv2 = tex.y;
            Tu3 = tex.x;
            Tv3 = tex.y;
        }
    }
}
