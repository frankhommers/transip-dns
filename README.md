# TransIp.Dns.Cli

[![CI](https://github.com/frankhommers/transip-dns/actions/workflows/ci.yml/badge.svg)](https://github.com/frankhommers/transip-dns/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Small .NET 10 console app for CRUD operations on TransIP DNS records, built on
[`Apigen.TransIp.Client`](https://www.nuget.org/packages/Apigen.TransIp.Client).

## Run

### Docker (recommended)

Multi-arch image (amd64 + arm64) is published to GitHub Container Registry.

    docker run --rm \
      -v /path/to/transip.pem:/keys/transip.pem:ro \
      ghcr.io/frankhommers/transip-dns:latest \
      --login YOUR_LOGIN --key-file /keys/transip.pem --global-key \
      domains

Available tags:
- `latest` — most recent main branch build
- `vX.Y.Z` and `X.Y` — released versions
- `main` — latest main branch (rolling)
- `sha-<short>` — pinned commit

### From source

    dotnet build
    dotnet run --project src/TransIp.Dns.Cli -- --help

## Authentication

The CLI needs a TransIP login and the PEM private key generated in the TransIP
control panel. Provide them via flags or environment variables:

| Flag           | Env var                      |
|----------------|------------------------------|
| `--login`      | `TRANSIP_LOGIN`              |
| `--key-file`   | `TRANSIP_PRIVATE_KEY_PATH`   |

Other global options:

- `--label` — token label, must be unique per active token. Default is
  auto-generated with a timestamp suffix (e.g. `transip-dns-cli-20260514174538123`).
- `--expiration` — token lifetime, default `30 minutes`.
- `--global-key` — request a token usable from any IP. Without this, the token
  is bound to the IP that requested it; if your account uses IP whitelisting and
  your address is not whitelisted you will get
  `Remote IP is not authorized for this request`.
- `--read-only` — request a read-only token (rejects mutating endpoints).
  Default is full access; opt in for safety.
- `--verbose` — Debug-level SDK logging and full stack traces on error.

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
`--expire <ttl>` to disambiguate. Both `delete` and `update` then list the
matches on stderr so you can pick one.

## Notes

- TransIP records have no IDs; updates and deletes match by tuple
  (name, type, content).
- Two records with identical (name, type, content) are not allowed by the API.
  Round-robin DNS uses multiple records with the same name/type but different
  content.
- Exit codes: `0` success, `1` user/validation error, `2` API error.
- `--verbose` prints the full `ApiException.ResponseBody` on failure (e.g. the
  exact reason behind a 401 or 422).

## License

MIT — see [LICENSE](LICENSE).
