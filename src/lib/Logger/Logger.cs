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

        public Logger(string outputFileUrl) {
            try {
                if (!File.Exists(outputFileUrl)) {
                    File.Create(outputFileUrl);
                }

                this.streamWriter = new StreamWriter(outputFileUrl, true) {
                    AutoFlush = true
                };

            } catch (Exception e) {
                LogError(e, $"Failed to create output file on '{outputFileUrl}.'");
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
            Log($"{labelName.PadLeft(6).ToUpper()} - {message}", includeTimeStamp);
        }

        public void LogError(Exception e, string message = "") {
            LogMessage($"{message} {e.GetType().FullName}: {e.Message}\n{e.StackTrace}", LogLabel.Error);
        }

        public void Close() {
            streamWriter?.Close();
            streamWriter = null;
        }
    }
}