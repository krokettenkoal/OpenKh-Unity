using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenKh.Unity.MdlxMset.Encoding;
using OpenKh.Unity.MdlxMset.Texture;

namespace OpenKh.Unity.MdlxMset.Model {
    public class TIMReader {
        public static Texex2 Load(Stream fs) {
            var alt = new Texex2();
            int wcx, wcy;
            byte[] refal;

            var br = new BinaryReader(fs);
            fs.Position = 8;
            wcx = br.ReadInt32(); // @ 0x08
            wcy = br.ReadInt32(); // @ 0x0C
            var woff = br.ReadInt32(); // @ 0x10
            var texinf1off = br.ReadInt32(); // @0x14
            var texinf2off = br.ReadInt32(); // @0x18
            fs.Position = woff;
            refal = br.ReadBytes(wcy);
            fs.Position = 0x1c;
            var picoff = br.ReadInt32(); // @0x1C
            var paloff = br.ReadInt32(); // @0x20

            fs.Position = texinf1off;
            _ = br.ReadBytes(texinf2off - texinf1off);
            fs.Position = texinf2off;
            _ = br.ReadBytes(picoff - texinf2off);
            fs.Position = picoff;
            _ = br.ReadBytes(paloff - picoff);
            _ = br.ReadBytes(Convert.ToInt32(fs.Length) - 4 - paloff);

            var gsram = new byte[4 * 1024 * 1024];

            // TEXTURE MAPPING PROGRAM
            for (var wy = 0; wy < wcy; wy++) {
                if (true) {
                    // TEXTURE PREPARATION PROGRAM
                    for (var wi = 0; wi < 2; wi++) {
                        var wx = (wi == 0) ? 0 : (1 + refal[wy]);
                        fs.Position = texinf1off + 144 * wx + 0x20;
                        var v0 = br.ReadUInt64();
                        _ = (int)(v0 >> 0) & 0x3FFF;
                        _ = (int)(v0 >> 16) & 0x3F;
                        _ = (int)(v0 >> 24) & 0x3F;
                        var dbp = (int)(v0 >> 32) & 0x3FFF;
                        var dbw = (int)(v0 >> 48) & 0x3F;
                        var dpsm = (int)(v0 >> 56) & 0x3F;
                        Trace.Assert(br.ReadUInt64() == 0x50, "Unexpected texture format");

                        fs.Position = texinf1off + 144 * wx + 0x40;
                        var v2 = br.ReadUInt64();
                        _ = (int)(v2 >> 0) & 0xFFF;
                        _ = (int)(v2 >> 32) & 0xFFF;
                        Trace.Assert(br.ReadUInt64() == 0x52, "Unexpected texture format");

                        fs.Position = texinf1off + 144 * wx + 0x60;
                        UInt64 v4 = br.ReadUInt64();
                        var nloop = ((int)(v4 >> 0) & 0x3FFF);

                        fs.Position = texinf1off + 144 * wx + 0x70;
                        var v5 = br.ReadUInt64();
                        var ilen = ((int)(v5 >> 0) & 0x3FFF);
                        var ioff = ((int)(v5 >> 32) & 0x7FFFFFFF);
                        Trace.Assert(nloop == ilen, "Unexpected texture format");

                        fs.Position = ioff;
                        var ibin = new byte[16 * ilen];
                        _ = fs.Read(ibin, 0, 16 * ilen);

                        Trace.Assert(dpsm == 0, "Unexpected texture format");
                        var dbh = Convert.ToInt32(ibin.Length) / 8192 / dbw;
                        ibin = Reform32.Encode32(ibin, dbw, dbh);

                        Array.Copy(ibin, 0, gsram, 256 * dbp, 16 * ilen);

                        Console.Write("");
                    }
                }

                {
                    Debug.Assert(refal[wy] < wcx, "Invalid");

                    fs.Position = texinf2off + 160 * wy + 0x20;
                    var v0 = br.ReadUInt64();
                    Trace.Assert(v0 == 0, "Unexpected texture format");
                    Trace.Assert(br.ReadUInt64() == 0x3F, "Unexpected texture format");

                    fs.Position = texinf2off + 160 * wy + 0x30;
                    var v1 = br.ReadUInt64();
                    Trace.Assert(v1 == 0, "Unexpected texture format");
                    Trace.Assert(br.ReadUInt64() == 0x34, "Unexpected texture format");

                    fs.Position = texinf2off + 160 * wy + 0x40;
                    var v2 = br.ReadUInt64();
                    Trace.Assert(v2 == 0, "Unexpected texture format");
                    Trace.Assert(br.ReadUInt64() == 0x36, "Unexpected texture format");

                    fs.Position = texinf2off + 160 * wy + 0x50;
                    var v3 = br.ReadUInt64();
                    _ = (int)(v3 >> 0) & 0x3F;
                    _ = (int)(v3 >> 37) & 0x3FFF;
                    _ = (int)(v3 >> 51) & 0xF;
                    _ = (int)(v3 >> 55) & 0x1;
                    _ = (int)(v3 >> 56) & 0x1F;
                    _ = (int)(v3 >> 61) & 0x7;
                    Trace.Assert(br.ReadUInt64() == 0x16, "Unexpected texture format");

                    fs.Position = texinf2off + 160 * wy + 0x70;
                    var v5 = br.ReadUInt64();
                    var tbp0 = (int)(v5 >> 0) & 0x3FFF;
                    var tbw = (int)(v5 >> 14) & 0x3F;
                    var psmX = (int)(v5 >> 20) & 0x3F;
                    var tw = (int)(v5 >> 26) & 0xF;
                    var th = (int)(v5 >> 30) & 0xF;
                    _ = (int)(v5 >> 34) & 0x1;
                    _ = (int)(v5 >> 35) & 0x3;
                    var cbpX = (int)(v5 >> 37) & 0x3FFF;
                    _ = (int)(v5 >> 51) & 0xF;
                    _ = (int)(v5 >> 55) & 0x1;
                    _ = (int)(v5 >> 56) & 0x1F;
                    _ = (int)(v5 >> 56) & 0x7;
                    Trace.Assert(br.ReadUInt64() == 0x06, "Unexpected texture format");
                    //TransUtil.Exist(texbuf, cbpX);
                    //Trace.Assert(texbuf.ContainsKey(cbpX), "Invalid");
                    //Trace.Assert(texbuf.ContainsKey(tbp0), "Invalid");
                    //Trace.Assert(cpsmX == 0, "Unsupported");
                    //Trace.Assert(csmX == 0, "Unsupported");
                    //Trace.Assert(csaX == 0, "Unsupported");

                    var sizetbp0 = (1 << tw) * (1 << th);
                    var buftbp0 = new byte[sizetbp0];
                    Array.Copy(gsram, 256 * tbp0, buftbp0, 0, buftbp0.Length);
                    var bufcbpX = new byte[8192];
                    Array.Copy(gsram, 256 * cbpX, bufcbpX, 0, bufcbpX.Length);

                    TIMBitmap ipic = psmX switch
                    {
                        0x13 => TexUtil.Decode8(buftbp0, bufcbpX, tbw, 1 << tw, 1 << th),
                        0x14 => TexUtil.Decode4(buftbp0, bufcbpX, tbw, 1 << tw, 1 << th),
                        _ => null
                    };

                    if(ipic != null)
                        alt.bitmapList.Add(ipic);
                }
            }

            for (var offx0 = 0; offx0 < fs.Length; offx0 += 16) {
                fs.Position = offx0;
                var bin = br.ReadBytes(16);
                if (bin[0] != 0x5F || bin[1] != 0x44 || bin[2] != 0x4D || bin[3] != 0x59)
                {
                    continue;
                } 
                // _DMY
                if (bin[8] != 0x54 || bin[9] != 0x45 || bin[10] != 0x58 || bin[11] != 0x41)
                {
                    continue;
                } 
                // TEXA
                fs.Position = offx0 + 16 + 0x02;
                int texi = br.ReadUInt16(); // @0x02 texi
                fs.Position = offx0 + 16 + 0x0C;
                _= br.ReadUInt16(); // @0x0C cntt2
                int ycnt = br.ReadUInt16(); // @0x0E ycnt
                int patx = br.ReadUInt16(); // @0x10 patx
                int paty = br.ReadUInt16(); // @0x12 paty
                int patcx = br.ReadUInt16(); // @0x14 patcx
                int patcy = br.ReadUInt16(); // @0x16 patcy
                var offt1 = br.ReadInt32(); // @0x18 offt1
                var offt2 = br.ReadInt32(); // @0x1C offt2
                var patcpicoff = br.ReadInt32(); // @0x20 picoff

                fs.Position = offx0 + 16 + patcpicoff;
                var bits = br.ReadBytes(patcx * patcy * ycnt);
                FacePatch patc;
                alt.facePatchList.Add(patc = new FacePatch(bits, patx, paty, patcx, patcy, ycnt, texi));

                {
                    var toff = offx0 + 16;

                    var cntt1 = (offt2 - offt1) / 2;
                    var ale1 = new List<FaceTexture>();
                    for (var y = 0; y < cntt1; y++) {
                        fs.Position = toff + offt1 + 2 * y;
                        var x = br.ReadInt16() - 1;
                        if (x < 0)
                        {
                            continue;
                        }

                        fs.Position = toff + offt2 + 4 * x;
                        var offx1 = br.ReadInt32();
                        fs.Position = toff + offx1;
                        var z = 0;
                        while (true) {
                            var e1 = new FaceTexture {
                                i0 = y,
                                i1 = x,
                                i2 = z,
                                v0 = br.ReadInt16(),
                                v2 = br.ReadInt16(),
                                v4 = br.ReadInt16(),
                                v6 = br.ReadInt16()
                            };

                            ale1.Add(e1);
                            z++;
                            if (e1.v0 < 0)
                                break;
                        }
                    }

                    patc.faceTextureList = ale1.ToArray();
                }
            }

            return alt;
        }

        class TexUtil {
            public static TIMBitmap Decode8(byte[] picbin, byte[] palbin, int tbw, int cx, int cy) {
                var pic = new Bitmap(cx, cy, PixelFormat.Format8bppIndexed);
                tbw /= 2;
                Debug.Assert(tbw != 0, "Invalid");
                var bin = Reform8.Decode8(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
                var bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

                try {
                    var buffSize = bd.Stride * bd.Height;
                    Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
                }
                finally {
                    pic.UnlockBits(bd);
                }
                var pals = pic.Palette;
                var psi = 0;

                var palb2 = new byte[1024];
                for (var t = 0; t < 256; t++) {
                    var toi = KHcv8pal.repl(t);
                    Array.Copy(palbin, 4 * t + 0, palb2, 4 * toi + 0, 4);
                }
                Array.Copy(palb2, 0, palbin, 0, 1024);

#if false
                for (int t = 0; t < 256; t++) {
                    int toi = (t & 0xE7) | (((t & 0x10) != 0) ? 0x08 : 0) | (((t & 0x08) != 0) ? 0x10 : 0);
                    Array.Copy(palbin, 4 * t, palb2, toi * 4, 4);
                }
                Array.Copy(palb2, 0, palbin, 0, 1024);

                for (int t = 0; t < 256; t++) {
                    if (palbin[4 * t + 3] != 0x80) {
                        palbin[4 * t + 0] = 0;
                        palbin[4 * t + 1] = 0;
                        palbin[4 * t + 2] = 0;
                        palbin[4 * t + 3] = 0;
                    }
                }
#endif

                for (var pi = 0; pi < 256; pi++) {
                    pals.Entries[pi] = CUtil.NoGamma(Color.FromArgb(
                        AcUt.GetA(palbin[psi + 4 * pi + 3]) ^ (pi & 1),
                        Math.Min(255, palbin[psi + 4 * pi + 0] + 1),
                        Math.Min(255, palbin[psi + 4 * pi + 1] + 1),
                        Math.Min(255, palbin[psi + 4 * pi + 2] + 1)
                        ));
                }
                pic.Palette = pals;
                //pic.Save("α.png", ImageFormat.Png);

                return new TIMBitmap(pic);
            }

            class AcUt {
                public static byte GetA(byte a)
                {
                    return 0 < a ? (byte) 255 : (byte) 0;
                }
            }

            public static TIMBitmap Decode4(byte[] picbin, byte[] palbin, int tbw, int cx, int cy) {
                var pic = new Bitmap(cx, cy, PixelFormat.Format4bppIndexed);
                tbw /= 2;
                Debug.Assert(tbw != 0, "Invalid");
                var bin = Reform4.Decode4(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
                var bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, PixelFormat.Format4bppIndexed);

                try {
                    var buffSize = bd.Stride * bd.Height;
                    Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
                }
                finally {
                    pic.UnlockBits(bd);
                }
                var pals = pic.Palette;
                var psi = 0;
                for (var pi = 0; pi < 16; pi++) {
                    pals.Entries[pi] = CUtil.NoGamma(Color.FromArgb(
                        AcUt.GetA(palbin[psi + 4 * pi + 3]),
                        palbin[psi + 4 * pi + 0],
                        palbin[psi + 4 * pi + 1],
                        palbin[psi + 4 * pi + 2]
                        ));
                }
                pic.Palette = pals;

                return new TIMBitmap(pic);
            }
        }

        public const float γ = 0.5f;

        class CUtil {
            public static Color Gamma(Color a, float gamma) {
                return Color.FromArgb(
                    a.A,
                    Math.Min(255, (int)(Math.Pow(a.R / 255.0, gamma) * 255.0)),
                    Math.Min(255, (int)(Math.Pow(a.G / 255.0, gamma) * 255.0)),
                    Math.Min(255, (int)(Math.Pow(a.B / 255.0, gamma) * 255.0))
                    );
            }
            public static Color NoGamma(Color a) {
                return Color.FromArgb(
                    a.A,
                    a.R,
                    a.G,
                    a.B
                    );
            }
        }
    }
}
