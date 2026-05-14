using System.Text.Json;
using Apigen.TransIp.Models;

namespace TransIp.Dns.Cli.Infrastructure;

public static class DnsEntryReader
{
  public static List<DnsEntry> Read(JsonElement response)
  {
    List<DnsEntry> result = new();
    if (!response.TryGetProperty("dnsEntries", out JsonElement arr)
        || arr.ValueKind != JsonValueKind.Array)
      return result;

    foreach (JsonElement e in arr.EnumerateArray())
      result.Add(new DnsEntry
      {
        Name = e.TryGetProperty("name", out JsonElement n) ? n.GetString() ?? "" : "",
        Type = e.TryGetProperty("type", out JsonElement t) ? t.GetString() ?? "" : "",
        Content = e.TryGetProperty("content", out JsonElement c) ? c.GetString() ?? "" : "",
        Expire = e.TryGetProperty("expire", out JsonElement ex) ? ex.GetDecimal() : 0m
      });
    return result;
  }
}