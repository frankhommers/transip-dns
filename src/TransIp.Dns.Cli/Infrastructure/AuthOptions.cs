namespace TransIp.Dns.Cli.Infrastructure;

public sealed record AuthOptions(
    string Login,
    string PrivateKeyPath,
    string Label,
    bool ReadOnly,
    string Expiration)
{
    public static AuthOptions Resolve(
        string? login,
        string? keyFile,
        string label,
        bool readOnly,
        string expiration)
    {
        login ??= Environment.GetEnvironmentVariable("TRANSIP_LOGIN");
        keyFile ??= Environment.GetEnvironmentVariable("TRANSIP_PRIVATE_KEY_PATH");

        if (string.IsNullOrWhiteSpace(login))
            throw new InvalidOperationException(
                "Missing --login (or TRANSIP_LOGIN env var).");
        if (string.IsNullOrWhiteSpace(keyFile))
            throw new InvalidOperationException(
                "Missing --key-file (or TRANSIP_PRIVATE_KEY_PATH env var).");
        if (!File.Exists(keyFile))
            throw new FileNotFoundException(
                $"Private key file not found: {keyFile}", keyFile);

        return new AuthOptions(login, keyFile, label, readOnly, expiration);
    }
}
