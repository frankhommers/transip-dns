using Apigen.TransIp.Client;
using Microsoft.Extensions.Logging;

namespace TransIp.Dns.Cli.Infrastructure;

public static class ClientFactory
{
    public static TransIpApiClient Create(AuthOptions opts)
    {
        var pem = File.ReadAllText(opts.PrivateKeyPath);
        var logger = LoggerProvider.CreateLogger(opts.Verbose);

        return TransIpAuthTokenProvider.CreateClient(
            login: opts.Login,
            privateKeyPem: pem,
            label: opts.Label,
            readOnly: opts.ReadOnly,
            expirationTime: opts.Expiration,
            globalKey: opts.GlobalKey,
            logger: logger);
    }
}

internal static class LoggerProvider
{
    public static ILogger CreateLogger(bool verbose)
    {
        var factory = LoggerFactory.Create(b =>
        {
            b.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Warning);
            b.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "HH:mm:ss ";
            });
        });
        return factory.CreateLogger("TransIp");
    }
}
