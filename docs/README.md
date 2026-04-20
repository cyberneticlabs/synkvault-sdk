# SynkVault SDK Documentation

Official SDK documentation for the SynkVault Partners API, organized by language and platform.

## SDKs

### JavaScript/TypeScript

Official Node.js & browser SDK with full feature support.

- **Installation**: `npm install @synkvault/sdk`
- **Node**: >=18
- **Features**: Resources, Chat with streaming, Type safety
- **[Quick Start](https://github.com/CyberneticLabs/synkvault-sdk/blob/master/docs/js/README.md)** — See documentation links inside

### .NET

Official .NET SDK for .NET 6+ with streaming support.

- **Installation**: NuGet package coming soon
- **Runtime**: .NET 6+
- **Features**: Resources, Chat with streaming
- **[Getting Started](https://github.com/CyberneticLabs/synkvault-sdk/blob/master/docs/dotnet/README.md)** — See documentation links inside

### Python

Official Python SDK (planned).

- **Status**: In development
- **Python**: 3.10+
- **Planned Features**: Resources, Chat with streaming, Async support
- **[Roadmap](https://github.com/CyberneticLabs/synkvault-sdk/blob/master/docs/python/README.md)**

---

## Common Concepts

### Resources

All SDKs follow the same resource-based architecture:

- **Health** — API status and version
- **Orgs** — Organization and user management
- **Ontology** — Schema definitions (entity types, relationships)
- **Knowledge** — Query the knowledge graph
- **Ingest** — Load data (text, JSON, URLs)
- **Documents** — Upload and extract from files
- **Chat** — AI agent conversations

### Authentication

All API requests require either:
- **API Key** (server-to-server): `X-Api-Key` header
- **Bearer Token** (OAuth, user sessions): `Authorization: Bearer` header

### Organization Context

Most requests automatically include your organization ID (`org_id`) as a query parameter, scoping all data to your tenant.

### Errors

All SDKs throw language-specific exceptions with consistent structure:

**JavaScript**: `SynkVaultError`
**C#**: `SynkVaultException`
**Python**: `SynkVaultError` (coming)

---

## API Reference

- **OpenAPI Spec**: [https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json](https://synkvault-web-dev-partners-yqmxkygnlq-nw.a.run.app/api/v1/docs/openapi.json)
- **Package Repository**: [NPM @synkvault/sdk](https://www.npmjs.com/package/@synkvault/sdk)

---

## Support

For issues, questions, or contributions, contact the SynkVault team.

**License**: MIT
