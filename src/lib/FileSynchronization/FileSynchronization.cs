using FolderSync;
using Logging;

namespace FileSynchronization {

    /// <summary>
    /// Instantiates a new folder synchronization service.
    /// </summary>
    class FolderSynchronizer {
        private readonly string sourceFolderPath;
        private readonly string replicaFolderPath;
        private readonly IFileComparisonStrategy comparisonStrategy;
        private readonly Logger? logger;
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public FolderSynchronizer(string sourcePath, string replicaPath, IFileComparisonStrategy comparisonStrategy, Logger? logger = null) {
            sourceFolderPath = sourcePath;
            replicaFolderPath = replicaPath;
            this.comparisonStrategy = comparisonStrategy;
            this.logger = logger;
        }

        /// <summary>
        /// Synchronizes all folders from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        public void SynchronizeFolders(string sourceFolder, string replicaFolder) {
            try {
                if (!Directory.Exists(replicaFolder)) {
                    Directory.CreateDirectory(replicaFolder);
                    logger?.LogMessage($"Created folder '{replicaFolder}'.", LogLabel.Create);
                }
            } catch {
                logger?.LogMessage($"Could not create {replicaFolder}. Invalid or inaccessible path.", LogLabel.Error);
                return;
            }

            SynchronizeFiles(sourceFolder, replicaFolder);
            SynchronizeSubFolders(sourceFolder, replicaFolder);
        }

        /// <summary>
        /// Synchronizes subfolders recursively from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        private void SynchronizeSubFolders(string sourceFolder, string replicaFolder) {
            try {
                foreach (string sourceSubFolder in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories)) {
                    string sourceSubFolderName = Path.GetFileName(sourceSubFolder);
                    string replicaSubFolder = Path.Combine(replicaFolder, sourceSubFolderName);
                    SynchronizeFolders(sourceSubFolder, replicaSubFolder);
                }
            } catch {
                logger?.LogMessage($"Could not access sub folders from {sourceFolder}. Invalid or inaccessible path.", LogLabel.Error);
            }
        }

        /// <summary>
        /// Synchronizes all files from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        private void SynchronizeFiles(string sourceFolder, string replicaFolder) {
            try {
                foreach (string sourceFile in Directory.GetFiles(sourceFolder, "*")) {
                    string sourceFileName = Path.GetFileName(sourceFile);
                    string replicaFile = Path.Combine(replicaFolder, sourceFileName);

                    try {
                        // If both files are equal, continue to next iteration.
                        if (comparisonStrategy.Compare(sourceFile, replicaFile)) { continue; }

                        File.Copy(sourceFile, replicaFile, true);
                        logger?.LogMessage($"Copied file from '{sourceFile}' to '{replicaFile}'.", label: LogLabel.Copy);

                    } catch {
                        logger?.LogMessage($"Could not copy {sourceFile} to {replicaFile}. Invalid or inaccessible path.", LogLabel.Error);
                    }
                }
            } catch {
                logger?.LogMessage($"Could not access files from {sourceFolder}. Invalid or inaccessible path.", LogLabel.Error);
            }
        }

        /// <summary>
        /// Deletes all folders from <c>folder</c> that are not present in <c>referenceFolder</c>.
        /// </summary>
        private void PruneFoldersFrom(string folder, string referenceFolder) {
            PruneFilesFrom(folder, referenceFolder);

            try {            
                foreach (string referenceSubFolder in Directory.GetDirectories(referenceFolder, "*", SearchOption.AllDirectories)) {
                    string referenceSubFolderName = Path.GetFileName(referenceSubFolder);
                    string subFolder = Path.Combine(folder, referenceSubFolderName);

                    try {
                        if (!Directory.Exists(subFolder)) {
                            Directory.Delete(subFolder, true);
                            logger?.LogMessage($"Deleted folder '{subFolder}'.", label: LogLabel.Delete);

                        } else {
                            PruneFoldersFrom(subFolder, referenceSubFolder);
                        }

                    } catch {
                        logger?.LogMessage($"Could not delete {subFolder}. Invalid or inaccessible path.", LogLabel.Error);
                    }
                }
            } catch {
                logger?.LogMessage($"Could not access files from {folder}. Invalid or inaccessible path.", LogLabel.Error);
            }
        }

        /// <summary>
        /// Deletes all files from <c>sourceFolderPath</c> that are not present in <c>targetFolderPath</c>.
        /// </summary>
        private void PruneFilesFrom(string folder, string referenceFolderPath) {
            try {
                foreach (string file in Directory.GetFiles(folder, "*")) {
                    string fileName = Path.GetFileName(file);
                    string referenceFile = Path.Combine(referenceFolderPath, fileName);

                    try {
                        if (!File.Exists(referenceFile)) {
                            File.Delete(file);
                            logger?.LogMessage($"Deleted file '{file}'.", label: LogLabel.Delete);
                        }
                    } catch {
                        logger?.LogMessage($"Could not delete {file}. Invalid or inaccessible path.", LogLabel.Error);
                    }
                }
            } catch {
                logger?.LogMessage($"Could not access files from {folder}. Invalid or inaccessible path.", LogLabel.Error);
            }
        }

        /// <summary>
        /// Starts the synchronization process with a specified interval.
        /// </summary>
        /// <param name="interval">The synchronization interval in seconds.</param>
        public async Task StartSynchronization(TimeSpan interval) {
            using PeriodicTimer timer = new (interval);

            logger?.LogMessage("Execution started.", LogLabel.Debug);

            while (!cancellationTokenSource.IsCancellationRequested) {
                try {
                    logger?.LogMessage("Starting synchronization.", LogLabel.Info);

                    SynchronizeFolders(sourceFolderPath, replicaFolderPath);
                    PruneFoldersFrom(replicaFolderPath, referenceFolder: sourceFolderPath);

                    logger?.LogMessage("Synchronization completed.", LogLabel.Info);

                } catch (System.OperationCanceledException) {
                    logger?.LogMessage("Execution canceled by user.", LogLabel.Debug);

                } catch (Exception e) {
                    logger?.LogError(e, true);

                } finally {
                    await timer.WaitForNextTickAsync(cancellationTokenSource.Token);
                }
            }

            logger?.LogMessage("Synchronization stopped.", LogLabel.Debug);
        }

        /// <summary>
        /// Stops the synchronization process.
        /// </summary>
        public void StopSynchronization() {
            cancellationTokenSource.Cancel();
        }
    }
}