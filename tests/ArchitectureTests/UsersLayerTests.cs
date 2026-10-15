using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ApiAssembly = Users.Api.AssemblyReference;
using ApplicationAssembly = Users.Application.AssemblyReference;
using DomainAssembly = Users.Domain.AssemblyReference;
using InfrastructureAssembly = Users.Infrastructure.AssemblyReference;

namespace ArchitectureTests;

public class UsersLayerTests
{
    private static readonly Architecture UsersArchitecture = new ArchLoader()
        .LoadAssemblies(
            typeof(ApiAssembly).Assembly,
            typeof(ApplicationAssembly).Assembly,
            typeof(DomainAssembly).Assembly,
            typeof(InfrastructureAssembly).Assembly)
        .Build();

    private static readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInAssembly(typeof(ApiAssembly).Assembly).As("Users Api");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(typeof(ApplicationAssembly).Assembly).As("Users Application");

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(typeof(DomainAssembly).Assembly).As("Users Domain");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly(typeof(InfrastructureAssembly).Assembly).As("Users Infrastructure");

    [Fact]
    public void Domain_ShouldNotDependOn_OtherLayers()
    {
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Because("the Domain is the core of Clean Architecture and must not know about the layers that use it");

        rule.Check(UsersArchitecture);
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Frameworks()
    {
        IArchRule rule = Types().That().Are(DomainLayer)
            .Should().NotDependOnAnyTypesThat()
            .ResideInNamespaceMatching(@"^Microsoft\.(EntityFrameworkCore|AspNetCore)(\..*)?$")
            .Because("business rules must be testable without a database or web server and must survive a change of framework");

        rule.Check(UsersArchitecture);
    }

    [Fact]
    public void Application_ShouldNotDependOn_InfrastructureOrApi()
    {
        IArchRule rule = Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Because("use cases depend on abstractions they define, which Infrastructure implements (Dependency Inversion Principle)");

        rule.Check(UsersArchitecture);
    }
    
    [Fact]
    public void Infrastructure_ShouldNotDependOn_Api()
    {
        IArchRule rule = Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(ApiLayer)
            .Because("Api is the composition root; Infrastructure implements Application's abstractions and must not know how the service is hosted or exposed");

        rule.Check(UsersArchitecture);
    }
}