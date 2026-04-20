using MicroServiceSales.Domain.Models;

namespace MicroServiceSales.Domain.UnitTest;

internal static class SalesValidationTestData
{
    public static Sale CreateValidSale()
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

    public static SaleDetail CreateValidDetail()
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

    public static void ApplyInvalidSaleScenario(Sale sale, string scenario)
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

    public static void ApplyInvalidDetailScenario(SaleDetail detail, string scenario)
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
