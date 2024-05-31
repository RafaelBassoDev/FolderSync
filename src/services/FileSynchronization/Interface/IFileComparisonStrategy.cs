namespace FolderSync;

interface IFileComparisonStrategy {
    bool Compare(string sourceFile, string targetFile);
}