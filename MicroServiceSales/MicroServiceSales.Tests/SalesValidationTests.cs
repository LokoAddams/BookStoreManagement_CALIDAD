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

    [Fact]
    public void Validate_Should_Return_Client_Error_When_ClientId_Is_Empty()
    {
        var sale = CreateValidSale();
        sale.ClientId = Guid.Empty;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.ClientId), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_User_Error_When_UserId_Is_Empty()
    {
        var sale = CreateValidSale();
        sale.UserId = Guid.Empty;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.UserId), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_SaleDate_Error_When_Date_Is_Default()
    {
        var sale = CreateValidSale();
        sale.SaleDate = default;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.SaleDate), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_Subtotal_Error_When_Subtotal_Is_Negative()
    {
        var sale = CreateValidSale();
        sale.Subtotal = -10m;
        sale.Total = -10m;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Contains(errors, error => error.Field == nameof(Sale.Subtotal));
    }

    [Fact]
    public void Validate_Should_Return_Total_Error_When_Total_Is_Negative()
    {
        var sale = CreateValidSale();
        sale.Total = -10m;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Contains(errors, error => error.Field == nameof(Sale.Total));
    }

    [Fact]
    public void Validate_Should_Return_Status_Error_When_Status_Is_Empty()
    {
        var sale = CreateValidSale();
        sale.Status = string.Empty;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.Status), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_Status_Length_Error_When_Status_Exceeds_Maximum()
    {
        var sale = CreateValidSale();
        sale.Status = "STRING_MUY_LARGO_AQUI_XXXX";

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.Status), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_Status_Invalid_Error_When_Status_Is_Not_Allowed()
    {
        var sale = CreateValidSale();
        sale.Status = "ESTADO_INVALIDADO";

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.Status), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_CancelledAt_Error_When_CancelledBy_Exists_And_Date_Is_Missing()
    {
        var sale = CreateValidSale();
        sale.CancelledBy = Guid.NewGuid();

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.CancelledAt), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_CancelledBy_Error_When_CancelledAt_Exists_And_User_Is_Missing()
    {
        var sale = CreateValidSale();
        sale.CancelledAt = DateTimeOffset.UtcNow;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.CancelledBy), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_Total_Equality_Error_When_Total_Does_Not_Match_Subtotal()
    {
        var sale = CreateValidSale();
        sale.Total = 100m;
        sale.Subtotal = 90m;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.Total), errors[0].Field);
    }

    [Fact]
    public void Validate_Should_Return_CreatedAt_Error_When_CreatedAt_Is_Default()
    {
        var sale = CreateValidSale();
        sale.CreatedAt = default;

        var errors = SalesValidation.Validate(sale).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(Sale.CreatedAt), errors[0].Field);
    }

    [Fact]
    public void ValidateDetail_Should_Return_NoErrors_For_A_Valid_Detail()
    {
        var detail = CreateValidDetail();

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateDetail_Should_Return_SaleId_Error_When_SaleId_Is_Empty()
    {
        var detail = CreateValidDetail();
        detail.SaleId = Guid.Empty;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(SaleDetail.SaleId), errors[0].Field);
    }

    [Fact]
    public void ValidateDetail_Should_Return_ProductId_Error_When_ProductId_Is_Empty()
    {
        var detail = CreateValidDetail();
        detail.ProductId = Guid.Empty;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(SaleDetail.ProductId), errors[0].Field);
    }

    [Fact]
    public void ValidateDetail_Should_Return_Quantity_Error_When_Quantity_Is_Zero()
    {
        var detail = CreateValidDetail();
        detail.Subtotal = 0m;
        detail.Quantity = 0;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(SaleDetail.Quantity), errors[0].Field);
    }

    [Fact]
    public void ValidateDetail_Should_Return_UnitPrice_Error_When_UnitPrice_Is_Negative()
    {
        var detail = CreateValidDetail();
        detail.Subtotal = -5m;
        detail.Quantity = 1;
        detail.UnitPrice = -5m;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, error => error.Field == nameof(SaleDetail.UnitPrice));
        Assert.Contains(errors, error => error.Field == nameof(SaleDetail.Subtotal));
    }

    [Fact]
    public void ValidateDetail_Should_Return_Subtotal_Error_When_Subtotal_Is_Negative()
    {
        var detail = CreateValidDetail();
        detail.Subtotal = -10m;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Equal(2, errors.Count);
        Assert.All(errors, error => Assert.Equal(nameof(SaleDetail.Subtotal), error.Field));
    }

    [Fact]
    public void ValidateDetail_Should_Return_Subtotal_Error_When_Subtotal_Does_Not_Match_Quantity_Times_UnitPrice()
    {
        var detail = CreateValidDetail();
        detail.Quantity = 2;
        detail.UnitPrice = 10m;
        detail.Subtotal = 15m;

        var errors = SalesValidation.ValidateDetail(detail).ToList();

        Assert.Single(errors);
        Assert.Equal(nameof(SaleDetail.Subtotal), errors[0].Field);
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
}