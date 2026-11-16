
namespace BankSphere.Compliance.Domain;

public sealed record KycReview(Guid Id, DateTimeOffset ReviewedAtUtc, DateTimeOffset NextReviewAtUtc, string Officer, string Findings, string EvidenceReference);
