using System;
using System.Diagnostics;
using System.IO;
using OpenKh.Unity.Extensions;

namespace Noesis
{
    [Flags]
    public enum AdvancedCommands : byte
    {
        None = 0,
        FbxMultitake = 1 << 0,
    }
    public class NoesisClient
    {
        private string _binPath;
        private ProcessStartInfo _startInfo;
        private string BaseArguments => $"/S /C \"\"{_binPath}\" ?cmode";

        public NoesisClient() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Noesis", "Noesis.exe")) { }
        public NoesisClient(string binPath)
        {
            if (!File.Exists(binPath))
                throw new FileNotFoundException($"Noesis binary not found: {binPath}");

            _binPath = binPath;
            _startInfo = new ProcessStartInfo("cmd", BaseArguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
            };
        }

        public bool Export(string srcPath, string destPath, AdvancedCommands advCmd = AdvancedCommands.FbxMultitake)
        {
            if (!File.Exists(_binPath) || !File.Exists(srcPath))
                return false;

            var dir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _startInfo.Arguments += $" \"{srcPath}\" \"{destPath}\"";

            foreach (var flag in advCmd.GetIndividualFlags())
            {
                _startInfo.Arguments += $" -{flag.ToString().ToLowerInvariant()}";
            }

            _startInfo.Arguments += "\"";

            using var proc = new Process();
            proc.StartInfo = _startInfo;

            //UnityEngine.Debug.Log($"Executing Noesis: {_startInfo.Arguments[6..]}");

            proc.Start();
            proc.WaitForExit();
            proc.Close();

            _startInfo.Arguments = BaseArguments;

            return true;
        }
    }
}
