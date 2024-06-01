using System.Security.Cryptography;

namespace FolderSync;

class HashFileComparison : IFileComparisonStrategy {

    private readonly MD5 md5;

    public HashFileComparison() {
        md5 = MD5.Create();
    }

    public bool Compare(string sourceFile, string targetFile) {
        if (!File.Exists(targetFile)) { return false; }
        return GetFileHash(sourceFile) == GetFileHash(targetFile);
    }

    private string GetFileHash(string filePath) {
        using FileStream stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}