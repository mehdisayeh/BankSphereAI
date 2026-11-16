var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BankSphere_Identity_Api>("banksphere-identity-api", launchProfileName: null)
    .WithHttpsEndpoint(port: 7100, env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_AI_Api>("banksphere-ai-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_Customers_Api>("banksphere-customers-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_Accounts_Api>("banksphere-accounts-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_Transactions_Api>("banksphere-transactions-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_FraudInvestigation_Api>("banksphere-fraudinvestigation-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_Compliance_Api>("banksphere-compliance-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_CreditAssessment_Api>("banksphere-creditassessment-api", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_McpServer>("banksphere-mcpserver", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_A2AHost>("banksphere-a2ahost", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.BankSphere_Gateway>("banksphere-gateway", launchProfileName: null)
    .WithHttpEndpoint(env: "ASPNETCORE_URLS")
    .WithHttpHealthCheck("/health/ready");

builder.Build().Run();
