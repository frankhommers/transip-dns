using System.CommandLine;

namespace TransIp.Dns.Cli.Infrastructure;

public static class GlobalOptions
{
    public static readonly Option<string?> Login = new("--login")
    {
        Description = "TransIP account login (or env TRANSIP_LOGIN)."
    };

    public static readonly Option<string?> KeyFile = new("--key-file")
    {
        Description = "Path to PEM private key (or env TRANSIP_PRIVATE_KEY_PATH)."
    };

    public static readonly Option<string> Label = new("--label")
    {
        Description = "Token label.",
        DefaultValueFactory = _ => "transip-dns-cli"
    };

    public static readonly Option<string> Expiration = new("--expiration")
    {
        Description = "Token lifetime (e.g. '30 minutes').",
        DefaultValueFactory = _ => "30 minutes"
    };

    public static readonly Option<bool> Verbose = new("--verbose")
    {
        Description = "Print stack traces and raw responses on error."
    };

    public static void AttachTo(Command cmd)
    {
        cmd.Options.Add(Login);
        cmd.Options.Add(KeyFile);
        cmd.Options.Add(Label);
        cmd.Options.Add(Expiration);
        cmd.Options.Add(Verbose);
    }
}
