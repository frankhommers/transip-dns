using System.CommandLine;
using System.Text.Json;
using Apigen.TransIp.Client;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsListCommand
{
  public static Command Build()
  {
    Argument<string> domainArg = new("domain")
    {
      Description = "Domain name, e.g. example.nl"
    };
    Option<string?> typeOption = new("--type")
    {
      Description = "Filter by record type (A, AAAA, CNAME, MX, TXT, ...)"
    };

    Command cmd = new("list", "List DNS records for a domain.");
    cmd.Arguments.Add(domainArg);
    cmd.Options.Add(typeOption);

    cmd.SetAction(async (parse, ct) =>
    {
      try
      {
        AuthOptions auth = AuthOptions.From(parse);
        using TransIpApiClient api = ClientFactory.Create(auth);
        string domain = parse.GetValue(domainArg)!;
        string? filter = parse.GetValue(typeOption);

        JsonElement response = await api.Domains.ListAllDnsEntriesDomainAsync(domain, ct);
        List<DnsEntry> entries = DnsEntryReader.Read(response);

        if (!string.IsNullOrWhiteSpace(filter))
          entries = entries
            .Where(e => string.Equals(
              e.Type, filter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        TableWriter.Write(
          new[] { "Name", "Type", "Expire", "Content" },
          entries.Select(e => (IReadOnlyList<string>)new[]
          {
            e.Name ?? "", e.Type ?? "",
            ((int)e.Expire).ToString(),
            e.Content ?? ""
          }));
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