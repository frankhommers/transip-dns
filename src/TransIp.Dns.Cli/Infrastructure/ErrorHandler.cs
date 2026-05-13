namespace TransIp.Dns.Cli.Infrastructure;

public static class ErrorHandler
{
    public static int Handle(Exception ex, bool verbose)
    {
        var isUserError =
            ex is InvalidOperationException
              or FileNotFoundException
              or ArgumentException;
        Console.Error.WriteLine(verbose ? ex.ToString() : ex.Message);
        return isUserError ? 1 : 2;
    }
}
