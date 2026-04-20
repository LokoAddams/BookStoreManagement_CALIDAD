using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;
using Npgsql;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryCreateDetailsTests
{
    [Fact]
    public void CreateDetails_Should_End_Immediately_When_Details_Are_Empty()
    {
        var database = new FakeDatabase();
        var createdCommands = new List<NpgsqlCommand>();
        var executeCount = 0;
        var repository = CreateRepository(database, createdCommands, () => executeCount++);

        repository.CreateDetails(Guid.NewGuid(), Array.Empty<SaleDetail>());

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Empty(createdCommands);
        Assert.Equal(0, executeCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateDetails_Should_Handle_Detail_Id_By_Scenario(bool useEmptyId)
    {
        var database = new FakeDatabase();
        var createdCommands = new List<NpgsqlCommand>();
        var executeCount = 0;
        var repository = CreateRepository(database, createdCommands, () => executeCount++);
        var saleId = Guid.NewGuid();
        var existingId = Guid.NewGuid();
        var detail = CreateDetail(useEmptyId ? Guid.Empty : existingId);

        repository.CreateDetails(saleId, new[] { detail });

        Assert.Single(createdCommands);
        Assert.Equal(1, executeCount);

        var actualId = GetGuidParameter(createdCommands[0], "@id");
        if (useEmptyId)
            Assert.NotEqual(Guid.Empty, actualId);
        else
            Assert.Equal(existingId, actualId);

        Assert.Equal(saleId, GetGuidParameter(createdCommands[0], "@sale_id"));
        Assert.Equal(detail.ProductId, GetGuidParameter(createdCommands[0], "@product_id"));
    }

    [Fact]
    public void CreateDetails_Should_Process_Each_Item_And_Apply_Id_Logic_Per_Detail()
    {
        var database = new FakeDatabase();
        var createdCommands = new List<NpgsqlCommand>();
        var executeCount = 0;
        var repository = CreateRepository(database, createdCommands, () => executeCount++);

        var saleId = Guid.NewGuid();
        var providedId = Guid.NewGuid();
        var first = CreateDetail(Guid.Empty);
        var second = CreateDetail(providedId);

        repository.CreateDetails(saleId, new[] { first, second });

        Assert.Equal(2, createdCommands.Count);
        Assert.Equal(2, executeCount);

        Assert.NotEqual(Guid.Empty, GetGuidParameter(createdCommands[0], "@id"));
        Assert.Equal(first.ProductId, GetGuidParameter(createdCommands[0], "@product_id"));

        Assert.Equal(providedId, GetGuidParameter(createdCommands[1], "@id"));
        Assert.Equal(second.ProductId, GetGuidParameter(createdCommands[1], "@product_id"));

        Assert.All(createdCommands, cmd => Assert.Equal(saleId, GetGuidParameter(cmd, "@sale_id")));
    }

    private static SalesRepository CreateRepository(FakeDatabase database, List<NpgsqlCommand> createdCommands, Action onExecute)
    {
        return new SalesRepository(
            database,
            createDetailInsertCommand: _ =>
            {
                var cmd = new NpgsqlCommand();
                createdCommands.Add(cmd);
                return cmd;
            },
            executeNonQuery: _ => onExecute());
    }

    private static SaleDetail CreateDetail(Guid id)
    {
        return new SaleDetail
        {
            Id = id,
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = 10m,
            Subtotal = 20m
        };
    }

    private static Guid GetGuidParameter(NpgsqlCommand command, string parameterName)
    {
        return (Guid)command.Parameters[parameterName].Value!;
    }
}
