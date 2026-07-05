# Foundational Architecture and Technology Stack

- **Status:** Accepted
- **Date:** 2026-07-05
- **Deciders:** MOYO OMS team

## Context

I am  building only the Order Management System (OMS) for the MOYO Online
Order Solution. The system must support vendor sign-in, price/stock
maintenance, and management of allocated orders. It integrates with the
Client Portal (async, Azure Service Bus) and the Product Management System
(sync, REST). The case study mandates .NET 8, Angular, EF Core Code First,
Azure SQL, Azure Service Bus, OAuth 2.0 / OpenID Connect, and cloud-first,
PaaS-over-IaaS delivery on a zero-cost (free-tier) budget.

## Decision

1. **Clean Architecture** with four layers: Domain, Application,
   Infrastructure, and Hosts. Dependencies point inward only.
2. **Four independently deployable .NET processes** plus one Angular SPA,
   mirroring the C4 component diagram: OMS API, Order Intake Worker, Order
   Status Worker, and Allocation Function.
3. **.NET 8 (LTS)** as the target framework, pinned via `global.json`, per the
   case study.
4. **Azure PaaS free tiers** for hosting: App Service (F1), Azure SQL (free
   serverless offer), Service Bus, Static Web Apps, and Functions.
5. **Microsoft Entra ID** with OAuth 2.0 / OIDC for authentication, via
   `Microsoft.Identity.Web` (API) and MSAL (Angular).

## Consequences

- **Positive:** Domain and Application layers are written once and shared by
  all four processes (DRY). Swapping infrastructure (DB, messaging) touches
  only the Infrastructure layer. Aligns with the case study's scalability and
  cloud-first principles.
- **Negative:** Four deployable units add operational complexity versus a
  single monolith; justified by the C4 design and future scalability.
- **Note:** Choosing .NET 8 over .NET 10 means we forgo newer runtime features
  to match the brief. Revisit if the brief is relaxed.
