using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

using TransactionsApiAssembly = Transactions.Api.AssemblyReference;
using TransactionsApplicationAssembly = Transactions.Application.AssemblyReference;
using TransactionsDomainAssembly = Transactions.Domain.AssemblyReference;
using TransactionsInfrastructureAssembly = Transactions.Infrastructure.AssemblyReference;
using UsersApiAssembly = Users.Api.AssemblyReference;
using UsersApplicationAssembly = Users.Application.AssemblyReference;
using UsersDomainAssembly = Users.Domain.AssemblyReference;
using UsersInfrastructureAssembly = Users.Infrastructure.AssemblyReference;

namespace ArchitectureTests;

public class ServiceBoundaryTests
{
    private const string UsersNamespace = @"^Users(\..*)?$";
    private const string TransactionsNamespace = @"^Transactions(\..*)?$";

    private static readonly Architecture AllServices = new ArchLoader()
        .LoadAssemblies(
            typeof(UsersApiAssembly).Assembly,
            typeof(UsersApplicationAssembly).Assembly,
            typeof(UsersDomainAssembly).Assembly,
            typeof(UsersInfrastructureAssembly).Assembly,
            typeof(TransactionsApiAssembly).Assembly,
            typeof(TransactionsApplicationAssembly).Assembly,
            typeof(TransactionsDomainAssembly).Assembly,
            typeof(TransactionsInfrastructureAssembly).Assembly)
        .Build();

    [Fact]
    public void Users_ShouldNotDependOn_Transactions()
    {
        IArchRule rule = Types().That().ResideInNamespaceMatching(UsersNamespace)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(TransactionsNamespace)
            .Because("services share data only through their APIs, tokens or events, never through each other's code");

        rule.Check(AllServices);
    }

    [Fact]
    public void Transactions_ShouldNotDependOn_Users()
    {
        IArchRule rule = Types().That().ResideInNamespaceMatching(TransactionsNamespace)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(UsersNamespace)
            .Because("services share data only through their APIs, tokens or events, never through each other's code");

        rule.Check(AllServices);
    }
}