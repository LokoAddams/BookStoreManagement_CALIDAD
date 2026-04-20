using MicroServiceSales.Domain.Models;
using MicroServiceSales.Infrastructure.Repositories;
using Npgsql;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryCreateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Create_Should_Handle_Optional_Cancellation_Fields(bool useNullCancellationData)
    {
        var database = new FakeDatabase();
        var createdCommands = new List<NpgsqlCommand>();
        var executeCount = 0;
        var repository = CreateRepository(database, createdCommands, () => executeCount++);

        var sale = CreateSale(useNullCancellationData);

        repository.Create(sale);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Single(createdCommands);
        Assert.Equal(1, executeCount);

        var cmd = createdCommands[0];
        Assert.Equal(sale.Id, GetGuidParameter(cmd, "@id"));
        Assert.Equal(sale.ClientId, GetGuidParameter(cmd, "@client_id"));
        Assert.Equal(sale.UserId, GetGuidParameter(cmd, "@user_id"));

        if (useNullCancellationData)
        {
            Assert.Equal(DBNull.Value, cmd.Parameters["@cancellation_reason"].Value);
            Assert.Equal(DBNull.Value, cmd.Parameters["@cancelled_at"].Value);
            Assert.Equal(DBNull.Value, cmd.Parameters["@cancelled_by"].Value);
        }
        else
        {
            Assert.Equal(sale.CancellationReason, cmd.Parameters["@cancellation_reason"].Value);
            Assert.Equal(sale.CancelledAt, cmd.Parameters["@cancelled_at"].Value);
            Assert.Equal(sale.CancelledBy, cmd.Parameters["@cancelled_by"].Value);
        }
    }

    private static SalesRepository CreateRepository(FakeDatabase database, List<NpgsqlCommand> createdCommands, Action onExecute)
    {
        return new SalesRepository(
            database,
            createSaleInsertCommand: _ =>
            {
                var cmd = new NpgsqlCommand();
                createdCommands.Add(cmd);
                return cmd;
            },
            executeNonQuery: _ => onExecute());
    }

    private static Sale CreateSale(bool useNullCancellationData)
    {
        return new Sale
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 100m,
            Total = 100m,
            Status = "PENDING",
            CancellationReason = useNullCancellationData ? null : "Sin stock",
            CancelledAt = useNullCancellationData ? null : DateTimeOffset.UtcNow,
            CancelledBy = useNullCancellationData ? null : Guid.NewGuid()
        };
    }

    private static Guid GetGuidParameter(NpgsqlCommand command, string parameterName)
    {
        return (Guid)command.Parameters[parameterName].Value!;
    }
}
