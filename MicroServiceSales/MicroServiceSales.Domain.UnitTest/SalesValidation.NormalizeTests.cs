using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;
using Xunit;

namespace MicroServiceSales.Domain.UnitTest;

public class SalesValidationNormalizeTests
{
    [Fact]
    public void Normalize_Should_Convert_Status_To_Uppercase_And_Round_Decimals()
    {
        var sale = new Sale
        {
            Status = " pending ",
            Subtotal = 10.555m,
            Total = 10.555m
        };

        SalesValidation.Normalize(sale);

        Assert.Equal("PENDING", sale.Status);
        Assert.Equal(10.56m, sale.Subtotal);
        Assert.Equal(10.56m, sale.Total);
    }
}
