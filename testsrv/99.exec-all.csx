#r "nuget: Lestaly, 0.44.0"
#nullable enable
using Lestaly;

// This script is meant to run with dotnet-script (v1.4 or lator).
// You can install .NET SDK 7.0 and install dotnet-script with the following command.
// $ dotnet tool install -g dotnet-script

return await Paved.RunAsync(async () =>
{
    static Task runScript(string name)
    {
        ConsoleWig.WriteLineColored(ConsoleColor.Green, name);
        return CmdProc.ExecAsync("dotnet", new[] { "script", ThisSource.RelativeFile(name).FullName, }, stdOut: Console.Out, stdErr: Console.Error).AsSuccessCode();
    }

    await runScript("00.delete-data.csx");
    await runScript("01.restart.csx");
    await runScript("02.rewrite-api-key-for-debug.csx");
});
