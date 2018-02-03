using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bluewire.Common.Console.Daemons;
using Bluewire.Common.Console.Util;
using log4net;

namespace Bluewire.Common.Console.Environment
{
    public class RedirectConsoleToFiles
    {
        public IDisposable RedirectTo(string logPath, string logNameRoot)
        {
            if (string.IsNullOrWhiteSpace(logPath)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(logPath));
            if (string.IsNullOrWhiteSpace(logNameRoot)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(logNameRoot));
            if (!PathValidator.IsValidFileName(logNameRoot.TrimEnd('.'))) throw new ArgumentException($"Value must be a valid file name: {logNameRoot}", nameof(logNameRoot));
            if (!PathValidator.IsValidPath(logPath)) throw new ArgumentException($"Value must be a valid path: {logPath}", nameof(logPath));
            if (!Path.IsPathRooted(logPath)) throw new ArgumentException($"Not an absolute path: {logPath}", nameof(logPath));

            var stdOutFilePath = Path.GetFullPath(Path.Combine(logPath, $"{logNameRoot.TrimEnd('.')}.stdout"));
            var stdErrFilePath = Path.GetFullPath(Path.Combine(logPath, $"{logNameRoot.TrimEnd('.')}.stderr"));

            var stdout = new LazyFileWriter(stdOutFilePath);
            var stderr = new LazyFileWriter(stdErrFilePath);

            return new RedirectConsoleScope(stdout, stderr);
        }

        class LazyFileWriter : TextWriter
        {
            private readonly string filePath;
            private TextWriter writer;

            public LazyFileWriter(string filePath)
            {
                if (!PathValidator.IsValidPath(filePath)) throw new ArgumentException($"Value must be a valid path: {filePath}", nameof(filePath));
                this.filePath = filePath;
            }

            private TextWriter GetWriter()
            {
                if (writer == null)
                {
                    writer = OpenWriter(true);
                }
                else if (writer == Null)
                {
                    writer = OpenWriter(false);
                }
                return writer;
            }

            private TextWriter OpenWriter(bool firstTime)
            {
                try
                {
                    return File.AppendText(filePath);
                }
                catch (Exception ex)
                {
                    if (firstTime) LogManager.GetLogger(typeof(RedirectConsoleToFiles)).Error($"Unable to open console log file: {filePath}", ex);
                    return Null;
                }
            }

            public override void Flush() => writer?.Flush();
            public override Task FlushAsync() => writer?.FlushAsync() ?? TaskHelpers.CompletedTask;
            public override void Write(char value) => GetWriter().Write(value);
            public override void Write(string value) => GetWriter().Write(value);
            public override void Write(char[] buffer, int index, int count) => GetWriter().Write(buffer, index, count);

            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}
