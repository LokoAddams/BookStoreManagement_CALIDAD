using MicroServiceProduct.Domain.Models;
using MicroServiceProduct.Domain.Services;
using MicroServiceProduct.Infraestructure.Repository;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace XUnit;

public class ProductRepositoryTests
{
    [Fact]
    public void Create_ShouldMap_AllProvidedValues()
    {
        var command = new FakeDbCommand { NonQueryResult = 1 };
        var database = new FakeDatabase(new FakeDbConnection(command));
        var repository = new ProductRepository(database);

        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 10, 1, 8, 30, 0, DateTimeKind.Utc);

        var product = new Product
        {
            Id = id,
            Name = "Libro",
            Description = "Descripcion",
            CategoryId = categoryId,
            Price = 55.5m,
            Stock = 12,
            CreatedAt = createdAt
        };

        repository.Create(product);

        Assert.Equal(1, command.ExecuteNonQueryCalls);
        Assert.Contains("INSERT INTO products", command.CommandText, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(id, command.GetParameterValue("@id"));
        Assert.Equal("Libro", command.GetParameterValue("@name"));
        Assert.Equal("Descripcion", command.GetParameterValue("@description"));
        Assert.Equal(categoryId, command.GetParameterValue("@category_id"));
        Assert.Equal(55.5m, command.GetParameterValue("@price"));
        Assert.Equal(12, command.GetParameterValue("@stock"));
        Assert.Equal(createdAt, command.GetParameterValue("@created_at"));
    }

    [Fact]
    public void Create_ShouldUseFallbackValues_WhenOptionalDataIsMissing()
    {
        var command = new FakeDbCommand { NonQueryResult = 1 };
        var database = new FakeDatabase(new FakeDbConnection(command));
        var repository = new ProductRepository(database);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Libro",
            Description = null!,
            CategoryId = Guid.Empty,
            Price = 20m,
            Stock = 1,
            CreatedAt = default
        };

        var before = DateTime.UtcNow;
        repository.Create(product);
        var after = DateTime.UtcNow;

        Assert.Equal(DBNull.Value, command.GetParameterValue("@description"));
        Assert.Equal(DBNull.Value, command.GetParameterValue("@category_id"));

        var createdAt = Assert.IsType<DateTime>(command.GetParameterValue("@created_at"));
        Assert.InRange(createdAt, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void Read_ShouldReturnMappedProduct_WhenRowExistsWithValues()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var createdAt = new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc);

        var reader = BuildReader(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["name"] = "Libro A",
            ["description"] = "Descripcion A",
            ["category_id"] = categoryId,
            ["category_name"] = "Categoria A",
            ["price"] = 100.25m,
            ["stock"] = 8,
            ["created_at"] = createdAt
        });

        var command = new FakeDbCommand { ReaderFactory = () => reader };
        var database = new FakeDatabase(new FakeDbConnection(command));
        var repository = new ProductRepository(database);

        var product = repository.Read(id);

        Assert.NotNull(product);
        Assert.Equal(id, product!.Id);
        Assert.Equal("Libro A", product.Name);
        Assert.Equal("Descripcion A", product.Description);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.Equal("Categoria A", product.CategoryName);
        Assert.Equal(100.25m, product.Price);
        Assert.Equal(8, product.Stock);
        Assert.Equal(createdAt, product.CreatedAt);
        Assert.Equal(id, command.GetParameterValue("@id"));
    }

    [Fact]
    public void Read_ShouldMapNullables_WhenDatabaseColumnsAreNull()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2024, 2, 20, 7, 0, 0, DateTimeKind.Utc);

        var reader = BuildReader(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["name"] = "Libro B",
            ["description"] = DBNull.Value,
            ["category_id"] = DBNull.Value,
            ["category_name"] = DBNull.Value,
            ["price"] = 10m,
            ["stock"] = 3,
            ["created_at"] = createdAt
        });

        var command = new FakeDbCommand { ReaderFactory = () => reader };
        var database = new FakeDatabase(new FakeDbConnection(command));
        var repository = new ProductRepository(database);

        var product = repository.Read(id);

        Assert.NotNull(product);
        Assert.Equal(string.Empty, product!.Description);
        Assert.Equal(Guid.Empty, product.CategoryId);
        Assert.Null(product.CategoryName);
    }

    [Fact]
    public void Read_ShouldReturnNull_WhenNoRowsAreReturned()
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(Guid));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("category_id", typeof(Guid));
        table.Columns.Add("category_name", typeof(string));
        table.Columns.Add("price", typeof(decimal));
        table.Columns.Add("stock", typeof(int));
        table.Columns.Add("created_at", typeof(DateTime));

        var command = new FakeDbCommand { ReaderFactory = () => table.CreateDataReader() };
        var database = new FakeDatabase(new FakeDbConnection(command));
        var repository = new ProductRepository(database);

        var result = repository.Read(Guid.NewGuid());

        Assert.Null(result);
    }

    private static DbDataReader BuildReader(Dictionary<string, object?> row)
    {
        var table = new DataTable();
        table.Columns.Add("id", typeof(Guid));
        table.Columns.Add("name", typeof(string));
        table.Columns.Add("description", typeof(string));
        table.Columns.Add("category_id", typeof(Guid));
        table.Columns.Add("category_name", typeof(string));
        table.Columns.Add("price", typeof(decimal));
        table.Columns.Add("stock", typeof(int));
        table.Columns.Add("created_at", typeof(DateTime));

        var dataRow = table.NewRow();
        foreach (var kv in row)
        {
            dataRow[kv.Key] = kv.Value ?? DBNull.Value;
        }

        table.Rows.Add(dataRow);
        return table.CreateDataReader();
    }

    private sealed class FakeDatabase : IDataBase
    {
        private readonly DbConnection _connection;

        public FakeDatabase(DbConnection connection)
        {
            _connection = connection;
        }

        public DbConnection GetConnection() => _connection;
    }

    private sealed class FakeDbConnection : DbConnection
    {
        private readonly DbCommand _command;

        public FakeDbConnection(DbCommand command)
        {
            _command = command;
        }

        public override string ConnectionString { get; set; } = string.Empty;
        public override string Database => "FakeDatabase";
        public override string DataSource => "FakeSource";
        public override string ServerVersion => "1.0";
        public override ConnectionState State => ConnectionState.Open;

        public override void ChangeDatabase(string databaseName) { }
        public override void Close() { }
        public override void Open() { }
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
        protected override DbCommand CreateDbCommand() => _command;
    }

    private sealed class FakeDbCommand : DbCommand
    {
        private readonly FakeParameterCollection _parameters = new();

        public int NonQueryResult { get; set; }
        public int ExecuteNonQueryCalls { get; private set; }
        public Func<DbDataReader>? ReaderFactory { get; set; }

        public override string CommandText { get; set; } = string.Empty;
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; } = CommandType.Text;
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbConnection DbConnection { get; set; } = null!;
        protected override DbParameterCollection DbParameterCollection => _parameters;
        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel() { }
        public override int ExecuteNonQuery()
        {
            ExecuteNonQueryCalls++;
            return NonQueryResult;
        }

        public override object? ExecuteScalar() => throw new NotSupportedException();
        public override void Prepare() { }
        protected override DbParameter CreateDbParameter() => new FakeDbParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => ReaderFactory?.Invoke() ?? throw new InvalidOperationException("ReaderFactory no configurada.");

        public object? GetParameterValue(string parameterName)
        {
            foreach (DbParameter parameter in _parameters)
            {
                if (parameter.ParameterName == parameterName)
                {
                    return parameter.Value;
                }
            }
            return null;
        }
    }

    private sealed class FakeDbParameter : DbParameter
    {
        public override DbType DbType { get; set; }
        public override ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public override bool IsNullable { get; set; }
        public override string ParameterName { get; set; } = string.Empty;
        public override string SourceColumn { get; set; } = string.Empty;
        public override object? Value { get; set; }
        public override bool SourceColumnNullMapping { get; set; }
        public override int Size { get; set; }
        public override void ResetDbType() { }
    }

    private sealed class FakeParameterCollection : DbParameterCollection
    {
        private readonly List<DbParameter> _items = new();

        public override int Count => _items.Count;
        public override object SyncRoot => ((System.Collections.ICollection)_items).SyncRoot;

        public override int Add(object value)
        {
            _items.Add((DbParameter)value);
            return _items.Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
            {
                Add(value!);
            }
        }

        public override void Clear() => _items.Clear();
        public override bool Contains(object value) => _items.Contains((DbParameter)value);
        public override bool Contains(string value) => _items.Any(p => p.ParameterName == value);
        public override void CopyTo(Array array, int index) => ((System.Collections.ICollection)_items).CopyTo(array, index);
        public override IEnumerator GetEnumerator() => _items.GetEnumerator();
        public override int IndexOf(object value) => _items.IndexOf((DbParameter)value);
        public override int IndexOf(string parameterName) => _items.FindIndex(p => p.ParameterName == parameterName);
        public override void Insert(int index, object value) => _items.Insert(index, (DbParameter)value);
        public override void Remove(object value) => _items.Remove((DbParameter)value);
        public override void RemoveAt(int index) => _items.RemoveAt(index);
        public override void RemoveAt(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index >= 0) RemoveAt(index);
        }

        protected override DbParameter GetParameter(int index) => _items[index];
        protected override DbParameter GetParameter(string parameterName)
            => _items.First(p => p.ParameterName == parameterName);
        protected override void SetParameter(int index, DbParameter value) => _items[index] = value;
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            if (index >= 0) _items[index] = value;
            else _items.Add(value);
        }
    }
}
