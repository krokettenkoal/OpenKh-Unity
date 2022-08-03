using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OpenKh.Unity.Tools.Cli
{
    public static class Proc
    {
        public static Process Create(string executable, IEnumerable<string> arguments, string workingDirectory = null, bool executeInBackground = false)
        {
            if (string.IsNullOrEmpty(workingDirectory) || !Directory.Exists(workingDirectory))
                workingDirectory = "";

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = string.Join(" ", arguments),
                UseShellExecute = false,
                RedirectStandardError = executeInBackground,
                RedirectStandardOutput = executeInBackground,
                CreateNoWindow = executeInBackground,
                WorkingDirectory = workingDirectory,
            };

            return new Process()
            {
                StartInfo = startInfo
            };
        }

        public static void Spawn(string executable, IEnumerable<string> arguments, string workingDirectory = null, bool executeInBackground = false)
        {
            var proc = Create(executable, arguments, workingDirectory, executeInBackground);

            using (proc)
            {
                var output = string.Empty;
                var errOut = string.Empty;

                proc.Start();

                if (executeInBackground)
                {
                    output = proc.StandardOutput.ReadToEnd();
                    errOut = proc.StandardError.ReadToEnd();
                }

                proc.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    Debug.Log(output);

                if (!string.IsNullOrEmpty(errOut))
                    Debug.LogError(errOut);
            }
        }
    }

#if UNITY_EDITOR_WIN
    public static class Terminal
    {
        public static void Execute(string command, bool asAdmin = false, string workingDirectory = "", bool executeInBackground = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-Command " + command,
                UseShellExecute = asAdmin,
                RedirectStandardError = !asAdmin && executeInBackground,
                RedirectStandardOutput = !asAdmin && executeInBackground,
                CreateNoWindow = executeInBackground,
                WorkingDirectory = asAdmin ? "" : workingDirectory,
            };

            if (asAdmin)
                startInfo.Verb = "runas"; // Runs process in admin mode. See https://stackoverflow.com/questions/2532769/how-to-start-a-process-as-administrator-mode-in-c-sharp
            
            var proc = new Process()
            {
                StartInfo = startInfo
            };

            using (proc)
            {
                var output = string.Empty;
                var errOut = string.Empty;

                proc.Start();

                if (!asAdmin && executeInBackground)
                {
                    output = proc.StandardOutput.ReadToEnd();
                    errOut = proc.StandardError.ReadToEnd();
                }

                proc.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    Debug.Log(output);

                if (!string.IsNullOrEmpty(errOut))
                    Debug.LogError(errOut);
            }
        }
        public static void ExecuteScript(string scriptPath, bool asAdmin = false, string workingDirectory = "", bool executeInBackground = false)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"\"{scriptPath}\" -ExecutionPolicy Bypass",
                UseShellExecute = asAdmin,
                RedirectStandardOutput = !asAdmin && executeInBackground,
                RedirectStandardError = !asAdmin && executeInBackground,
                CreateNoWindow = executeInBackground,
                WorkingDirectory = asAdmin ? "" : workingDirectory,
            };

            if (asAdmin)
                startInfo.Verb = "runas"; // Runs process in admin mode. See https://stackoverflow.com/questions/2532769/how-to-start-a-process-as-administrator-mode-in-c-sharp

            var proc = new Process()
            {
                StartInfo = startInfo
            };

            using (proc)
            {
                var output = string.Empty;
                var errOut = string.Empty;

                proc.Start();

                if (!asAdmin && executeInBackground)
                {
                    output = proc.StandardOutput.ReadToEnd();
                    errOut = proc.StandardError.ReadToEnd();
                }

                proc.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    Debug.Log(output);

                if (!string.IsNullOrEmpty(errOut))
                    Debug.LogError(errOut);
            }
        }
    }

#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
    public static class Terminal {
        public static void Execute(string command, string workingDirectory = "", bool executeInBackground = false)
        {
            command = command.Replace("\"", "\"\"");

            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = executeInBackground,
                    RedirectStandardError = executeInBackground,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                }
            };

            using (proc)
            {
                var output = string.Empty;
                var errOut = string.Empty;

                proc.Start();

                if (executeInBackground)
                {
                    output = proc.StandardOutput.ReadToEnd();
                    errOut = proc.StandardError.ReadToEnd();
                }

                proc.WaitForExit();

                if (!string.IsNullOrEmpty(output))
                    Debug.Log(output);

                if (!string.IsNullOrEmpty(errOut))
                    Debug.LogError(errOut);
            }
        }

    }
#endif

    public static class Git
    {
        public static void Init(string localPath) =>
            Proc.Spawn("git.exe", new[] { "init" }, localPath);

        public static void Pull(string localPath, string remoteUrl) =>
            Proc.Spawn("git.exe", new[] { "pull", remoteUrl }, localPath);

        public static void Clone(string localPath, string remoteUrl) =>
            Proc.Spawn("git.exe", new[] { "clone", remoteUrl, localPath });

        public static void SubModule(string localPath, IEnumerable<string> args) =>
            Proc.Spawn("git.exe", new[] { "submodule" }.Concat(args), localPath);
    }

    public class GitClient
    {
        public string LocalPath { get; }
        public string RemoteUrl { get; }

        public GitClient(string localPath)
        {
            LocalPath = localPath;
        }

        public GitClient(string localPath, string remoteUrl)
        {
            LocalPath = localPath;
            RemoteUrl = remoteUrl;
        }

        public void Init() => Git.Init(LocalPath);
        public void Pull() => Git.Pull(LocalPath, RemoteUrl);
        public void Clone() => Git.Clone(LocalPath, RemoteUrl);
        public void SubModule(IEnumerable<string> args) => Git.SubModule(LocalPath, args);
    }
}
