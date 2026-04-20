using MicroServiceSales.Infrastructure.Repositories;
using Npgsql;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryDeleteTests
{
    [Fact]
    public void Delete_Should_Execute_Command_When_Id_Is_Valid()
    {
        var database = new FakeDatabase();
        var createdCommands = new List<NpgsqlCommand>();
        var executeCount = 0;
        var repository = new SalesRepository(
            database,
            createSaleDeleteCommand: _ =>
            {
                var cmd = new NpgsqlCommand();
                createdCommands.Add(cmd);
                return cmd;
            },
            executeNonQuery: _ => executeCount++);

        var id = Guid.NewGuid();

        repository.Delete(id);

        Assert.Equal(1, database.GetConnectionCalls);
        Assert.Single(createdCommands);
        Assert.Equal(1, executeCount);
        Assert.Equal(id, createdCommands[0].Parameters["@id"].Value);
    }
}
