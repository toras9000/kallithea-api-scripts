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

    // Get repositories information.
    var repos = await client.GetReposAsync();

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

    // Save to csv file.
    var saveFile = ThisSource.RelativeFile($"get-repos_{DateTime.Now:yyyyMMdd_Hhmmss}.csv");
    await repos
        .Select(r => new
        {
            Name = r.repo_name,
            Type = r.repo_type,
            Description = r.description,
            Owner = r.owner,
            LastCommited = r.last_changeset?.date,
        })
        .SaveToCsvAsync(saveFile);

}, o => o.AnyPause());
