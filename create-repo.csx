#load ".common.csx"
#nullable enable
using System.Text.RegularExpressions;
using System.Threading;
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

// Create a repository group along with adding users to create a simple dedicated area.
// Users to be registered are defined in a csv file.

var settings = new
{
    // Service URL for Kallithea.
    ServiceUrl = new Uri("http://localhost:9996"),

    // The Kallithea repository group where the repository was created. 
    KallitheaPlace = "",

    // User permission for created repo
    PermitUsers = new[]
    {
        new { name = "alpha", perm = RepoPerm.write, },
        new { name = "beta",  perm = RepoPerm.write, },
    },

    // Location to clone the repository.
    LocalPlace = ThisSource.RelativeDirectory("repos"),
};

// main processing
await Paved.RunAsync(configuration: o => o.AnyPause(), action: async () =>
{
    // Prepare console
    using var outenc = ConsoleWig.OutputEncodingPeriod(Encoding.UTF8);
    using var signal = ConsoleWig.CreateCancelKeyHandlePeriod();

    // Show access address
    Console.WriteLine($"Service URL : {settings.ServiceUrl}");

    // Attempt to recover saved API key information.
    var info = await ApiKeyStore.RestoreAsync(new(settings.ServiceUrl, "_admin/api"), signal.Token);

    // Create client
    using var client = new SimpleKallitheaClient(info.ApiEntry, info.Key.Token);

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

    // Ask the user to enter the name of the repository to be created.
    ConsoleWig.WriteLine($"Create a repository in '{settings.KallitheaPlace.WhenWhite("top level")}'");
    var repoName = ConsoleWig.Write("Enter the name of the repository to be created.\n>").ReadLine().CancelIfWhite();

    // Create a repository.
    ConsoleWig.WriteLine($"Create repository");
    var repoPath = settings.KallitheaPlace.Mux(repoName, "/");
    await client.CreateRepoAsync(new(repoPath, repo_type: RepoType.git), signal.Token);

    // Grant user group permission
    ConsoleWig.WriteLine($"Grant user group permission");
    foreach (var user in settings.PermitUsers)
    {
        await client.GrantUserPermToRepoAsync(new(repoPath, user.name, user.perm), signal.Token);
    }

    // Clone the repository.
    ConsoleWig.WriteLine($"Clone the repository");
    var cloneDir = settings.LocalPlace.RelativeDirectory(repoName);
    var repoUri = new Uri(settings.ServiceUrl, repoPath);
    await CmdProc.RunAsync("git", new[] { "clone", repoUri.AbsoluteUri, cloneDir.FullName, });

});
