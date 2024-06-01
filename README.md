# FolderSync

Folder Synchronizer is a C# console application designed to synchronize two folders, ensuring that the replica folder is a complete, up-to-date copy of the source folder. The application continuously monitors the source folder and updates the replica folder at regular intervals. It supports logging actions such as file copying and directory creation, providing a clear audit trail of synchronization activities.

## Features
- **Automatic Folder Synchronization**: Keeps the replica folder in sync with the source folder at specified intervals.
- **Logging**: The application logs its activities to a file and to the console. The log file includes metadata about the synchronization session and detailed entries for each action performed.
- **File Comparison Strategy**: Utilizes a customizable strategy to determine whether files need to be copied.
- **Interruption Handling**: The application listens for user interruption signals (Ctrl+C, Ctrl+Break) to allow for a proper shutdown. When interrupted, the application will cancel any ongoing synchronization tasks and execute cleanup code before exiting.

## Requirements
.NET SDK 6.0 or higher

## Dependencies
Command Line Parser: [Command Line Parser Library for CLR and NetStandard](https://github.com/commandlineparser/commandline)

# Getting Started
Clone the project to yout machine:
```console
git clone https://github.com/RafaelBassoDev/FolderSync.git
```

Build the project:
```console
dotnet build
```

## Usage
### Command-Line Arguments
The application accepts the following command-line arguments:

`--source`:  Required. The path to the source folder to be synchronized.<br />
`--replica`:  Required. The path to the replica folder where synchronization will occur.<br />
`-i, --interval`:  Required. The interval, in seconds, at which synchronization tasks will be performed.<br />
`--log`:  The path to store logs.<br />

### Example
This command synchronizes the contents of /path/to/source to /path/to/replica every 10 seconds.
```console
dotnet run --source /path/to/source/folder --replica /path/to/replica/folder -i 10 --log /path/to/log.txt
```
<br />

Without log output file:
```console
dotnet run --source /path/to/source/folder --replica /path/to/replica/folder -i 10
```
In this case, the log file is not created and the logs are only displayed on console.

### Example Log File
Here is an example of what the log file might look like:
```
# Log file for FolderSync program
# Date: 06/01/2024 11:50:01.909 AM
# Version: 1.0
# Source Folder: /path/to/original
# Replica Folder: /path/to/replica
# Synchronization Interval: 00:00:05.0

[UTC 06/01/2024 11:50:01.960 AM] DEBUG  - Execution started.
[UTC 06/01/2024 11:50:01.960 AM] INFO   - Starting synchronization.
[UTC 06/01/2024 11:50:01.961 AM] CREATE - Created folder '/path/to/replica'.
[UTC 06/01/2024 11:50:01.964 AM] COPY   - Copied file from '/path/to/original/a.txt' to '/path/to/replica/a.txt'.
[UTC 06/01/2024 11:50:01.964 AM] COPY   - Copied file from '/path/to/original/b.txt' to '/path/to/replica/b.txt'.
[UTC 06/01/2024 11:50:01.965 AM] CREATE - Created folder '/path/to/replica/dir'.
[UTC 06/01/2024 11:50:01.966 AM] COPY   - Copied file from '/path/to/original/dir/a.txt' to '/path/to/replica/dir/a.txt'.
[UTC 06/01/2024 11:50:01.966 AM] COPY   - Copied file from '/path/to/original/dir/b.txt' to '/path/to/replica/dir/b.txt'.
[UTC 06/01/2024 11:50:01.967 AM] INFO   - Synchronization completed.
[UTC 06/01/2024 11:50:06.942 AM] INFO   - Starting synchronization.
[UTC 06/01/2024 11:50:06.944 AM] INFO   - Synchronization completed.
[UTC 06/01/2024 11:51:00.672 AM] DEBUG  - Execution canceled by user.
```

## Contributing
Contributions are welcome! If you have suggestions for improvements or new features, please create an issue or submit a pull request.