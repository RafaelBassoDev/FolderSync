namespace FolderSync;

class TimeStampComparison : IFileComparisonStrategy {
    public bool Compare(string sourceFile, string targetFile) {
        return File.GetLastWriteTime(sourceFile) == File.GetLastWriteTime(targetFile);
    }
}