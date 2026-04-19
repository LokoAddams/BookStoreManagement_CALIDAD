using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;
using System.Linq;
using Xunit;

namespace MicroServiceSales.Tests;

public class SalesValidationTests
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

    [Fact]
    public void Validate_Should_Return_NoErrors_For_A_Valid_Sale()
    {
        var sale = CreateValidSale();

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
        var sale = CreateValidSale();
        ApplyInvalidSaleScenario(sale, scenario);

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(expectedField, errors[0].Field);
    }

    [Theory]
    [InlineData(-10, -10, nameof(Sale.Subtotal))]
    [InlineData(10, -10, nameof(Sale.Total))]
    public void Validate_Should_Return_Amount_Error_When_Amounts_Are_Negative(decimal subtotal, decimal total, string expectedField)
    {
        var sale = CreateValidSale();
        sale.Subtotal = subtotal;
        sale.Total = total;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Contains(errors, error => error.Field == expectedField);
    }

    [Fact]
    public void ValidateDetail_Should_Return_NoErrors_For_A_Valid_Detail()
    {
        var detail = CreateValidDetail();

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("SaleIdEmpty", 1, nameof(SaleDetail.SaleId))]
    [InlineData("ProductIdEmpty", 1, nameof(SaleDetail.ProductId))]
    [InlineData("QuantityZero", 1, nameof(SaleDetail.Quantity))]
    [InlineData("UnitPriceNegative", 2, nameof(SaleDetail.UnitPrice) + "|" + nameof(SaleDetail.Subtotal))]
    [InlineData("SubtotalNegative", 2, nameof(SaleDetail.Subtotal) + "|" + nameof(SaleDetail.Subtotal))]
    [InlineData("SubtotalMismatch", 1, nameof(SaleDetail.Subtotal))]
    public void ValidateDetail_Should_Return_Expected_Errors_For_Invalid_Scenarios(string scenario, int expectedCount, string expectedFields)
    {
        var detail = CreateValidDetail();
        ApplyInvalidDetailScenario(detail, scenario);

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Equal(expectedCount, errors.Count);

        var expectedFieldList = expectedFields.Split('|');
        foreach (var expectedField in expectedFieldList.Distinct())
        {
            var expectedOccurrences = expectedFieldList.Count(field => field == expectedField);
            var actualOccurrences = errors.Count(error => error.Field == expectedField);
            Assert.Equal(expectedOccurrences, actualOccurrences);
        }
    }

    [Fact]
    public void ValidateAll_Should_Return_NoErrors_For_Valid_Sale_And_Valid_Details()
    {
        var sale = CreateValidSale();
        sale.Details.Add(CreateValidDetail());

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAll_Should_Return_Sale_Errors_And_Ignore_Valid_Details_When_Sale_Is_Invalid()
    {
        var sale = CreateValidSale();
        sale.ClientId = Guid.Empty;
        sale.Details.Add(CreateValidDetail());

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Contains(errors, error => error.Field == nameof(Sale.ClientId));
        Assert.DoesNotContain(errors, error => error.Field == nameof(SaleDetail.SaleId));
    }

    [Fact]
    public void ValidateAll_Should_Return_NoErrors_When_Details_Are_Null_And_Sale_Is_Valid()
    {
        var sale = CreateValidSale();
        sale.Details = null!;

        var errors = SalesValidation.ValidateAll(sale).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAll_Should_Return_Duplicate_Product_Error_When_Details_Repeat_ProductId()
    {
        var sale = CreateValidSale();
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

    private static Sale CreateValidSale()
    {
        return new Sale
        {
            ClientId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SaleDate = DateTimeOffset.UtcNow,
            Subtotal = 10m,
            Total = 10m,
            Status = "PENDING",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static SaleDetail CreateValidDetail()
    {
        return new SaleDetail
        {
            SaleId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            UnitPrice = 10m,
            Subtotal = 20m
        };
    }

    private static void ApplyInvalidSaleScenario(Sale sale, string scenario)
    {
        switch (scenario)
        {
            case "ClientIdEmpty":
                sale.ClientId = Guid.Empty;
                break;
            case "UserIdEmpty":
                sale.UserId = Guid.Empty;
                break;
            case "SaleDateDefault":
                sale.SaleDate = default;
                break;
            case "StatusEmpty":
                sale.Status = string.Empty;
                break;
            case "StatusTooLong":
                sale.Status = "STRING_MUY_LARGO_AQUI_XXXX";
                break;
            case "StatusInvalid":
                sale.Status = "ESTADO_INVALIDADO";
                break;
            case "CancelledAtMissing":
                sale.CancelledBy = Guid.NewGuid();
                break;
            case "CancelledByMissing":
                sale.CancelledAt = DateTimeOffset.UtcNow;
                break;
            case "TotalMismatch":
                sale.Total = 100m;
                sale.Subtotal = 90m;
                break;
            case "CreatedAtDefault":
                sale.CreatedAt = default;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }
    }

    private static void ApplyInvalidDetailScenario(SaleDetail detail, string scenario)
    {
        switch (scenario)
        {
            case "SaleIdEmpty":
                detail.SaleId = Guid.Empty;
                break;
            case "ProductIdEmpty":
                detail.ProductId = Guid.Empty;
                break;
            case "QuantityZero":
                detail.Subtotal = 0m;
                detail.Quantity = 0;
                break;
            case "UnitPriceNegative":
                detail.Subtotal = -5m;
                detail.Quantity = 1;
                detail.UnitPrice = -5m;
                break;
            case "SubtotalNegative":
                detail.Subtotal = -10m;
                break;
            case "SubtotalMismatch":
                detail.Quantity = 2;
                detail.UnitPrice = 10m;
                detail.Subtotal = 15m;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null);
        }
    }
}