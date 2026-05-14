using System.CommandLine;
using System.Globalization;
using TransIp.Dns.Cli.Commands;
using TransIp.Dns.Cli.Infrastructure;

Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

RootCommand root = new("TransIP DNS record CRUD.");
GlobalOptions.AttachTo(root);
root.Subcommands.Add(DomainsListCommand.Build());
root.Subcommands.Add(DnsListCommand.Build());
root.Subcommands.Add(DnsAddCommand.Build());
root.Subcommands.Add(DnsDeleteCommand.Build());
root.Subcommands.Add(DnsUpdateCommand.Build());

return await root.Parse(args).InvokeAsync();
