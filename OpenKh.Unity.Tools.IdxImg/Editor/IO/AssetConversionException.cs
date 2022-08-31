using System;

namespace OpenKh.Unity.Tools.IdxImg.IO
{
    public class AssetConversionException : Exception
    {
        public AssetConversionException() { }
        public AssetConversionException(string message) : base(message) { }
    }
}
