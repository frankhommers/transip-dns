using System.CommandLine;
using TransIp.Dns.Cli.Commands;
using TransIp.Dns.Cli.Infrastructure;

var root = new RootCommand("TransIP DNS record CRUD.");
GlobalOptions.AttachTo(root);
root.Subcommands.Add(DomainsListCommand.Build());
root.Subcommands.Add(DnsListCommand.Build());
root.Subcommands.Add(DnsAddCommand.Build());
root.Subcommands.Add(DnsDeleteCommand.Build());

return await root.Parse(args).InvokeAsync();
