using System.Data;
using MicroServiceSales.Infrastructure.Repositories;

namespace MicroServiceSales.Infrastructure.UnitTests;

public class SalesRepositoryMapSaleDetailTests
{
    [Fact]
    public void MapSaleDetail_Should_Map_All_Fields_From_DataRecord()
    {
        var id = Guid.NewGuid();
        var saleId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        const int quantity = 3;
        const decimal unitPrice = 12.50m;
        const decimal subtotal = 37.50m;

        var table = new DataTable();
        table.Columns.Add("id", typeof(Guid));
        table.Columns.Add("sale_id", typeof(Guid));
        table.Columns.Add("product_id", typeof(Guid));
        table.Columns.Add("quantity", typeof(int));
        table.Columns.Add("unit_price", typeof(decimal));
        table.Columns.Add("subtotal", typeof(decimal));
        table.Rows.Add(id, saleId, productId, quantity, unitPrice, subtotal);

        using var reader = table.CreateDataReader();
        Assert.True(reader.Read());

        var detail = SalesRepository.MapSaleDetailRecord(reader);

        Assert.Equal(id, detail.Id);
        Assert.Equal(saleId, detail.SaleId);
        Assert.Equal(productId, detail.ProductId);
        Assert.Equal(quantity, detail.Quantity);
        Assert.Equal(unitPrice, detail.UnitPrice);
        Assert.Equal(subtotal, detail.Subtotal);
    }
}
