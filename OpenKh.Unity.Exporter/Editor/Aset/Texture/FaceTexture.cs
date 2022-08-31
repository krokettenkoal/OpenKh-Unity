namespace OpenKh.Unity.Exporter.Aset.Texture {
    public class FaceTexture {
        public int i0, i1, i2;
        public short v0, v2, v4, v6;

        public override string ToString() {
            return $"{i0,3},{i1,2},{i2,2}|{v0,4},{v2,3},{v4,3},{v6,3}";
        }
    }
}
