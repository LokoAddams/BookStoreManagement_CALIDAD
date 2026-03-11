using Npgsql;

namespace MicroServiceSales.Domain.Interfaces
{
    public interface IDataBase
    {
        NpgsqlConnection GetConnection();
    }
}
