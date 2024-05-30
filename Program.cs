namespace FolderSync;

using System;
using System.IO;

class Program {
    static void Main(string[] args) {
        string dir = Directory.GetCurrentDirectory();
        string original = Path.Combine(dir, "origin");
        string replica = Path.Combine(dir, "replica");
        // double interval = 1;

        SynchronizeFolder(original, replica);
    }

    static void SynchronizeFolder(string originalFolderURL, string replicaFolderURL) {

        if (!Directory.Exists(replicaFolderURL)){
            Directory.CreateDirectory(replicaFolderURL);
        }

        foreach (string originalFileURL in Directory.GetFiles(originalFolderURL)) {
            // if files are equal
            // check if have permissions
            // if (true) { continue; }

            string originalFileName = Path.GetFileName(originalFileURL);
            string replicaFileURL = Path.Combine(replicaFolderURL, originalFileName);

            // if files are different
            
            if (File.Exists(replicaFileURL)) {
                File.Copy(originalFileURL, replicaFileURL, true);
                Console.WriteLine("<copy> <timestamp> " + originalFileURL);
                continue;
            }

            File.Create(replicaFileURL);
            Console.WriteLine("<create> <timestamp> " + originalFileURL);
        }

        foreach (string subDirectoryURL in Directory.GetDirectories(originalFolderURL)) {
            string originalDirectoryName = Path.GetFileName(subDirectoryURL);
            string replicaDirectoryURL = Path.Combine(replicaFolderURL, originalDirectoryName);
            SynchronizeFolder(subDirectoryURL, replicaDirectoryURL);
        }
    }

    static void PruneFiles() {
        //
    }
}
