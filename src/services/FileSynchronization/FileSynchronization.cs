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

        /// <summary>
        /// Synchronizes all the files and folders from <c>sourceFolderPath</c> to <c>replicaFolderPath</c>.
        /// </summary>
        public void SynchronizeFolders(string sourceFolderPath, string replicaFolderPath) {

            if (!Directory.Exists(replicaFolderPath)){
                Directory.CreateDirectory(replicaFolderPath);
                logger?.LogMessage("<create> <timestamp> " + replicaFolderPath);
            }

            foreach (string sourceFilePath in Directory.GetFiles(sourceFolderPath)) {
                string sourceFileName = Path.GetFileName(sourceFilePath);
                string replicaFilePath = Path.Combine(replicaFolderPath, sourceFileName);

                // If both files are equal, continue to next iteration.
                if (comparisonStrategy.Compare(sourceFilePath, replicaFilePath)) { continue; }

                File.Copy(sourceFilePath, replicaFilePath, true);
                logger?.LogMessage("<copy> <timestamp> " + sourceFilePath);
            }

            foreach (string subFolderPath in Directory.GetDirectories(sourceFolderPath)) {
                string sourceDirectoryName = Path.GetFileName(subFolderPath);
                string replicaDirectoryPath = Path.Combine(replicaFolderPath, sourceDirectoryName);
                SynchronizeFolders(subFolderPath, replicaDirectoryPath);
            }
        }

        /// <summary>
        /// Starts the synchronization process with a specified interval.
        /// </summary>
        /// <param name="interval">The synchronization interval in seconds.</param>
        public void StartSynchronization(int interval) {
            SynchronizeFolders(sourceFolderPath, replicaFolderPath);
            Console.WriteLine($"starting synchronization with interval: {interval}");
        }

        /// <summary>
        /// Stops the synchronization process.
        /// </summary>
        public void StopSynchronization() {
            Console.WriteLine("stopping synchronization");
        }
    }
}