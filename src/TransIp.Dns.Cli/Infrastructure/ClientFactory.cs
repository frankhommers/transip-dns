using Apigen.TransIp.Client;

namespace TransIp.Dns.Cli.Infrastructure;

public static class ClientFactory
{
    public static TransIpApiClient Create(AuthOptions opts)
    {
        var pem = File.ReadAllText(opts.PrivateKeyPath);
        return TransIpAuthTokenProvider.CreateClient(
            login: opts.Login,
            privateKeyPem: pem,
            label: opts.Label,
            readOnly: opts.ReadOnly,
            expirationTime: opts.Expiration);
    }
}
