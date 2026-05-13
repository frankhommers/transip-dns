using System.CommandLine;
using TransIp.Dns.Cli.Infrastructure;

var root = new RootCommand("TransIP DNS record CRUD.");
GlobalOptions.AttachTo(root);

return await root.Parse(args).InvokeAsync();
