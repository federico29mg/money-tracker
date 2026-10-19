using Transactions.Domain.CategoryAggregate;
using Transactions.Domain.Common;

namespace Transactions.UnitTests.Domain.CategoryAggregate;

public class CategoryTests
{
    private static readonly UserId UserId = UserId.From(Guid.NewGuid());

    private static Category CreateCategory(string name = "Groceries", TransactionType type = TransactionType.Expense) =>
        Category.Create(UserId, name, type);

    [Fact]
    public void Create_ValidData_SetsProperties()
    {
        var category = Category.Create(UserId, "Salary", TransactionType.Income);

        Assert.NotEqual(default, category.Id);
        Assert.Equal(UserId, category.UserId);
        Assert.Equal("Salary", category.Name);
        Assert.Equal(TransactionType.Income, category.Type);
        Assert.False(category.IsArchived);
    }

    [Fact]
    public void Create_NameWithSpaces_TrimsName() => Assert.Equal("Rent", CreateCategory("  Rent  ").Name);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_MissingName_ThrowsDomainException(string name) =>
        Assert.Throws<DomainException>(() => CreateCategory(name));

    [Fact]
    public void Create_TooLongName_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => CreateCategory(new string('a', Category.MaxNameLength + 1)));

    [Fact]
    public void Create_UnknownType_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => CreateCategory(type: (TransactionType)99));

    [Fact]
    public void Create_DefaultUserId_ThrowsDomainException() =>
        Assert.Throws<DomainException>(() => Category.Create(default, "Rent", TransactionType.Expense));

    [Fact]
    public void Rename_ValidName_ChangesName()
    {
        var category = CreateCategory();

        category.Rename("Supermarket");

        Assert.Equal("Supermarket", category.Name);
    }

    [Fact]
    public void Archive_MarksCategoryAsArchived()
    {
        var category = CreateCategory();

        category.Archive();

        Assert.True(category.IsArchived);
    }
}