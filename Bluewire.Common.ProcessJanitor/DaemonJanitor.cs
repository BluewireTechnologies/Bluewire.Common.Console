using Bluewire.Common.Console.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Bluewire.Common.ProcessJanitor
{
    public class DaemonJanitor
    {
        private readonly string janitorPath;

        public DaemonJanitor()
        {
            janitorPath = GetJanitorPath();
        }

        public DaemonJanitor(string applicationBinaryDirectory)
        {
            if (!Path.IsPathRooted(applicationBinaryDirectory)) throw new ArgumentException($"Not an absolute path: {applicationBinaryDirectory}", nameof(applicationBinaryDirectory));
            var fileName = Path.GetFileName(GetJanitorPath());
            janitorPath = Path.Combine(applicationBinaryDirectory, fileName);
            if (!File.Exists(janitorPath)) throw new FileNotFoundException($"Could not find the ProcessJanitor binary at the specified location: {janitorPath}");
        }

        public event Action<string> ErrorMessage = _ => { };

        public Task WatchAndTerminateOnExit(DaemonExeProcess daemon) =>
            WatchAndTerminateOnExit(daemon, null);

        public Task WatchAndTerminateOnExit(DaemonExeProcess daemon, ProcessDaemonExe.ShadowCopiedProcessDaemonExe shadowCopyScope)
        {
            if (shadowCopyScope != null)
            {
                if (shadowCopyScope.Daemon != daemon.Daemon) throw new ArgumentException("Shadow copy scope relates to a different daemon.", nameof(shadowCopyScope));
            }

            if (daemon.Process == null) throw new InvalidOperationException("Daemon process has not been started.");
            if (daemon.Process.HasExited) return Task.CompletedTask;

            var quotedArguments = CreateArguments(daemon, shadowCopyScope).Select(QuoteIfNecessary).ToArray();
            var info = new ProcessStartInfo(janitorPath, string.Join(" ", quotedArguments)) {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var waitForStart = new ManualResetEventSlim();
            var expectedText = "Watching " + daemon.Process.Id;

            var process = Process.Start(info);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null) return;
                if (e.Data != expectedText)
                {
                    ErrorMessage($"WARNING: Expected '{expectedText}', got '{e.Data}'");
                }
                waitForStart.Set();
            };
            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) ErrorMessage(e.Data);
            };

            // Wait for the janitor to start.
            if (waitForStart.Wait(5000))
            {
                ErrorMessage("WARNING: Janitor process did not start in the expected time.");
            }

            return WatchForExit(process);
        }

        private static Task<Process> WatchForExit(Process process)
        {
            process.EnableRaisingEvents = true;
            var tcs = new TaskCompletionSource<Process>();
            process.Exited += (s, e) => tcs.TrySetResult(process);
            process.Refresh();
            if (process.HasExited) tcs.TrySetResult(process);
            return tcs.Task;
        }

        private static IEnumerable<string> CreateArguments(DaemonExeProcess daemon, ProcessDaemonExe.ShadowCopiedProcessDaemonExe scope)
        {
            var thisProcessId = Process.GetCurrentProcess().Id;
            yield return "--parent-pid";
            yield return thisProcessId.ToString();

            yield return "--watch-pid";
            yield return daemon.Process.Id.ToString();

            yield return "--watch-exe";
            yield return daemon.Daemon.ApplicationSourceFile;

            if (scope != null)
            {
                yield return "--clean-directory";
                yield return scope.TemporaryContainer;
            }
        }

        private static readonly Regex rxNoQuotingRequired = new Regex(@"^[-\w\d/\\:\.]+$", RegexOptions.Compiled);
        private static string QuoteIfNecessary(string arg) => rxNoQuotingRequired.IsMatch(arg) ? arg : Quote(arg);
        private static string Quote(string arg) => $"\"{arg.Replace("\"", "\"\"")}\"";
        private static string GetJanitorPath() => typeof(Program).Assembly.Location;
    }
}
