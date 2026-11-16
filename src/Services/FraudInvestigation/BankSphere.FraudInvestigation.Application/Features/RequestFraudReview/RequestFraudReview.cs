
namespace BankSphere.FraudInvestigation.Application;

public sealed record RequestFraudReview(string Reviewer, string Reason, long ExpectedVersion);
