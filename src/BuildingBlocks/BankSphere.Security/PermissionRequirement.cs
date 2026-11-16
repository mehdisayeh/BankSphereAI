using Microsoft.AspNetCore.Authorization;

namespace BankSphere.Security;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
