#r "nuget: Lestaly, 0.44.0"
#nullable enable
using System.Net.Http;
using System.Threading;
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

// Restart docker container.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    var baseDir = ThisSource.RelativeDirectory("./docker");
    var serviceUri = new Uri(@"http://localhost:9996/");

    Console.WriteLine("Restart service");
    var composeFile = baseDir.RelativeFile("docker-compose.yml");
    await CmdProc.RunAsync("docker-compose", new[] { "--file", composeFile.FullName, "down", }).AsSuccessCode();
    await CmdProc.RunAsync("docker-compose", new[] { "--file", composeFile.FullName, "up", "-d", }).AsSuccessCode();

    Console.WriteLine("Waiting initialize ... ");
    using var timer = new CancellationTokenSource(TimeSpan.FromSeconds(3 * 60));
    while (true)
    {
        var check = await CmdProc.RunAsync("docker", new[] { "compose", "--file", composeFile.FullName, "exec", "app", "test", "-f", "/kallithea/config/kallithea.ini" });
        if (check.ExitCode == 0) break;
        await Task.Delay(1000, timer.Token);
    }

    Console.WriteLine("Waiting for accessible ...");
    using var checker = new HttpClient();
    while (true) { try { await checker.GetAsync(serviceUri); break; } catch { await Task.Delay(1000); } }
});