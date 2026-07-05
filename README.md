# MOYO Order Management System (OMS)

Vendor-facing Order Management System for the **MOYO Online Order Solution**.
Vendors sign in to maintain product price/stock and to manage orders that have
been allocated to them.

> This repository implements **only the OMS**. The Client Portal and Product
> Management System (PMS) are external systems we integrate with, not build.

## Architecture

Clean Architecture across four independently deployable .NET processes plus an
Angular SPA, communicating via synchronous REST and asynchronous Azure Service
Bus messaging.

| Component | Type | Purpose |
|-----------|------|---------|
| Order Management UI | Angular SPA | Vendor sign-in, price/stock, order workspaces |
| OMS Backend API | ASP.NET Core Web API | Vendor operations, order & inventory rules |
| Order Intake Worker | .NET Worker | Subscribes to `orders.new` from Client Portal |
| Order Status Worker | .NET Worker | Publishes `orders.status` to Client Portal |
| Allocation Function | Azure Function | Matches orders to vendors |
| Order Data Store | Azure SQL | Orders, vendors, price/stock, allocations |

See [`docs/architecture`](docs/architecture) for the C4 model diagrams.

## Technology

- .NET 8 (LTS) · ASP.NET Core · EF Core 8 (Code First)
- Angular · MSAL (OAuth 2.0 / OpenID Connect via Microsoft Entra ID)
- Azure SQL · Azure Service Bus · Azure App Service · Azure Functions
- GitHub Actions (CI/CD)

## Status

🚧 Under active development — see the implementation roadmap.
