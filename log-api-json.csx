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

await Paved.RunAsync(async () =>
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

    // Handle API log events and record them in the log file.
    using var logWriter = ThisSource.RelativeFile($"log-api-json_{DateTime.Now:yyyyMMdd_Hhmmss}.txt").CreateTextWriter();
    client.Logging += log => logWriter.WriteLine($"Request:\n  {log.Request}\nResponse:(result={log.Status})\n  {log.Response}\n");

    // Make some kind of API call.
    var user = await client.GetUserAsync();
    var repos = await client.GetReposAsync();
    foreach (var repo in repos.Take(3))   // only some repos
    {
        Console.WriteLine($"Repository: {repo.repo_name}");
        try
        {
            // Get repository changesets.
            var changesets = await client.GetChangesetsAsync(new($"{repo.repo_id}", max_revisions: "5", reverse: true));
            foreach (var change in changesets)
            {
                Console.WriteLine($"  {change.summary.short_id} : {change.summary.message?.FirstLine()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
    }

}, o => o.AnyPause());
