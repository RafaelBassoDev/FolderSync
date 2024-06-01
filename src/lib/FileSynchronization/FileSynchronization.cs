using FolderSync;
using Logging;

namespace FileSynchronization {

    /// <summary>
    /// Instantiates a new folder synchronization service.
    /// </summary>
    class FolderSynchronizer(string sourcePath, string replicaPath, IFileComparisonStrategy comparisonStrategy, Logger? logger = null) {
        private readonly string sourceFolderPath = sourcePath;
        private readonly string replicaFolderPath = replicaPath;

        private readonly IFileComparisonStrategy comparisonStrategy = comparisonStrategy;
        private readonly Logger? logger = logger;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        /// <summary>
        /// Synchronizes all folders from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        public void SynchronizeFolders(string sourceFolderPath, string replicaFolderPath) {
            if (!Directory.Exists(replicaFolderPath)) {
                Directory.CreateDirectory(replicaFolderPath);
                logger?.LogMessage($"Created folder '{replicaFolderPath}'.", label: LogLabel.Create);
            }

            // If the directory is empty of files/folders, exit the current iteration.
            if (!Directory.EnumerateFileSystemEntries(sourceFolderPath).Any()) { return; }

            SynchronizeFiles(sourceFolderPath, replicaFolderPath);

            foreach (string sourceSubFolderPath in Directory.GetDirectories(sourceFolderPath, "*", SearchOption.AllDirectories)) {
                string sourceSubFolderName = Path.GetFileName(sourceSubFolderPath);
                string replicaSubFolderPath = Path.Combine(replicaFolderPath, sourceSubFolderName);
                SynchronizeFolders(sourceSubFolderPath, replicaSubFolderPath);
            }
        }

        /// <summary>
        /// Synchronizes all files from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        public void SynchronizeFiles(string sourceFolderPath, string replicaFolderPath) {
            foreach (string sourceFilePath in Directory.GetFiles(sourceFolderPath, "*")) {
                string sourceFileName = Path.GetFileName(sourceFilePath);
                string replicaFilePath = Path.Combine(replicaFolderPath, sourceFileName);

                // If both files are equal, continue to next iteration.
                if (comparisonStrategy.Compare(sourceFilePath, replicaFilePath)) { continue; }

                File.Copy(sourceFilePath, replicaFilePath, true);
                logger?.LogMessage($"Copied file from '{sourceFilePath}' to '{replicaFilePath}'.", label: LogLabel.Copy);
            }
        }

        /// <summary>
        /// Deletes all folders from <c>sourceFolderPath</c> that are not present in <c>targetFolderPath</c>.
        /// </summary>
        private void PruneFoldersFrom(string folderPath, string targetFolderPath) {
            PruneFilesFrom(folderPath, targetFolderPath);

            foreach (string subFolderPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories)) {
                string subFolderName = Path.GetFileName(subFolderPath);
                string targetSubFolderPath = Path.Combine(targetFolderPath, subFolderName);

                if (!Directory.Exists(targetSubFolderPath)) {
                    Directory.Delete(subFolderPath, true);
                    logger?.LogMessage($"Deleted folder '{subFolderPath}'.", label: LogLabel.Delete);
                    continue;
                }

                PruneFoldersFrom(subFolderPath, targetSubFolderPath);
            }
        }

        /// <summary>
        /// Deletes all files from <c>sourceFolderPath</c> that are not present in <c>targetFolderPath</c>.
        /// </summary>
        private void PruneFilesFrom(string folderPath, string targetFolderPath) {
            foreach (string filePath in Directory.GetFiles(folderPath, "*")) {
                string fileName = Path.GetFileName(filePath);
                string targetFilePath = Path.Combine(targetFolderPath, fileName);

                if (File.Exists(targetFilePath)) { continue; }

                File.Delete(filePath);
                logger?.LogMessage($"Deleted file '{filePath}'.", label: LogLabel.Delete);
            }
        }

        /// <summary>
        /// Starts the synchronization process with a specified interval.
        /// </summary>
        /// <param name="interval">The synchronization interval in seconds.</param>
        public async Task StartSynchronization(TimeSpan interval) {
            PeriodicTimer timer = new (interval);

            logger?.LogMessage("Execution started.", LogLabel.Debug);

            while (!cancellationTokenSource.IsCancellationRequested) {
                try {
                    logger?.LogMessage("Synchronization began.", LogLabel.Info);

                    SynchronizeFolders(sourceFolderPath, replicaFolderPath);
                    PruneFoldersFrom(replicaFolderPath, targetFolderPath: sourceFolderPath);

                    logger?.LogMessage("Synchronization completed.", LogLabel.Info);

                    await timer.WaitForNextTickAsync(cancellationTokenSource.Token);

                } catch (System.OperationCanceledException) {
                    logger?.LogMessage("Execution canceled by user.", LogLabel.Debug);

                } catch (Exception e) {
                    logger?.LogError(e);
                }
            }

            timer.Dispose();
        }

        /// <summary>
        /// Stops the synchronization process.
        /// </summary>
        public void StopSynchronization() {
            cancellationTokenSource.Cancel();
        }
    }
}