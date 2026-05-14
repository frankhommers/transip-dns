using Apigen.TransIp.Client;

namespace TransIp.Dns.Cli.Infrastructure;

public static class ErrorHandler
{
    public static int Handle(Exception ex, bool verbose)
    {
        var isUserError =
            ex is InvalidOperationException
              or FileNotFoundException
              or ArgumentException;

        if (verbose)
        {
            Console.Error.WriteLine(ex);
        }
        else if (ex is ApiException api)
        {
            var body = string.IsNullOrWhiteSpace(api.ResponseBody)
                ? "(no body)"
                : api.ResponseBody;
            Console.Error.WriteLine($"{api.Message}: {body}");
        }
        else
        {
            Console.Error.WriteLine(ex.Message);
        }

        return isUserError ? 1 : 2;
    }
}
