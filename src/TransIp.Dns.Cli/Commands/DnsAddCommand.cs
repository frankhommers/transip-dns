using System.CommandLine;
using Apigen.TransIp.Client;
using Apigen.TransIp.Models;
using TransIp.Dns.Cli.Infrastructure;

namespace TransIp.Dns.Cli.Commands;

public static class DnsAddCommand
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
      Description = "Record name (e.g. www, @, subdomain)."
    };
    Option<string> typeOpt = new("--type")
    {
      Required = true,
      Description = "Record type (A, AAAA, CNAME, MX, TXT, ...)."
    };
    Option<string> contentOpt = new("--content")
    {
      Required = true,
      Description = "Record content (e.g. IP address, hostname, text value)."
    };
    Option<int> expireOpt = new("--expire")
    {
      DefaultValueFactory = _ => 300,
      Description = "TTL in seconds (default 300)"
    };

    Command cmd = new("add", "Add a DNS record.");
    cmd.Arguments.Add(domainArg);
    cmd.Options.Add(nameOpt);
    cmd.Options.Add(typeOpt);
    cmd.Options.Add(contentOpt);
    cmd.Options.Add(expireOpt);

    cmd.SetAction(async (parse, ct) =>
    {
      try
      {
        AuthOptions auth = AuthOptions.From(parse);
        using TransIpApiClient api = ClientFactory.Create(auth);

        string domain = parse.GetValue(domainArg)!;
        string name = parse.GetValue(nameOpt)!;
        string type = parse.GetValue(typeOpt)!;
        string content = parse.GetValue(contentOpt)!;
        int expire = parse.GetValue(expireOpt);

        DnsEntry entry = new()
        {
          Name = name,
          Type = type,
          Content = content,
          Expire = expire
        };

        await api.Domains.AddNewSingleDnsEntryDomainAsync(
          domain,
          new AddNewSingleDnsEntryDomainRequest { DnsEntry = entry },
          ct);

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