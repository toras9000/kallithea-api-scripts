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
};

// API Access Information
record ApiAccessInfo(string Entry, string Key);

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

    // Obtaining user information via API.
    Console.WriteLine($"Get users info.");
    var users = await client.GetUsersAsync(signal.Token);

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

    // Saving options. Exclude some properties.
    var options = new SaveToCsvOptions();
    options.MemberFilter = m => m.Name switch { nameof(UserInfo.emails) => false, _ => true };

    // Save to csv.
    Console.WriteLine($"Save to csv.");
    var usersFile = ThisSource.RelativeFile($"users_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    await users.SaveToCsvAsync(usersFile, options);
});
