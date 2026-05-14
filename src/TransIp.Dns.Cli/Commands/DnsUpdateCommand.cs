using System.CommandLine;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsUpdateCommand
{
    public static Command Build()
    {
        var domainArg = new Argument<string>("domain")
        {
            Description = "Domain name, e.g. example.nl"
        };
        var nameOpt = new Option<string>("--name")
        {
            Required = true,
            Description = "Current record name to match."
        };
        var typeOpt = new Option<string>("--type")
        {
            Required = true,
            Description = "Current record type to match."
        };
        var contentOpt = new Option<string>("--content")
        {
            Required = true,
            Description = "Current record content to match."
        };
        var expireOpt = new Option<int?>("--expire")
        {
            Description = "Match exact TTL (only needed for disambiguation)."
        };
        var newContentOpt = new Option<string?>("--new-content")
        {
            Description = "New content value."
        };
        var newExpireOpt = new Option<int?>("--new-expire")
        {
            Description = "New TTL in seconds."
        };

        var cmd = new Command("update", "Update a DNS record.");
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
                var newContent = parse.GetValue(newContentOpt);
                var newExpire = parse.GetValue(newExpireOpt);
                if (newContent is null && newExpire is null)
                    throw new InvalidOperationException(
                        "At least one of --new-content or --new-expire must be specified.");

                var auth = AuthOptions.From(parse);
                using var api = ClientFactory.Create(auth);

                var domain = parse.GetValue(domainArg)!;
                var name = parse.GetValue(nameOpt)!;
                var type = parse.GetValue(typeOpt)!;
                var content = parse.GetValue(contentOpt)!;
                var expire = parse.GetValue(expireOpt);

                var response = await api.Domains.ListAllDnsEntriesDomainAsync(domain, ct);
                var entries = DnsEntryReader.Read(response);

                var matches = entries.Where(e =>
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
                    foreach (var m in matches)
                        Console.Error.WriteLine(
                            $"  {m.Type}\t{m.Name}\t{(int)m.Expire}\t{m.Content}");
                    throw new InvalidOperationException("Add --expire to disambiguate.");
                }

                var match = matches[0];
                var updated = new DnsEntry
                {
                    Name = match.Name,
                    Type = match.Type,
                    Content = newContent ?? match.Content,
                    Expire = newExpire is int ne ? ne : match.Expire
                };

                await api.Domains.UpdateSingleDnsEntryAsync(
                    domain,
                    new UpdateSingleDnsEntryRequest { DnsEntry = updated },
                    ct);

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
