using Apigen.TransIp.Client;
using Microsoft.Extensions.Logging;

namespace TransIp.Dns.Cli.Infrastructure;

public static class ClientFactory
{
  public static TransIpApiClient Create(AuthOptions opts)
  {
    string pem = File.ReadAllText(opts.PrivateKeyPath);
    ILogger logger = LoggerProvider.CreateLogger(opts.Verbose);

    return TransIpAuthTokenProvider.CreateClient(
      opts.Login,
      pem,
      opts.Label,
      opts.ReadOnly,
      opts.Expiration,
      opts.GlobalKey,
      logger: logger);
  }
}
