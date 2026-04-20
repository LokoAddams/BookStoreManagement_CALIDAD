using System.Linq;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;
using Xunit;

namespace MicroServiceSales.Domain.UnitTest;

public class SalesValidationValidateAllTests
{
    [Fact]
    public void ValidateAll_Should_Return_NoErrors_For_Valid_Sale_And_Valid_Details()
    {
        var sale = SalesValidationTestData.CreateValidSale();
        sale.Details.Add(SalesValidationTestData.CreateValidDetail());

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAll_Should_Return_Sale_Errors_And_Ignore_Valid_Details_When_Sale_Is_Invalid()
    {
        var sale = SalesValidationTestData.CreateValidSale();
        sale.ClientId = Guid.Empty;
        sale.Details.Add(SalesValidationTestData.CreateValidDetail());

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Contains(errors, error => error.Field == nameof(Sale.ClientId));
        Assert.DoesNotContain(errors, error => error.Field == nameof(SaleDetail.SaleId));
    }

    [Fact]
    public void ValidateAll_Should_Return_NoErrors_When_Details_Are_Null_And_Sale_Is_Valid()
    {
        var sale = SalesValidationTestData.CreateValidSale();
        sale.Details = null!;

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAll_Should_Return_Duplicate_Product_Error_When_Details_Repeat_ProductId()
    {
        var sale = SalesValidationTestData.CreateValidSale();
        var duplicateProductId = Guid.NewGuid();

        sale.Details.Add(new SaleDetail
        {
            SaleId = Guid.NewGuid(),
            ProductId = duplicateProductId,
            Quantity = 2,
            UnitPrice = 10m,
            Subtotal = 20m
        });

        sale.Details.Add(new SaleDetail
        {
            SaleId = Guid.NewGuid(),
            ProductId = duplicateProductId,
            Quantity = 1,
            UnitPrice = 10m,
            Subtotal = 10m
        });

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Contains(errors, error => error.Field == "Details");
    }
}
