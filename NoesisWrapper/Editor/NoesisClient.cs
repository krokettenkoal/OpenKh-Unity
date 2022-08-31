using System;
using System.Diagnostics;
using System.IO;
using OpenKh.Unity.Exporter.Progress;
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

        /// <summary>
        /// Export/convert a file to the file format specified in the destination path using Noesis.
        /// </summary>
        /// <param name="srcPath">Path to the source file being converted</param>
        /// <param name="destPath">Output path of the resulting file</param>
        /// <param name="advCmd">Advanced Noesis command flags</param>
        /// <returns>True if the export was successful</returns>
        public bool Export(string srcPath, string destPath, Func<OperationStatus, bool> onProgress, AdvancedCommands advCmd = AdvancedCommands.FbxMultitake)
        {
            var status = new OperationStatus
            {
                OperationType = "FBX import",
                Current = 1,
                Total = 1,
                ProgressFactor = 0,
                Message = Path.GetFileName(destPath),
            };

            if (onProgress?.Invoke(status) is true)
                return false;

            if (!File.Exists(_binPath) || !File.Exists(srcPath))
                return false;

            //  Create target directory if necessary
            var dir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            //  Set input and output files
            _startInfo.Arguments += $" \"{srcPath}\" \"{destPath}\"";

            //  Append specified flags as advanced Noesis commands
            foreach (var flag in advCmd.GetIndividualFlags())
            {
                _startInfo.Arguments += $" -{flag.ToString().ToLowerInvariant()}";
            }

            _startInfo.Arguments += "\"";

            //  Setup process
            using var proc = new Process();
            proc.StartInfo = _startInfo;

            //UnityEngine.Debug.Log($"Executing Noesis: {_startInfo.Arguments[6..]}");

            status.State = OperationState.Processing;
            status.ProgressFactor = .1f;
            if (onProgress?.Invoke(status) is true)
                return false;

            //  Execute process
            proc.Start();

            status.ProgressFactor = .2f;
            if (onProgress?.Invoke(status) is true)
            {
                proc.Kill();
                proc.Dispose();
                return false;
            }

            proc.WaitForExit();
            proc.Close();
            
            status.ProgressFactor = .5f;
            if (onProgress?.Invoke(status) is true)
            {
                proc.Kill();
                proc.Dispose();
                return false;
            }

            _startInfo.Arguments = BaseArguments;

            return true;
        }
    }
}
