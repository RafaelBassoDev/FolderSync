namespace Logging {
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
                Console.WriteLine($"<Logger>[{e.GetType().ToString()}]: failed to create output file on '{outputFileUrl}'");
            }
        }

        ~Logger() {
            Close();
        }

        private void Log(string message, bool includeTimeStamp = true) {
            streamWriter?.WriteLine($"-{(includeTimeStamp ? DateTime.UtcNow : "")}- {message}");
            Console.WriteLine($"-{(includeTimeStamp ? DateTime.UtcNow : "")}- {message}");
        }

        public void LogMessage(string message) {
            Log(message);
        }

        public void LogError(Exception e, string message = "") {
            Log($"{message}\n{e.Message}\n{e.StackTrace}");
        }

        public void Close() {
            streamWriter?.Close();
            streamWriter = null;
        }
    }
}