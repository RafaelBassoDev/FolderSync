namespace FolderSync;

using FileSynchronization;
using Logging;

class Synchronizer {
    
    private string sourcePath = "";

    private string replicaPath = "";

    private TimeSpan interval;

    private string? logPath;

    /// <summary>
    /// Setup the current synchronizer with given <c>CommandLineOptions</c> properties.
    /// </summary>
    /// <param name="options">The setup options.</param>
    public void SetupWith(CommandLineOptions options) {
        sourcePath = options.Source;
        replicaPath = options.Replica;
        interval = TimeSpan.FromSeconds(options.Interval);
        logPath = options.Log;
    }

    /// <summary>
    /// <para>Starts the folder synchronization process, setting up the necessary services alltogether.</para>
    /// 
    /// Services:<br />
    /// - Starts the <c>Logger</c> service;<br />
    /// - Sets up a delegate to handle user initiated interruption (Ctrl+c or Crtl+Break);
    /// </summary>
    public async Task Start() {
            Logger? logger = new(logPath);

            if (sourcePath.Length == 0 || !Directory.Exists(sourcePath)) {
                logger?.LogMessage($"Could not file source folder path: {sourcePath}.", LogLabel.Error);
                return;
            }

            if (replicaPath.Length == 0) {
                logger?.LogMessage($"Could not create replica folder. Invalid name: {replicaPath}./", LogLabel.Error);
                return;
            }

            logger.LogHeader([
                "# Log file for FolderSync program",
                $"# Date: {DateTime.UtcNow.ToString("MM/dd/yyyy hh:mm:ss.fff tt")}",
                "# Version: 1.0",
                $"# Source Folder: {sourcePath}",
                $"# Replica Folder: {replicaPath}",
                $"# Synchronization Interval: {interval}.{interval.Milliseconds}",
            ]);

            IFileComparisonStrategy strategy = new TimeStampComparison();

            FolderSynchronizer synchronizer = new(sourcePath, replicaPath, strategy, logger);

            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                synchronizer.StopSynchronization();
                logger?.Close();
                Environment.Exit(0);
            };

            await synchronizer.StartSynchronization(interval);
        }
}