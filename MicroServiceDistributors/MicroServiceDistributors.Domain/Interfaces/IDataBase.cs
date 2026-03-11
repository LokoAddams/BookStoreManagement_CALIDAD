using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroServiceUsers.Domain.Interfaces
{
    public interface IDataBase
    {
        DbConnection GetConnection();
    }
}
