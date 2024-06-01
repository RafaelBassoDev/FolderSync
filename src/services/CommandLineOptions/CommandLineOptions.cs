namespace FolderSync;

using CommandLine;
using CommandLine.Text;

class CommandLineOptions {

    [Option("source", Required = true, HelpText = "Source folder path.")]
    public required string Source { get; set; }
    
    [Option("replica", Required = true, HelpText = "Replica folder path.")]
    public required string Replica { get; set; }

    [Option('i', "interval", Required = true, HelpText = "Synchronization interval in seconds.")]
    public int Interval { get; set; }

    [Option("log", Required = false, HelpText = "Log output file path.", Default = null)]
    public required string? Log { get; set; }

    [Usage(ApplicationAlias = "dotnet run")]
    public static IEnumerable<Example> ExecutableExample {
        get {
            return [
                new("Synchronize folders periodically", new CommandLineOptions { Source = "/path/to/source/folder", Replica = "/path/to/replica/folder", Interval = 5, Log = "/path/to/log.txt" })
            ];
        }
    }
}