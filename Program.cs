namespace FolderSync;

using System.IO;
using FileSynchronization;
using Logging;

class Program {
    static async Task Main(string[] args) {
        string dir = Directory.GetCurrentDirectory();
        string original = Path.Combine(dir, "origin");
        string replica = Path.Combine(dir, "replica");
        TimeSpan interval = TimeSpan.FromSeconds(10);

        string logFile = Path.Combine(dir, "log.txt");
        Logger logger = new(logFile);

        IFileComparisonStrategy strategy = new TimeStampComparison();

        FolderSynchronizer synchronizer = new(original, replica, strategy, logger);

        Console.CancelKeyPress += (sender, eventArgs) => {
            eventArgs.Cancel = true;
            synchronizer.StopSynchronization();
            logger.Close();
            Environment.Exit(0);
        };

        await synchronizer.StartSynchronization(TimeSpan.FromSeconds(5));
    }
}
