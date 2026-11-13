using System.Xml.Linq;
using BankSphere.SharedKernel;
using Xunit;

namespace BankSphere.Foundation.Tests;

public sealed class FoundationTests
{
    private static readonly string[] ProjectRoots = ["src", "orchestration", "tests"];

    [Fact]
    public void FailedResultCannotExposeAValue()
    {
        var error = new Error("test.failure", "Expected failure");
        var result = Result<int>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Same(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void SuccessPreservesDefaultValue()
    {
        var result = Result<int>.Success(0);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public void AggregateRetainsEventsUntilExplicitlyCleared()
    {
        var aggregate = new TestAggregate();
        var domainEvent = new TestEvent(Guid.NewGuid(), DateTimeOffset.UtcNow);
        aggregate.Raise(domainEvent);
        var events = aggregate.DomainEvents;

        Assert.Same(domainEvent, Assert.Single(events));
        aggregate.ClearDomainEvents();
        Assert.Empty(events);
    }

    [Fact]
    public void ProductionProjectGraphRespectsTheApprovedDependencies()
    {
        var root = FindRepositoryRoot();
        var allowed = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["BankSphere.SharedKernel"] = [],
            ["BankSphere.Observability"] = [],
            ["BankSphere.EventBus"] = [],
            ["BankSphere.Security"] = [],
            ["BankSphere.AI.Domain"] = ["BankSphere.SharedKernel"],
            ["BankSphere.AI.Application"] = ["BankSphere.AI.Domain"],
            ["BankSphere.AI.Infrastructure"] = ["BankSphere.AI.Application", "BankSphere.AI.Domain"],
            ["BankSphere.AI.Api"] = ["BankSphere.AI.Application", "BankSphere.AI.Infrastructure", "BankSphere.ServiceDefaults"],
            ["BankSphere.AI.Contracts"] = [],
            ["BankSphere.AI.Agents"] = ["BankSphere.AI.Contracts"],
            ["BankSphere.AI.Orchestration"] = ["BankSphere.AI.Contracts"],
            ["BankSphere.AI.Rag"] = ["BankSphere.AI.Contracts"],
            ["BankSphere.AI.Memory"] = ["BankSphere.AI.Contracts"],
            ["BankSphere.McpServer"] = ["BankSphere.ServiceDefaults"],
            ["BankSphere.A2AHost"] = ["BankSphere.ServiceDefaults"],
            ["BankSphere.Gateway"] = ["BankSphere.ServiceDefaults"],
            ["BankSphere.ServiceDefaults"] = ["BankSphere.Observability"]
        };
        string[] contexts = ["Customers", "Accounts", "Transactions", "FraudInvestigation", "Compliance", "CreditAssessment"];
        var hosts = new List<string> { "BankSphere.AI.Api", "BankSphere.McpServer", "BankSphere.A2AHost", "BankSphere.Gateway" };
        foreach (var context in contexts)
        {
            var prefix = $"BankSphere.{context}";
            allowed[$"{prefix}.Domain"] = ["BankSphere.SharedKernel"];
            allowed[$"{prefix}.Application"] = [$"{prefix}.Domain"];
            allowed[$"{prefix}.Infrastructure"] = [$"{prefix}.Application", $"{prefix}.Domain"];
            allowed[$"{prefix}.Api"] = [$"{prefix}.Application", $"{prefix}.Infrastructure", "BankSphere.ServiceDefaults"];
            hosts.Add($"{prefix}.Api");
        }

        allowed["BankSphere.AppHost"] = [.. hosts];
        var projects = Directory.GetFiles(Path.Combine(root, "src"), "*.csproj", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(Path.Combine(root, "orchestration"), "*.csproj", SearchOption.AllDirectories))
            .ToArray();
        Assert.Equal(allowed.Count, projects.Length);
        foreach (var project in projects)
        {
            var name = Path.GetFileNameWithoutExtension(project);
            Assert.True(allowed.ContainsKey(name), $"Unreviewed project: {name}");
            var document = XDocument.Load(project);
            var references = document.Descendants("ProjectReference")
                .Select(element => Path.GetFileNameWithoutExtension((string)element.Attribute("Include")!))
                .Order(StringComparer.Ordinal).ToArray();
            Assert.Equal(allowed[name].Order(StringComparer.Ordinal), references);
            if (name.EndsWith(".Domain", StringComparison.Ordinal)
                || name.EndsWith(".Application", StringComparison.Ordinal)
                || name is "BankSphere.SharedKernel" or "BankSphere.AI.Contracts")
            {
                Assert.Empty(document.Descendants("PackageReference"));
                Assert.Empty(document.Descendants("FrameworkReference"));
            }
        }
    }

    [Fact]
    public void SolutionRegistersEveryProjectExactlyOnce()
    {
        var root = FindRepositoryRoot();
        var solution = XDocument.Load(Path.Combine(root, "BankSphereAI.slnx"));
        var registered = solution.Descendants("Project")
            .Select(project => Path.GetFullPath(Path.Combine(root, (string)project.Attribute("Path")!)))
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();
        var physical = ProjectRoots
            .SelectMany(folder => Directory.GetFiles(Path.Combine(root, folder), "*.csproj", SearchOption.AllDirectories))
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();

        Assert.Equal(physical, registered);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BankSphereAI.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private sealed class TestAggregate() : AggregateRoot<Guid>(Guid.NewGuid())
    {
        public void Raise(IDomainEvent domainEvent) => RaiseDomainEvent(domainEvent);
    }

    private sealed record TestEvent(Guid EventId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
}
