namespace FolderSync;

using CommandLine;

class Program {
    static async Task Main(string[] args) {
        Synchronizer synchronizer = new();

        var parsedOptions = Parser.Default.ParseArguments<CommandLineOptions>(args)
            .WithParsed(synchronizer.SetupWith);

        await synchronizer.Start();
    }
}
