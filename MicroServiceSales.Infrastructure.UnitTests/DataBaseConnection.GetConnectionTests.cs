using MicroServiceSales.Infrastructure.DataBase;
using Npgsql;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class DataBaseConnectionGetConnectionTests
{
    [Fact]
    public void GetConnection_Should_Create_Open_And_Return_NpgsqlConnection()
    {
        var connectionString = "Host=localhost;Database=bookstore;Username=user;Password=pass";
        var expected = new NpgsqlConnection();
        var factoryCalls = 0;
        var openCalls = 0;

        var sut = new DataBaseConnection(
            connectionString,
            connectionFactory: cs =>
            {
                factoryCalls++;
                Assert.Equal(connectionString, cs);
                return expected;
            },
            openConnection: conn =>
            {
                openCalls++;
                Assert.Same(expected, conn);
            });

        var result = sut.GetConnection();

        Assert.Equal(1, factoryCalls);
        Assert.Equal(1, openCalls);
        Assert.Same(expected, result);
    }
}
