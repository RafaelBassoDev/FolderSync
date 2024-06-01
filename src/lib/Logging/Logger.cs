namespace Logging {
    public enum LogLabel {
        Copy,
        Create,
        Delete,
        Error,
        Info,
        Debug
    }

    public class Logger {
        private StreamWriter? streamWriter;

        public Logger(string? outputFilePath) {
            try {
                ArgumentException.ThrowIfNullOrEmpty(outputFilePath);

                streamWriter = new(outputFilePath, true) {
                    AutoFlush = true
                };

            } catch {
                LogMessage($"Invalid or inaccessible path: {outputFilePath}. Check if you have write permissions for log file or the path is correct. WARNING: The current execution will only be logged to the console, no file will contain the current execution logs.", LogLabel.Error);
            }
        }

        ~Logger() {
            Close();
        }

        private void Log(string message, bool includeTimeStamp = true) {
            string timeStamp = includeTimeStamp ? $"[UTC {DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}] " : "";
            streamWriter?.WriteLine(timeStamp + message);
            Console.WriteLine(timeStamp + message);
        }

        public void LogMessage(string message, LogLabel label = LogLabel.Info, bool includeTimeStamp = true) {
            string labelName = Enum.GetName(typeof(LogLabel), label) ?? "";
            Log($"{labelName.PadRight(6).ToUpper()} - {message}", includeTimeStamp);
        }

        public void LogError(Exception e, bool logStackTrace = false) {
            string stackTrace = logStackTrace ? $"\n{e.StackTrace ?? ""}" : "";
            LogMessage($"{e.GetType().FullName}: {e.Message}{stackTrace}", LogLabel.Error);
        }

        public void LogHeader(string[] messages) {
            LogEmptyLine();
            foreach (string message in messages) {
                Log(message, false);
            }
            LogEmptyLine();
        }

        public void LogEmptyLine() {
            streamWriter?.WriteLine();
            Console.WriteLine();
        }

        public void Close() {
            streamWriter?.Close();
            streamWriter = null;
        }
    }
}