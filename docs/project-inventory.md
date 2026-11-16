# Project inventory

Generated from the 52 physical projects on 2026-11-16. All inherit net9.0 from Directory.Build.props.

| Project | Target framework | Direct project references |
| --- | --- | --- |
| orchestration/BankSphere.AppHost/BankSphere.AppHost.csproj | net9.0 | BankSphere.Identity.Api, BankSphere.AI.Api, BankSphere.Customers.Api, BankSphere.Accounts.Api, BankSphere.Transactions.Api, BankSphere.FraudInvestigation.Api, BankSphere.Compliance.Api, BankSphere.CreditAssessment.Api, BankSphere.McpServer, BankSphere.A2AHost, BankSphere.Gateway |
| orchestration/BankSphere.ServiceDefaults/BankSphere.ServiceDefaults.csproj | net9.0 | BankSphere.Observability, BankSphere.Security |
| src/AI/BankSphere.AI.Agents/BankSphere.AI.Agents.csproj | net9.0 | BankSphere.AI.Contracts |
| src/AI/BankSphere.AI.Api/BankSphere.AI.Api.csproj | net9.0 | BankSphere.AI.Application, BankSphere.AI.Infrastructure, BankSphere.ServiceDefaults |
| src/AI/BankSphere.AI.Application/BankSphere.AI.Application.csproj | net9.0 | BankSphere.AI.Domain |
| src/AI/BankSphere.AI.Contracts/BankSphere.AI.Contracts.csproj | net9.0 |  |
| src/AI/BankSphere.AI.Domain/BankSphere.AI.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/AI/BankSphere.AI.Infrastructure/BankSphere.AI.Infrastructure.csproj | net9.0 | BankSphere.AI.Application, BankSphere.AI.Domain |
| src/AI/BankSphere.AI.Memory/BankSphere.AI.Memory.csproj | net9.0 | BankSphere.AI.Contracts |
| src/AI/BankSphere.AI.Orchestration/BankSphere.AI.Orchestration.csproj | net9.0 | BankSphere.AI.Contracts |
| src/AI/BankSphere.AI.Rag/BankSphere.AI.Rag.csproj | net9.0 | BankSphere.AI.Contracts |
| src/BuildingBlocks/BankSphere.EventBus/BankSphere.EventBus.csproj | net9.0 |  |
| src/BuildingBlocks/BankSphere.Observability/BankSphere.Observability.csproj | net9.0 |  |
| src/BuildingBlocks/BankSphere.Persistence/BankSphere.Persistence.csproj | net9.0 | BankSphere.SharedKernel |
| src/BuildingBlocks/BankSphere.Security/BankSphere.Security.csproj | net9.0 | BankSphere.SharedKernel |
| src/BuildingBlocks/BankSphere.SharedKernel/BankSphere.SharedKernel.csproj | net9.0 |  |
| src/Gateway/BankSphere.Gateway/BankSphere.Gateway.csproj | net9.0 | BankSphere.ServiceDefaults |
| src/Integrations/BankSphere.A2AHost/BankSphere.A2AHost.csproj | net9.0 | BankSphere.ServiceDefaults |
| src/Integrations/BankSphere.McpServer/BankSphere.McpServer.csproj | net9.0 | BankSphere.ServiceDefaults |
| src/Services/Accounts/BankSphere.Accounts.Api/BankSphere.Accounts.Api.csproj | net9.0 | BankSphere.Accounts.Application, BankSphere.Accounts.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/Accounts/BankSphere.Accounts.Application/BankSphere.Accounts.Application.csproj | net9.0 | BankSphere.Accounts.Domain |
| src/Services/Accounts/BankSphere.Accounts.Domain/BankSphere.Accounts.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/Accounts/BankSphere.Accounts.Infrastructure/BankSphere.Accounts.Infrastructure.csproj | net9.0 | BankSphere.Accounts.Application, BankSphere.Accounts.Domain, BankSphere.Persistence |
| src/Services/Compliance/BankSphere.Compliance.Api/BankSphere.Compliance.Api.csproj | net9.0 | BankSphere.Compliance.Application, BankSphere.Compliance.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/Compliance/BankSphere.Compliance.Application/BankSphere.Compliance.Application.csproj | net9.0 | BankSphere.Compliance.Domain |
| src/Services/Compliance/BankSphere.Compliance.Domain/BankSphere.Compliance.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/Compliance/BankSphere.Compliance.Infrastructure/BankSphere.Compliance.Infrastructure.csproj | net9.0 | BankSphere.Compliance.Application, BankSphere.Compliance.Domain, BankSphere.Persistence |
| src/Services/CreditAssessment/BankSphere.CreditAssessment.Api/BankSphere.CreditAssessment.Api.csproj | net9.0 | BankSphere.CreditAssessment.Application, BankSphere.CreditAssessment.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/CreditAssessment/BankSphere.CreditAssessment.Application/BankSphere.CreditAssessment.Application.csproj | net9.0 | BankSphere.CreditAssessment.Domain |
| src/Services/CreditAssessment/BankSphere.CreditAssessment.Domain/BankSphere.CreditAssessment.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/CreditAssessment/BankSphere.CreditAssessment.Infrastructure/BankSphere.CreditAssessment.Infrastructure.csproj | net9.0 | BankSphere.CreditAssessment.Application, BankSphere.CreditAssessment.Domain, BankSphere.Persistence |
| src/Services/Customers/BankSphere.Customers.Api/BankSphere.Customers.Api.csproj | net9.0 | BankSphere.Customers.Application, BankSphere.Customers.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/Customers/BankSphere.Customers.Application/BankSphere.Customers.Application.csproj | net9.0 | BankSphere.Customers.Domain |
| src/Services/Customers/BankSphere.Customers.Domain/BankSphere.Customers.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/Customers/BankSphere.Customers.Infrastructure/BankSphere.Customers.Infrastructure.csproj | net9.0 | BankSphere.Customers.Application, BankSphere.Customers.Domain, BankSphere.Persistence |
| src/Services/FraudInvestigation/BankSphere.FraudInvestigation.Api/BankSphere.FraudInvestigation.Api.csproj | net9.0 | BankSphere.FraudInvestigation.Application, BankSphere.FraudInvestigation.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/FraudInvestigation/BankSphere.FraudInvestigation.Application/BankSphere.FraudInvestigation.Application.csproj | net9.0 | BankSphere.FraudInvestigation.Domain |
| src/Services/FraudInvestigation/BankSphere.FraudInvestigation.Domain/BankSphere.FraudInvestigation.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/FraudInvestigation/BankSphere.FraudInvestigation.Infrastructure/BankSphere.FraudInvestigation.Infrastructure.csproj | net9.0 | BankSphere.FraudInvestigation.Application, BankSphere.FraudInvestigation.Domain, BankSphere.Persistence |
| src/Services/Identity/BankSphere.Identity.Api/BankSphere.Identity.Api.csproj | net9.0 | BankSphere.Identity.Application, BankSphere.Identity.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/Identity/BankSphere.Identity.Application/BankSphere.Identity.Application.csproj | net9.0 | BankSphere.Identity.Domain |
| src/Services/Identity/BankSphere.Identity.Domain/BankSphere.Identity.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/Identity/BankSphere.Identity.Infrastructure/BankSphere.Identity.Infrastructure.csproj | net9.0 | BankSphere.Identity.Application, BankSphere.Identity.Domain, BankSphere.Persistence, BankSphere.Security |
| src/Services/Transactions/BankSphere.Transactions.Api/BankSphere.Transactions.Api.csproj | net9.0 | BankSphere.Transactions.Application, BankSphere.Transactions.Infrastructure, BankSphere.ServiceDefaults |
| src/Services/Transactions/BankSphere.Transactions.Application/BankSphere.Transactions.Application.csproj | net9.0 | BankSphere.Transactions.Domain |
| src/Services/Transactions/BankSphere.Transactions.Domain/BankSphere.Transactions.Domain.csproj | net9.0 | BankSphere.SharedKernel |
| src/Services/Transactions/BankSphere.Transactions.Infrastructure/BankSphere.Transactions.Infrastructure.csproj | net9.0 | BankSphere.Transactions.Application, BankSphere.Transactions.Domain, BankSphere.Persistence |
| tests/BankSphere.Foundation.Tests/BankSphere.Foundation.Tests.csproj | net9.0 | BankSphere.SharedKernel |
| tests/Integration/BankSphere.Identity.IntegrationTests/BankSphere.Identity.IntegrationTests.csproj | net9.0 | BankSphere.Identity.Api, BankSphere.Customers.Api, BankSphere.Accounts.Api, BankSphere.Transactions.Api, BankSphere.FraudInvestigation.Api, BankSphere.Compliance.Api, BankSphere.CreditAssessment.Api |
| tests/Integration/BankSphere.Persistence.Tests/BankSphere.Persistence.Tests.csproj | net9.0 | BankSphere.Customers.Infrastructure, BankSphere.Accounts.Infrastructure, BankSphere.Transactions.Infrastructure, BankSphere.FraudInvestigation.Infrastructure, BankSphere.Compliance.Infrastructure, BankSphere.CreditAssessment.Infrastructure |
| tests/Unit/BankSphere.Banking.Tests/BankSphere.Banking.Tests.csproj | net9.0 | BankSphere.Customers.Infrastructure, BankSphere.Accounts.Infrastructure, BankSphere.Transactions.Infrastructure, BankSphere.FraudInvestigation.Infrastructure, BankSphere.Compliance.Infrastructure, BankSphere.CreditAssessment.Infrastructure, BankSphere.Customers.Api, BankSphere.Accounts.Api, BankSphere.Transactions.Api, BankSphere.FraudInvestigation.Api, BankSphere.Compliance.Api, BankSphere.CreditAssessment.Api |
| tests/Unit/BankSphere.Identity.Tests/BankSphere.Identity.Tests.csproj | net9.0 | BankSphere.Identity.Application, BankSphere.Security |
