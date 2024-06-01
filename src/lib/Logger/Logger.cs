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
                if (outputFilePath == null) {
                    throw new NullReferenceException();
                }

                this.streamWriter = new(outputFilePath, true) {
                    AutoFlush = true
                };

            } catch (Exception e) {
                LogError(e, $"Failed to create output file on '{outputFilePath}.'");
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

        public void LogError(Exception e, string message = "") {
            LogMessage($"{message} {e.GetType().FullName}: {e.Message}\n{e.StackTrace}", LogLabel.Error);
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