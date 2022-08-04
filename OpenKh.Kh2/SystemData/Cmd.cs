using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Cmd
    {
        public enum Action : byte
        {
            Null = 0,
            Idle = 1,
            Jump = 2,
        }

        public enum Camera : byte
        {
            Null = 0,
            Watch = 1,
            LockOn = 2,
            WatchLockOn = 3,
        }

        public enum Icon : byte
        {
            Null = 0,
            Attack = 1,
            Magic = 2,
            Item = 3,
            Form = 4,
            Summon = 5,
            Friend = 6,
            Limit = 7,
        }

        public enum Receiver : byte
        {
            Player = 0,
            Target = 1,
            Both = 2,
        }

        [Data] public ushort Id { get; set; }
        [Data] public ushort Execute { get; set; }
        [Data] public short Argument { get; set; } //this can be Argument, Form, Magic
        [Data] public sbyte SubMenu { get; set; }
        [Data] public Icon CmdIcon { get; set; }
        [Data] public int MessageId { get; set; }
        [Data] public uint Flags { get; set; }
        [Data] public float Range { get; set; }
        [Data] public float Dir { get; set; }
        [Data] public float DirRange { get; set; }
        [Data] public byte Cost { get; set; }
        [Data] public Camera CmdCamera { get; set; }
        [Data] public byte Priority { get; set; }
        [Data] public Receiver CmdReceiver { get; set; }
        [Data] public ushort Time { get; set; }
        [Data] public ushort Require { get; set; }
        [Data] public byte Mark { get; set; }
        [Data] public Action CmdAction { get; set; }
        [Data] public ushort ReactionCount { get; set; }
        [Data] public ushort DistRange { get; set; }
        [Data] public ushort Score { get; set; }
        [Data] public ushort DisableForm { get; set; }
        [Data] public byte Group { get; set; }
        [Data] public byte Reserve { get; set; }

        public static List<Cmd> Read(Stream stream) => BaseTable<Cmd>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Cmd> entries) =>
            BaseTable<Cmd>.Write(stream, 2, entries);
    }
}
