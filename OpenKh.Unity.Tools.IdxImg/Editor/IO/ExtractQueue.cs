using System.Collections.Generic;
using System.Linq;
using OpenKh.Unity.Tools.IdxImg.ViewModels;
using Unity.Collections;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public static class ExtractQueue
    {
        public static List<FileViewModel> Active { get; } = new();
    }
}
