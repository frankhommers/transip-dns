using System.Text.Json;
using Apigen.TransIp.Models;

namespace TransIp.Dns.Cli.Infrastructure;

public static class DnsEntryReader
{
    public static List<DnsEntry> Read(JsonElement response)
    {
        var result = new List<DnsEntry>();
        if (!response.TryGetProperty("dnsEntries", out var arr)
            || arr.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var e in arr.EnumerateArray())
        {
            result.Add(new DnsEntry
            {
                Name = e.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                Type = e.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "",
                Content = e.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "",
                Expire = e.TryGetProperty("expire", out var ex) ? ex.GetDecimal() : 0m
            });
        }
        return result;
    }
}
