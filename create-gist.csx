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

    // Information to create a Gist 
    var files = new PropertySet<GistContent>
    {
        new ("file1.txt", new GistContent("text\ncontent", "text")),
        new ("file2.cs",  new GistContent("using System;\n\nConsole.WriteLine(\"Hello World.\");", "csharp")),
    };
    var lifetime = 10 * 24 * 60;  // 10 days [minutes]

    // Create Gist
    var created = await client.CreateGistAsync(new(files, "test gist", GistType.@public, lifetime: lifetime));
    Console.WriteLine($"Createt:");
    Console.WriteLine($"  Gist ID     : {created.gist_id}");
    Console.WriteLine($"  Type        : {created.type}");
    Console.WriteLine($"  Description : {created.description}");
    Console.WriteLine($"  URL         : {created.url}");
    var expires = (created.expires < 0) ? "permanent" : DateTime.UnixEpoch.AddSeconds(created.expires).ToString();
    Console.WriteLine($"  Expires     : {expires}");

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

}, o => o.AnyPause());
