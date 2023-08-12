#load ".common.csx"
#nullable enable
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

var settings = new
{
    // Service URL for Kallithea.
    ServiceUrl = new Uri("http://localhost:9996"),
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

    // Get user information.
    var user = await client.GetUserAsync();
    Console.WriteLine($"User: {user.user.username}");

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

    // Print a list of permissions to the repository.
    if (0 < user.permissions.repositories.Count)
    {
        Console.WriteLine($"  RepoPerms:");
        foreach (var repoPerm in user.permissions.repositories.OrderBy(e => e.name))
        {
            Console.WriteLine($"    {repoPerm.name}: {repoPerm.value.ToPermName()}");
        }
    }
    // Print a list of permissions to the repository group.
    if (0 < user.permissions.repositories_groups.Count)
    {
        Console.WriteLine($"  RepoGroupPerms:");
        foreach (var grpPerm in user.permissions.repositories_groups)
        {
            Console.WriteLine($"    {grpPerm.name}: {grpPerm.value.ToPermName()}");
        }
    }

});
