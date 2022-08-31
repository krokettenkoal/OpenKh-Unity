namespace OpenKh.Unity.Exporter.Aset.Model {
    internal class Sepa {
        public int svi;
        public int cnt;
        public int t;
        public int sel;

        public Sepa(int startVertexIndex, int cntPrimitives, int ti, int sel) {
            this.svi = startVertexIndex;
            this.cnt = cntPrimitives;
            this.t = ti;
            this.sel = sel;
        }
    }
}
