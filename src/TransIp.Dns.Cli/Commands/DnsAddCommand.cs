using System.CommandLine;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsAddCommand
{
    public static Command Build()
    {
        var domainArg = new Argument<string>("domain")
        {
            Description = "Domain name, e.g. example.nl"
        };
        var nameOpt = new Option<string>("--name") { Required = true };
        var typeOpt = new Option<string>("--type") { Required = true };
        var contentOpt = new Option<string>("--content") { Required = true };
        var expireOpt = new Option<int>("--expire")
        {
            DefaultValueFactory = _ => 300,
            Description = "TTL in seconds (default 300)"
        };

        var cmd = new Command("add", "Add a DNS record.");
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

                var entry = new DnsEntry
                {
                    Name = name,
                    Type = type,
                    Content = content,
                    Expire = expire
                };

                await api.Domains.AddNewSingleDnsEntryDomainAsync(
                    domain,
                    new AddNewSingleDnsEntryDomainRequest { DnsEntry = entry });

                Console.WriteLine($"Added {type} {name} -> {content} TTL {expire}");
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
