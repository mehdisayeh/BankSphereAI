
namespace BankSphere.Customers.Domain;

public sealed record FinancialProfile(string Occupation, EmploymentStatus EmploymentStatus, string? EmployerName,
    string? Industry, string AnnualIncomeRange, string SourceOfFunds, string SourceOfWealth, string ExpectedAccountActivity);
