// New file: IDataBase returns a DbConnection used by repositories
using System.Data.Common;

namespace MicroServiceProduct.Domain.Services
{
    public interface IDataBase
    {
        DbConnection GetConnection();
    }
}

