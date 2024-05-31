namespace FolderSync;

interface IFileComparisonStrategy {
    /// <summary>
    /// Compares two files and checks wether they are considered equal.
    /// </summary>
    /// <param name="sourceFile"> the source file.</param>
    /// <param name="targetFile"> the file to be compared.</param>
    /// <returns>Returns <c>true</c> if the files are equal, otherwise returns <c>false</c>.</returns>
    bool Compare(string sourceFile, string targetFile);
}