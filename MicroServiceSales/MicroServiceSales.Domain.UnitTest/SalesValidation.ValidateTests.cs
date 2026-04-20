using System.Linq;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;
using Xunit;

namespace MicroServiceSales.Domain.UnitTest;

public class SalesValidationValidateTests
{
    [Fact]
    public void Validate_Should_Return_NoErrors_For_A_Valid_Sale()
    {
        var sale = SalesValidationTestData.CreateValidSale();

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("ClientIdEmpty", nameof(Sale.ClientId))]
    [InlineData("UserIdEmpty", nameof(Sale.UserId))]
    [InlineData("SaleDateDefault", nameof(Sale.SaleDate))]
    [InlineData("StatusEmpty", nameof(Sale.Status))]
    [InlineData("StatusTooLong", nameof(Sale.Status))]
    [InlineData("StatusInvalid", nameof(Sale.Status))]
    [InlineData("CancelledAtMissing", nameof(Sale.CancelledAt))]
    [InlineData("CancelledByMissing", nameof(Sale.CancelledBy))]
    [InlineData("TotalMismatch", nameof(Sale.Total))]
    [InlineData("CreatedAtDefault", nameof(Sale.CreatedAt))]
    public void Validate_Should_Return_Expected_Single_Error_For_Invalid_Scenarios(string scenario, string expectedField)
    {
        var sale = SalesValidationTestData.CreateValidSale();
        SalesValidationTestData.ApplyInvalidSaleScenario(sale, scenario);

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(expectedField, errors[0].Field);
    }

    [Theory]
    [InlineData(-10, -10, nameof(Sale.Subtotal))]
    [InlineData(10, -10, nameof(Sale.Total))]
    public void Validate_Should_Return_Amount_Error_When_Amounts_Are_Negative(decimal subtotal, decimal total, string expectedField)
    {
        var sale = SalesValidationTestData.CreateValidSale();
        sale.Subtotal = subtotal;
        sale.Total = total;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Contains(errors, error => error.Field == expectedField);
    }
}
