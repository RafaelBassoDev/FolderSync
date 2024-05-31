namespace Logging {
    public class Logger {
        private readonly StreamWriter? streamWriter;

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
            streamWriter?.Close();
        }

        private void Log(string message) {
            streamWriter?.WriteLine(message);
        }

        public void LogMessage(string message) {
            Log(message);
        }

        public void Close() {
            streamWriter?.Close();
        }
    }
}