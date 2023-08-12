#load ".common.csx"
#nullable enable
using System.Text.RegularExpressions;
using KallitheaApiClient;
using KallitheaApiClient.Utils;
using Lestaly;

var settings = new
{
    // Service URL for Kallithea.
    ServiceUrl = new Uri("http://localhost:9996"),

    // User list file for regist
    UserListFile = ThisSource.RelativeFile("regist-users-list.json"),
};

// user info for regist
record UserInfo(string LoginID, string FirstName, string LastName, string Mail, string Password);

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

    // Read the user information to be registered.
    var targets = await settings.UserListFile.ReadJsonAsync<UserInfo[]>() ?? throw new PavedMessageException("Failed to read user list.");

    // If not, create a parent group for the group for the user.
    var baseRepoGrpName = "users";
    var baseRepoGrpGroup = await Try.FuncOrDefaultAsync(async () => await client.GetRepoGroupInfoAsync(new(baseRepoGrpName)));
    if (baseRepoGrpGroup == null)
    {
        baseRepoGrpGroup = await client.CreateRepoGroupAsync(new(baseRepoGrpName));
    }

    // If API access is successful, scramble and save the API key.
    await info.SaveAsync(signal.Token);

    // Create listed users.
    foreach (var target in targets)
    {
        Console.WriteLine($"User: {target.LoginID}");
        try
        {
            // Create user.
            var user = await client.CreateUserAsync(new(target.LoginID, target.Mail, target.FirstName, target.LastName, target.Password));
            Console.WriteLine($"  User created.");

            // Create repository groups for user. User is the owner.
            var userRepoGrpName = $"{baseRepoGrpName}/{target.LoginID}";
            var userRepoGrp = await client.CreateRepoGroupAsync(new(target.LoginID, parent: baseRepoGrpName, owner: target.LoginID));
            Console.WriteLine($"  Repo group created. '{userRepoGrpName}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Error: {ex.Message}");
        }
        Console.WriteLine();
    }

});
