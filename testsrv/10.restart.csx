// This script is meant to run with dotnet-script.
// You can install .NET SDK 6.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

#r "nuget: Lestaly, 0.44.0"
using System.Threading;
using Lestaly;

// Restart docker container with deletion of persistent data.
// (If it is not activated, it is simply activated.)

await Paved.RunAsync(async () =>
{
    try
    {
        var composeFile = ThisSource.RelativeFile("./docker/docker-compose.yml");
        Console.WriteLine("Restart service");
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "down", "--remove-orphans", "--volumes", });
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "up", "-d", });

        Console.WriteLine("Waiting initialize");
        while (true)
        {
            await Task.Delay(1000);
            var check = await CmdProc.RunAsync("docker", new[] { "compose", "--file", composeFile.FullName, "exec", "app", "test", "-f", "/kallithea/config/kallithea.ini" });
            if (check.ExitCode == 0) break;
        }

        Console.WriteLine("Rewrite api-key");
        await CmdProc.CallAsync("docker", new[] { "compose", "--file", composeFile.FullName, "exec", "-it", "app", "su-exec", "kallithea:kallithea", "python3", "/kallithea/assets/rewrite-api-key.py" });

        Console.WriteLine("completed.");
    }
    catch (CmdProcExitCodeException err)
    {
        throw new PavedMessageException($"ExitCode: {err.ExitCode}\nOutput: {err.Output}", err);
    }
});
