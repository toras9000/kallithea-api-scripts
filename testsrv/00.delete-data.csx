#r "nuget: Lestaly, 0.44.0"
#nullable enable
using System.Threading;
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Stop docker container and deletion of persistent data.

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var dataDir = baseDir.RelativeDirectory("data");

    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    Console.WriteLine("Stop service");
    await CmdProc.RunAsync("docker-compose", new[] { "--file", composeFile.FullName, "down", }).AsSuccessCode();

    Console.WriteLine("Delete config/repos");
    if (dataDir.Exists) { dataDir.DoFiles(c => c.File?.SetReadOnly(false)); dataDir.Delete(recursive: true); }
});
