using System.CommandLine;
using System.Text.Json;
using Apigen.TransIp.Client;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsUpdateCommand
{
  public static Command Build()
  {
    Argument<string> domainArg = new("domain")
    {
      Description = "Domain name, e.g. example.nl"
    };
    Option<string> nameOpt = new("--name")
    {
      Required = true,
      Description = "Current record name to match."
    };
    Option<string> typeOpt = new("--type")
    {
      Required = true,
      Description = "Current record type to match."
    };
    Option<string> contentOpt = new("--content")
    {
      Required = true,
      Description = "Current record content to match."
    };
    Option<int?> expireOpt = new("--expire")
    {
      Description = "Match exact TTL (only needed for disambiguation)."
    };
    Option<string?> newContentOpt = new("--new-content")
    {
      Description = "New content value."
    };
    Option<int?> newExpireOpt = new("--new-expire")
    {
      Description = "New TTL in seconds."
    };

    Command cmd = new("update", "Update a DNS record.");
    cmd.Arguments.Add(domainArg);
    cmd.Options.Add(nameOpt);
    cmd.Options.Add(typeOpt);
    cmd.Options.Add(contentOpt);
    cmd.Options.Add(expireOpt);
    cmd.Options.Add(newContentOpt);
    cmd.Options.Add(newExpireOpt);

    cmd.SetAction(async (parse, ct) =>
    {
      try
      {
        string? newContent = parse.GetValue(newContentOpt);
        int? newExpire = parse.GetValue(newExpireOpt);
        if (newContent is null && newExpire is null)
          throw new InvalidOperationException(
            "At least one of --new-content or --new-expire must be specified.");

        AuthOptions auth = AuthOptions.From(parse);
        using TransIpApiClient api = ClientFactory.Create(auth);

        string domain = parse.GetValue(domainArg)!;
        string name = parse.GetValue(nameOpt)!;
        string type = parse.GetValue(typeOpt)!;
        string content = parse.GetValue(contentOpt)!;
        int? expire = parse.GetValue(expireOpt);

        JsonElement response = await api.Domains.ListAllDnsEntriesDomainAsync(domain, ct);
        List<DnsEntry> entries = DnsEntryReader.Read(response);

        List<DnsEntry> matches = entries.Where(e =>
            e.Name == name &&
            string.Equals(e.Type, type, StringComparison.OrdinalIgnoreCase) &&
            e.Content == content &&
            (expire is null || (int)e.Expire == expire))
          .ToList();

        if (matches.Count == 0)
          throw new InvalidOperationException("No matching record.");
        if (matches.Count > 1)
        {
          Console.Error.WriteLine($"Ambiguous: {matches.Count} records match:");
          foreach (DnsEntry m in matches)
            Console.Error.WriteLine(
              $"  {m.Type}\t{m.Name}\t{(int)m.Expire}\t{m.Content}");
          throw new InvalidOperationException("Add --expire to disambiguate.");
        }

        DnsEntry match = matches[0];
        DnsEntry updated = new()
        {
          Name = match.Name,
          Type = match.Type,
          Content = newContent ?? match.Content,
          Expire = newExpire is int ne ? ne : match.Expire
        };

        bool ttlChanges = (int)updated.Expire != (int)match.Expire;

        if (ttlChanges)
        {
          // TransIP PATCH /domains/{name}/dns matches existing records on
          // (name, type, expire) and only mutates content; it cannot change
          // TTL. Fall back to remove+add. Brief window where the record is
          // absent; rolled back on failure.
          await api.Domains.RemoveDnsEntryDomainAsync(
            domain,
            new RemoveDnsEntryDomainRequest { DnsEntry = match },
            ct);
          try
          {
            await api.Domains.AddNewSingleDnsEntryDomainAsync(
              domain,
              new AddNewSingleDnsEntryDomainRequest { DnsEntry = updated },
              ct);
          }
          catch
          {
            await api.Domains.AddNewSingleDnsEntryDomainAsync(
              domain,
              new AddNewSingleDnsEntryDomainRequest { DnsEntry = match },
              CancellationToken.None);
            throw;
          }
        }
        else
        {
          await api.Domains.UpdateSingleDnsEntryAsync(
            domain,
            new UpdateSingleDnsEntryRequest { DnsEntry = updated },
            ct);
        }

        Console.WriteLine(
          $"Updated {updated.Type} {updated.Name} -> {updated.Content} TTL {(int)updated.Expire}");
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