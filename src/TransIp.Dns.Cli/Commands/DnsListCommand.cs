using System.CommandLine;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsListCommand
{
    public static Command Build()
    {
        var domainArg = new Argument<string>("domain")
        {
            Description = "Domain name, e.g. example.nl"
        };
        var typeOption = new Option<string?>("--type")
        {
            Description = "Filter by record type (A, AAAA, CNAME, MX, TXT, ...)"
        };

        var cmd = new Command("list", "List DNS records for a domain.");
        cmd.Arguments.Add(domainArg);
        cmd.Options.Add(typeOption);

        cmd.SetAction(async (parse, ct) =>
        {
            try
            {
                var auth = AuthOptions.From(parse);
                using var api = ClientFactory.Create(auth);
                var domain = parse.GetValue(domainArg)!;
                var filter = parse.GetValue(typeOption);

                var response = await api.Domains.ListAllDnsEntriesDomainAsync(domain, ct);
                var entries = DnsEntryReader.Read(response);

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
