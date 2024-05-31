namespace FolderSync;

using FileSynchronization;
using Logging;

class Program {
    static async Task Main(string[] args) {
        string sourcePath = args[0];
        string replicaPath = args[1];
        int interval = Int32.Parse(args[2]);
        string logPath = args[3];

        TimeSpan timeInterval = TimeSpan.FromSeconds(interval);

        Logger logger = new(logPath);

        logger.LogHeader([
            "# Log file for FolderSync program",
            $"# Date: {DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}",
            "# Version: 1.0",
            $"# Source Folder: {sourcePath}",
            $"# Replica Folder: {sourcePath}",
            $"# Synchronization Interval: {timeInterval}.{timeInterval.Milliseconds}",
        ]);

        if (!Directory.Exists(sourcePath)) {
            logger.LogMessage($"Could not find source folder path: '{sourcePath}'.", LogLabel.Error);
            return;
        }

        if (!Directory.Exists(logPath)) {
            logger.LogMessage($"Could not find source folder path: '{sourcePath}'.", LogLabel.Error);
            return;
        }

        IFileComparisonStrategy strategy = new TimeStampComparison();

        FolderSynchronizer synchronizer = new(sourcePath, replicaPath, strategy, logger);

        Console.CancelKeyPress += (sender, eventArgs) => {
            eventArgs.Cancel = true;
            synchronizer.StopSynchronization();
            logger.Close();
            Environment.Exit(0);
        };

        await synchronizer.StartSynchronization(timeInterval);
    }
}
