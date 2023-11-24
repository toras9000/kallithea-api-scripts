#r "nuget: Lestaly, 0.51.0"
#nullable enable
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// In kallithea's SQLite database, rewrite admin's API key to a fixed key for debugging.

return await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var composeFile = baseDir.RelativeFile("docker-compose.yml");

    Console.WriteLine("Rewrite api-key");
    await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "exec", "-it", "app", "su-exec", "kallithea:kallithea", "python3", "/kallithea/assets/rewrite-api-key.py" });
});
