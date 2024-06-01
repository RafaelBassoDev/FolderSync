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
            try {
                if (!Directory.Exists(replicaFolderPath)) {
                    Directory.CreateDirectory(replicaFolderPath);
                    logger?.LogMessage($"Created folder '{replicaFolderPath}'.", label: LogLabel.Create);
                }
            } catch {
                logger?.LogMessage($"Could not create {replicaFolderPath}. Invalid or inaccessible path.", LogLabel.Error);
                return;
            }
            
            SynchronizeFiles(sourceFolderPath, replicaFolderPath);

            try {
                string[] sourceSubFolders = Directory.GetDirectories(sourceFolderPath, "*", SearchOption.AllDirectories);

                // If the directory is empty of files/folders, exit the current iteration.
                if (sourceSubFolders.Length == 0) { return; }

                foreach (string sourceSubFolderPath in sourceSubFolders) {
                    string sourceSubFolderName = Path.GetFileName(sourceSubFolderPath);
                    string replicaSubFolderPath = Path.Combine(replicaFolderPath, sourceSubFolderName);
                    SynchronizeFolders(sourceSubFolderPath, replicaSubFolderPath);
                }
            } catch {
                logger?.LogMessage($"Could not access sub folders from {sourceFolderPath}. Invalid or inaccessible path.", LogLabel.Error);
                return;
            }
        }

        /// <summary>
        /// Synchronizes all files from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        private void SynchronizeFiles(string sourceFolderPath, string replicaFolderPath) {
            try {
                string[] sourceFiles = Directory.GetFiles(sourceFolderPath, "*");
                
                foreach (string sourceFilePath in sourceFiles) {
                    string sourceFileName = Path.GetFileName(sourceFilePath);
                    string replicaFilePath = Path.Combine(replicaFolderPath, sourceFileName);

                    try {
                        // If both files are equal, continue to next iteration.
                        if (comparisonStrategy.Compare(sourceFilePath, replicaFilePath)) { continue; }

                        File.Copy(sourceFilePath, replicaFilePath, true);
                        logger?.LogMessage($"Copied file from '{sourceFilePath}' to '{replicaFilePath}'.", label: LogLabel.Copy);

                    } catch {
                        logger?.LogMessage($"Could not copy {sourceFilePath} to {replicaFilePath}. Invalid or inaccessible path.", LogLabel.Error);
                        continue;
                    }
                }
            } catch {
                logger?.LogMessage($"Could not access files from {sourceFolderPath}. Invalid or inaccessible path.", LogLabel.Error);
                return;
            }
        }

        /// <summary>
        /// Deletes all folders from <c>sourceFolderPath</c> that are not present in <c>targetFolderPath</c>.
        /// </summary>
        private void PruneFoldersFrom(string folderPath, string targetFolderPath) {
            PruneFilesFrom(folderPath, targetFolderPath);

            try {            
                string[] subFolders = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);

                foreach (string subFolderPath in subFolders) {
                    string subFolderName = Path.GetFileName(subFolderPath);
                    string targetSubFolderPath = Path.Combine(targetFolderPath, subFolderName);

                    try {
                        if (!Directory.Exists(targetSubFolderPath)) {
                            Directory.Delete(subFolderPath, true);
                            logger?.LogMessage($"Deleted folder '{subFolderPath}'.", label: LogLabel.Delete);
                            continue;
                        }
                    } catch {
                        logger?.LogMessage($"Could not delete {subFolderPath}. Invalid or inaccessible path.", LogLabel.Error);
                        continue;
                    }

                    PruneFoldersFrom(subFolderPath, targetSubFolderPath);
                }
            } catch {
                logger?.LogMessage($"Could not access files from {folderPath}. Invalid or inaccessible path.", LogLabel.Error);
                return;
            }
        }

        /// <summary>
        /// Deletes all files from <c>sourceFolderPath</c> that are not present in <c>targetFolderPath</c>.
        /// </summary>
        private void PruneFilesFrom(string folderPath, string targetFolderPath) {
            try {
                string[] sourceFiles = Directory.GetFiles(folderPath, "*");
                
                foreach (string filePath in sourceFiles) {
                    string fileName = Path.GetFileName(filePath);
                    string targetFilePath = Path.Combine(targetFolderPath, fileName);

                    if (File.Exists(targetFilePath)) { continue; }

                    try {
                        File.Delete(filePath);
                        logger?.LogMessage($"Deleted file '{filePath}'.", label: LogLabel.Delete);
                    } catch {
                        logger?.LogMessage($"Could not delete {filePath}. Invalid or inaccessible path.", LogLabel.Error);
                        continue;
                    }
                }
            } catch {
                logger?.LogMessage($"Could not access files from {folderPath}. Invalid or inaccessible path.", LogLabel.Error);
                return;
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
                    logger?.LogMessage("Starting synchronization.", LogLabel.Info);

                    SynchronizeFolders(sourceFolderPath, replicaFolderPath);
                    PruneFoldersFrom(replicaFolderPath, targetFolderPath: sourceFolderPath);

                    logger?.LogMessage("Synchronization completed.", LogLabel.Info);

                } catch (System.OperationCanceledException) {
                    logger?.LogMessage("Execution canceled by user.", LogLabel.Debug);

                } catch (Exception e) {
                    logger?.LogError(e, true);

                } finally {
                    await timer.WaitForNextTickAsync(cancellationTokenSource.Token);
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