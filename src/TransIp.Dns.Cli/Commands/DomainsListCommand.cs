using System.CommandLine;
using System.Text.Json;
using Apigen.TransIp.Client;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DomainsListCommand
{
  public static Command Build()
  {
    Command cmd = new("domains", "List all domains on the account.");

    cmd.SetAction(async (parse, ct) =>
    {
      try
      {
        AuthOptions auth = AuthOptions.From(parse);
        using TransIpApiClient api = ClientFactory.Create(auth);

        JsonElement response = await api.Domains.ListAllDomainsAsync(
          new ListAllDomainsRequest(), ct);
        if (response.TryGetProperty("domains", out JsonElement domainsArray))
          foreach (JsonElement d in domainsArray.EnumerateArray())
            if (d.TryGetProperty("name", out JsonElement name))
              Console.WriteLine(name.GetString());

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