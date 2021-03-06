using System;
using System.IO;
using Infusion.Desktop.Profiles;
using Infusion.Logging;
using Infusion.Proxy;
using Infusion.Utilities;

namespace Infusion.Desktop
{
    internal sealed class FileLogger : ITimestampedLogger, IDisposable
    {
        private readonly Configuration configuration;
        private readonly CircuitBreaker loggingBreaker;
        private object logLock = new object();
        private bool firstWrite = true;
        private FileStream stream;
        private StreamWriter writer;

        public FileLogger(Configuration configuration, CircuitBreaker loggingBreaker)
        {
            this.configuration = configuration;
            this.loggingBreaker = loggingBreaker;
        }

        public void Dispose()
        {
            lock (logLock)
            {
                if (writer != null)
                {
                    writer.Dispose();
                    writer = null;
                }

                if (stream != null)
                {
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        private void WriteLine(DateTime timeStamp, string message)
        {
            if (!configuration.LogToFileEnabled)
                return;

            loggingBreaker.Protect(() =>
            {
                lock (logLock)
                {
                    string logsPath = configuration.LogPath;

                    string fileName = Path.Combine(logsPath, $"{timeStamp:yyyy-MM-dd}.log");

                    bool createdNew = false;
                    if (!File.Exists(fileName))
                    {
                        File.Create(fileName).Dispose();
                        createdNew = true;
                    }

                    if (stream == null || createdNew)
                    {
                        stream?.Dispose();
                        stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
                        writer = new StreamWriter(stream);
                    }

                    if (firstWrite || createdNew)
                    {
                        if (TimeZone.CurrentTimeZone != null)
                        {
                            var utcHoursDiff = TimeZone.CurrentTimeZone.GetUtcOffset(timeStamp).TotalHours;
                            var utcHoursDiffStr = utcHoursDiff >= 0 ? $"+{utcHoursDiff}" : $"-{utcHoursDiff}";
                            writer.WriteLine(
                                $"Log created on {timeStamp.Date:d}, using {TimeZone.CurrentTimeZone.StandardName} timezone (UTC {utcHoursDiffStr} h)");
                        }
                        else
                        {
                            writer.WriteLine(
                                $"Log created on {timeStamp.Date:d}, unknown timezone");
                        }

                        writer.WriteLine($@"Infusion {VersionHelpers.ProductVersion}");
                        firstWrite = false;
                    }
                    writer.WriteLine($@"{timeStamp:HH:mm:ss:fffff}: {message}");
                    writer.Flush();
                }
            });
        }

        public void Info(DateTime timeStamp, string message)
        {
            WriteLine(timeStamp, message);
        }

        public void Important(DateTime timeStamp, string message)
        {
            WriteLine(timeStamp, message);
        }

        public void Debug(DateTime timeStamp, string message)
        {
            WriteLine(timeStamp, message);
        }

        public void Critical(DateTime timeStamp, string message)
        {
            WriteLine(timeStamp, message);
        }

        public void Error(DateTime timeStamp, string message)
        {
            WriteLine(timeStamp, message);
        }
    }
}