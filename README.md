# TransIp.Dns.Cli

Small .NET 10 console app for CRUD operations on TransIP DNS records, built on
[`Apigen.TransIp.Client`](https://www.nuget.org/packages/Apigen.TransIp.Client).

## Build

    dotnet build

## Authentication

The CLI needs a TransIP login and the PEM private key generated in the TransIP
control panel. Provide them via flags or environment variables:

| Flag           | Env var                      |
|----------------|------------------------------|
| `--login`      | `TRANSIP_LOGIN`              |
| `--key-file`   | `TRANSIP_PRIVATE_KEY_PATH`   |

Other global options:

- `--label`        token label (default `transip-dns-cli`)
- `--expiration`   token lifetime (default `30 minutes`)
- `--verbose`      print stack traces on error

## Commands

### List all domains

    dotnet run --project src/TransIp.Dns.Cli -- domains

### List DNS records for a domain

    dotnet run --project src/TransIp.Dns.Cli -- list example.nl
    dotnet run --project src/TransIp.Dns.Cli -- list example.nl --type A

### Add a DNS record

    dotnet run --project src/TransIp.Dns.Cli -- add example.nl \
      --name www --type A --content 1.2.3.4 --expire 300

### Update a DNS record

Match on the current (`--name --type --content`); change content and/or TTL.

    dotnet run --project src/TransIp.Dns.Cli -- update example.nl \
      --name www --type A --content 1.2.3.4 \
      --new-content 5.6.7.8

### Delete a DNS record

    dotnet run --project src/TransIp.Dns.Cli -- delete example.nl \
      --name www --type A --content 5.6.7.8

If multiple records match (same name+type+content but different TTL), add
`--expire <ttl>` to disambiguate.

## Notes

- TransIP records have no IDs; updates and deletes match by tuple (name, type, content).
- Two records with identical (name, type, content) are not allowed by the API.
  Round-robin DNS uses multiple records with the same name/type but different content.
- Exit codes: `0` success, `1` user/validation error, `2` API error.

## License

MIT
