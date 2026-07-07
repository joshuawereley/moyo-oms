using System.Reflection;

using FluentAssertions;

using NetArchTest.Rules;

using Xunit;

namespace Moyo.Oms.ArchitectureTests;

public class LayerDependencyTests
{
    private const string Domain = "Moyo.Oms.Domain";
    private const string Application = "Moyo.Oms.Application";
    private const string Infrastructure = "Moyo.Oms.Infrastructure";
    private const string Api = "Moyo.Oms.Api";
    private const string Contracts = "Moyo.Oms.Contracts";

    private static readonly Assembly DomainAssembly =
        typeof(Moyo.Oms.Domain.Common.Entity).Assembly;

    private static readonly Assembly ApplicationAssembly =
        typeof(Moyo.Oms.Application.DependencyInjection).Assembly;

    private static readonly Assembly InfrastructureAssembly =
        typeof(Moyo.Oms.Infrastructure.DependencyInjection).Assembly;

    private static readonly Assembly ContractsAssembly =
        typeof(Moyo.Oms.Contracts.OrderPlaced).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOnAnyOtherLayer()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure, Api, Contracts)
            .GetResult();

        AssertRespected(result, "the Domain is the innermost layer and must reference no other layer");
    }

    [Fact]
    public void Application_ShouldDependOnlyInward()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        AssertRespected(result, "the Application layer must depend only on the Domain, never on Infrastructure or the API");
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(Api)
            .GetResult();

        AssertRespected(result, "Infrastructure must not reference the API host that composes it");
    }

    [Fact]
    public void Contracts_ShouldNotDependOnAnyOtherLayer()
    {
        var result = Types.InAssembly(ContractsAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(Domain, Application, Infrastructure, Api)
            .GetResult();

        AssertRespected(result, "Contracts are standalone integration messages and must stay free of internal layers");
    }

    private static void AssertRespected(TestResult result, string because)
    {
        var offenders = result.FailingTypeNames is null
            ? string.Empty
            : string.Join(", ", result.FailingTypeNames);

        result.IsSuccessful.Should().BeTrue(because + " Offending types: {0}", offenders);
    }
}
