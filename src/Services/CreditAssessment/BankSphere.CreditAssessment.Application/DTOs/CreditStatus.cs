using BankSphere.CreditAssessment.Domain;

namespace BankSphere.CreditAssessment.Application;

public sealed record CreditStatus(Guid Id, string ApplicationNumber, Guid CustomerId, CreditApplicationStatus Status,
    string ProductId, string Purpose, decimal RequestedAmount, string Currency, int TermMonths, DateTimeOffset? SubmittedAtUtc,
    DateTimeOffset? ReviewedAtUtc, string? AssignedOfficer, long Version);
