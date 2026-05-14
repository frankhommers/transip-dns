using System.CommandLine;

namespace TransIp.Dns.Cli.Infrastructure;

public sealed record AuthOptions(
  string Login,
  string PrivateKeyPath,
  string Label,
  bool ReadOnly,
  string Expiration,
  bool GlobalKey,
  bool Verbose)
{
  public static AuthOptions From(ParseResult parse)
  {
    string? login = parse.GetValue(GlobalOptions.Login)
                    ?? Environment.GetEnvironmentVariable("TRANSIP_LOGIN");
    string? keyFile = parse.GetValue(GlobalOptions.KeyFile)
                      ?? Environment.GetEnvironmentVariable("TRANSIP_PRIVATE_KEY_PATH");

    if (string.IsNullOrWhiteSpace(login))
      throw new InvalidOperationException(
        "Missing --login (or TRANSIP_LOGIN env var).");
    if (string.IsNullOrWhiteSpace(keyFile))
      throw new InvalidOperationException(
        "Missing --key-file (or TRANSIP_PRIVATE_KEY_PATH env var).");
    if (!File.Exists(keyFile))
      throw new FileNotFoundException(
        $"Private key file not found: {keyFile}", keyFile);

    return new AuthOptions(
      login,
      keyFile,
      parse.GetValue(GlobalOptions.Label)!,
      parse.GetValue(GlobalOptions.ReadOnly),
      parse.GetValue(GlobalOptions.Expiration)!,
      parse.GetValue(GlobalOptions.GlobalKey),
      parse.GetValue(GlobalOptions.Verbose));
  }
}