using OpenKh.Kh2;
using System.IO;

namespace OpenKh.Unity.Tools.IdxImg.Interfaces
{
    interface IIdxManager
    {
        Stream OpenFileFromIdx(string fileName);
        Stream OpenFileFromIdx(Idx.Entry entry);
    }
}
