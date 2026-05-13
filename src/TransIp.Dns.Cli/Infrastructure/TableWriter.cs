namespace TransIp.Dns.Cli.Infrastructure;

public static class TableWriter
{
    public static void Write(
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string>> rows)
    {
        var all = new List<IReadOnlyList<string>> { headers };
        all.AddRange(rows);

        var widths = new int[headers.Count];
        foreach (var r in all)
            for (var i = 0; i < headers.Count; i++)
                widths[i] = Math.Max(widths[i], (r[i] ?? "").Length);

        void WriteRow(IReadOnlyList<string> r) =>
            Console.WriteLine(string.Join("  ",
                r.Select((c, i) => (c ?? "").PadRight(widths[i]))));

        WriteRow(headers);
        Console.WriteLine(string.Join("  ", widths.Select(w => new string('-', w))));
        foreach (var r in all.Skip(1)) WriteRow(r);
    }
}
