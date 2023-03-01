using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace Bluewire.Common.Console.Hosting
{
    /// <summary>
    /// Starts a daemon as a separate process and provides methods for managing it.
    /// </summary>
    public class DaemonExeProcess : IDisposable
    {
        private readonly ILog log;

        private readonly ProcessDaemonExe daemon;
        private Task<int> processedTask;

        private readonly object syncObject = new object();
        private bool disposing;
        private Process process;

        public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public DaemonExeProcess(ProcessDaemonExe daemon)
        {
            log = LogManager.GetLogger(daemon.AssemblyName.Name + ".Container");
            this.daemon = daemon;
        }

        public Task<int> Run(params string[] args)
        {
            lock (syncObject)
            {
                AssertNotDisposing();
                if (processedTask != null) throw new InvalidOperationException("The process has already been run.");

                processedTask = CreateProcess(args);
                processedTask.ContinueWith(RecordProcessTermination, TaskContinuationOptions.ExecuteSynchronously);
                return processedTask;
            }
        }

        private int RecordProcessTermination(Task<int> task)
        {
            if (task.IsFaulted)
            {
                log.Error("The process invocation failed.", task.Exception);
                return 255;
            }
            var exitCode = task.Result;
            if (exitCode == 0)
            {
                log.Debug("The running assembly terminated successfully.");
            }
            else if (disposing && exitCode == -1)
            {
                // Since we're killing the process we'll never get a clean exit.
                log.Debug("The running assembly terminated successfully.");
            }
            else
            {
                log.ErrorFormat("The running assembly terminated with exit code {0}", exitCode);
            }
            return exitCode;
        }

        private void ShutDownProcess(DateTimeOffset deadline)
        {
            if (process == null) return;
            StopProcess(process);
            if (!process.WaitForExit((int)Until(deadline).TotalMilliseconds))
            {
                log.Warn("Timed out while waiting for process to shut down.");
            }
        }

        public static DaemonExeProcess Run(ProcessDaemonExe daemon, params string[] args)
        {
            var container = new DaemonExeProcess(daemon);
            try
            {
                container.Run(args);
                return container;
            }
            catch
            {
                container.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            lock (syncObject)
            {
                disposing = true;
                var deadline = DateTimeOffset.Now + ShutdownTimeout;
                ShutDownProcess(deadline);
            }
        }

        private void StopProcess(Process process)
        {
            if (process.HasExited) return;
            try
            {
                process.Kill();
            }
            catch (InvalidOperationException)
            {
                if (!process.HasExited) throw;
            }
        }

        private static TimeSpan Until(DateTimeOffset deadline)
        {
            var duration = deadline - DateTimeOffset.Now;
            if (duration <= TimeSpan.Zero) return TimeSpan.Zero;
            return duration;
        }

        // Must only be called from inside the lock.
        private void AssertNotDisposing()
        {
            if (disposing) throw new ObjectDisposedException("DaemonContainer has been disposed.");
        }

        private Task<int> CreateProcess(string[] args)
        {
            // Must have the lock!

            AssertNotDisposing();
            Debug.Assert(process == null);

            daemon.OnBeforeStart();

            var startInfo = new ProcessStartInfo(daemon.ApplicationSourceFile, GetQuotedArguments(args))
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = daemon.ApplicationSourceDirectory,
            };
            var tcs = new TaskCompletionSource<int>();
            try
            {
                process = Process.Start(startInfo);
                Debug.Assert(process != null);
                process.EnableRaisingEvents = true;
                process.Exited += (s, e) => tcs.SetResult(process.ExitCode);
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        private static string GetQuotedArguments(string[] arguments) => String.Join(" ", arguments.Select(Quote));

        private static readonly Regex rxSimpleArgument = new Regex(@"^[-\w\d/\\:\.]+$", RegexOptions.Compiled);

        private static string Quote(string arg)
        {
            if (rxSimpleArgument.IsMatch(arg)) return arg;
            return $"\"{arg.Replace("\"", "\"\"")}\"";
        }
    }
}
