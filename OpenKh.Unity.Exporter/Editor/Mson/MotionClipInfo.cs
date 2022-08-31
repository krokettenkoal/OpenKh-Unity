using System;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Aset.Motion;
using UnityEngine;

namespace OpenKh.Unity.Exporter.Mson
{
    [Serializable]
    public class MotionClipInfo
    {
        private const string DummyTag = "DUMMY";
        public string Tag;
        public int Slot;
        public int FrameCount;

        public MotionSet.MotionName Name => (MotionSet.MotionName)(Slot / 4);

        public MotionClipInfo() { }
        public MotionClipInfo(int slot) : this()
        {
            Slot = slot;
            Tag = DummyTag;
            FrameCount = 0;
        }
        public MotionClipInfo(int slot, MotionInformation info) : this()
        {
            Slot = slot;
            Tag = info.motion.id.Split('#')[0];
            FrameCount = (int) info.maxtick;
        }
        public MotionClipInfo(int slot, Bar.Entry entry)
        {
            Slot = slot;
            Tag = entry.Name;
        }
    }
}
