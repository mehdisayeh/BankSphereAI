
namespace BankSphere.Compliance.Domain;

public sealed record ScreeningResult(Guid Id, string Type, string ScreeningSource, string ProtectedReference, bool PotentialMatch, string Disposition, string Officer, DateTimeOffset AtUtc);
