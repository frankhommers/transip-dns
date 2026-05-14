using System.CommandLine;
using System.Text.Json;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DomainsListCommand
{
    public static Command Build()
    {
        var cmd = new Command("domains", "List all domains on the account.");

        cmd.SetAction(async (parse, ct) =>
        {
            try
            {
                var auth = AuthOptions.From(parse);
                using var api = ClientFactory.Create(auth);

                JsonElement response = await api.Domains.ListAllDomainsAsync(
                    new ListAllDomainsRequest(), ct);
                if (response.TryGetProperty("domains", out var domainsArray))
                {
                    foreach (var d in domainsArray.EnumerateArray())
                    {
                        if (d.TryGetProperty("name", out var name))
                            Console.WriteLine(name.GetString());
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                return ErrorHandler.Handle(ex, parse.GetValue(GlobalOptions.Verbose));
            }
        });

        return cmd;
    }
}
