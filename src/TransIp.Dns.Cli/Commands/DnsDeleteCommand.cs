using System.CommandLine;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsDeleteCommand
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
            Description = "Record name to match."
        };
        var typeOpt = new Option<string>("--type")
        {
            Required = true,
            Description = "Record type to match."
        };
        var contentOpt = new Option<string>("--content")
        {
            Required = true,
            Description = "Record content to match."
        };
        var expireOpt = new Option<int?>("--expire")
        {
            Description = "Match exact TTL (only needed for disambiguation)."
        };

        var cmd = new Command("delete", "Delete a DNS record.");
        cmd.Arguments.Add(domainArg);
        cmd.Options.Add(nameOpt);
        cmd.Options.Add(typeOpt);
        cmd.Options.Add(contentOpt);
        cmd.Options.Add(expireOpt);

        cmd.SetAction(async (parse, ct) =>
        {
            try
            {
                var auth = AuthOptions.Resolve(
                    parse.GetValue(GlobalOptions.Login),
                    parse.GetValue(GlobalOptions.KeyFile),
                    parse.GetValue(GlobalOptions.Label)!,
                    readOnly: false,
                    parse.GetValue(GlobalOptions.Expiration)!);
                using var api = ClientFactory.Create(auth);

                var domain = parse.GetValue(domainArg)!;
                var name = parse.GetValue(nameOpt)!;
                var type = parse.GetValue(typeOpt)!;
                var content = parse.GetValue(contentOpt)!;
                var expire = parse.GetValue(expireOpt);

                var response = await api.Domains.ListAllDnsEntriesDomainAsync(domain);
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
                await api.Domains.RemoveDnsEntryDomainAsync(
                    domain,
                    new RemoveDnsEntryDomainRequest { DnsEntry = match });

                Console.WriteLine(
                    $"Deleted {match.Type} {match.Name} -> {match.Content} TTL {(int)match.Expire}");
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
