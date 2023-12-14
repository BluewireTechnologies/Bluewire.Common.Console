using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bluewire.Common.Console;
using Bluewire.Common.Console.ThirdParty;

namespace Bluewire.Common.ProcessJanitor
{
    internal class Program : IReceiveOptions
    {
        public static async Task<int> Main(string[] args)
        {
            var session = new ConsoleSession();
            var program = new Program();
            session.Options.AddCollector(program);
            return await session.RunAsync(args, () => program.Run());
        }

        private async Task<int> Run()
        {
            if (!IsValidProcessId(ParentPid))
            {
                System.Console.Error.WriteLine("No valid parent PID specified.");
                return 2;
            }
            if (!IsValidProcessId(WatchPid))
            {
                System.Console.Error.WriteLine("No valid watch PID specified.");
                return 2;
            }
            if (string.IsNullOrWhiteSpace(WatchExe))
            {
                System.Console.Error.WriteLine("No watch executable specified.");
                return 2;
            }
            string cleanDirectoryPath = null;
            if (CleanDirectory != null)
            {
                // Sanity check: require child of %TEMP%.
                // We don't want to wipe out system files etc by accident.
                var tempDirectoryInfo = new DirectoryInfo(Path.GetTempPath());
                var cleanDirectoryInfo = new DirectoryInfo(CleanDirectory);
                if (!FileSystemNative.IsDescendantDirectory(tempDirectoryInfo, cleanDirectoryInfo))
                {
                    System.Console.Error.WriteLine("Path specified for --clean-directory is not a descendant of the temp directory.");
                    System.Console.Error.WriteLine("Temporary: " + tempDirectoryInfo.FullName);
                    System.Console.Error.WriteLine("Specified: " + cleanDirectoryInfo.FullName);
                    return 2;
                }
                cleanDirectoryPath = cleanDirectoryInfo.FullName;
            }

            Process watchProcess = null;
            try
            {
                // Sanity check: require process executable path to match expectations.
                // Deal with case where the process terminated and its PID was reused.
                var watchExePath = Path.GetFullPath(WatchExe);
                watchProcess = Process.GetProcessById(WatchPid);
                var processExePath = Path.GetFullPath(watchProcess.MainModule?.FileName ?? "");
                if (!StringComparer.OrdinalIgnoreCase.Equals(processExePath, watchExePath))
                {
                    System.Console.Error.WriteLine("Executable specified for --watch-exe does not match the --watch-pid process.");
                    System.Console.Error.WriteLine("Specified executable: " + watchExePath);
                    System.Console.Error.WriteLine("Process executable:   " + processExePath);
                    return 2;
                }
            }
            catch (InvalidOperationException ex)
            {
                if (watchProcess?.HasExited == false)
                {
                    System.Console.Error.WriteLine("Unable to determine validity of watch process.");
                    System.Console.Error.WriteLine(ex);
                    return 2;
                }
                throw;
            }

            var parentProcess = Process.GetProcessById(ParentPid);

            // Notify the parent process that we're running.
            System.Console.Out.WriteLine("Watching " + WatchPid);

            // Wait for process to exit. Note that the Exited event cannot be relied upon for a process
            // we did not start ourselves.
            watchProcess.Refresh();
            while (!watchProcess.HasExited)
            {
                parentProcess.Refresh();
                if (parentProcess.HasExited)
                {
                    // When parent exits, terminate the watched process.
                    System.Console.Error.WriteLine("Parent has exited: " + ParentPid);
                    ShutDownProcess(watchProcess);
                    continue;
                }

                await Task.Delay(200);
                watchProcess.Refresh();
            }

            System.Console.Error.WriteLine("Watched process has exited: " + WatchPid);

            if (cleanDirectoryPath != null)
            {
                System.Console.Error.WriteLine("Cleaning: " + cleanDirectoryPath);
                try
                {
                    Directory.Delete(CleanDirectory, true);
                }
                catch (Exception ex)
                {
                    if (Directory.Exists(CleanDirectory))
                    {
                        System.Console.Error.WriteLine("Could not clean up: " + CleanDirectory);
                        System.Console.Error.WriteLine(ex);
                    }
                }
            }
            return 0;
        }

        public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(15);

        private void ShutDownProcess(Process process)
        {
            if (process == null) return;
            var sw = Stopwatch.StartNew();
            if (process.HasExited) return;
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                if (!process.HasExited) throw;
            }
            if (!process.WaitForExit((int)(ShutdownTimeout.TotalMilliseconds - sw.ElapsedMilliseconds)))
            {
                System.Console.Error.WriteLine($"Timed out while waiting for process {process.Id} to shut down.");
            }
        }

        private bool IsValidProcessId(int pid)
        {
            if (pid == 0) return false; // Idle
            if (pid == 4) return false; // System
            return true;
        }

        public void ReceiveFrom(OptionSet options)
        {
            options.Add("parent-pid=", (int x) => ParentPid = x);
            options.Add("watch-pid=", (int x) => WatchPid = x);
            options.Add("watch-exe=", x => WatchExe = x);
            options.Add("clean-directory=", x => CleanDirectory = x);
        }

        public string CleanDirectory { get; set; }
        public string WatchExe { get; set; }
        public int WatchPid { get; set; }
        public int ParentPid { get; set; }
    }
}
