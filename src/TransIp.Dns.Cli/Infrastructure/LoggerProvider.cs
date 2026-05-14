using Microsoft.Extensions.Logging;

namespace TransIp.Dns.Cli.Infrastructure;

internal static class LoggerProvider
{
  public static ILogger CreateLogger(bool verbose)
  {
    ILoggerFactory factory = LoggerFactory.Create(b =>
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
