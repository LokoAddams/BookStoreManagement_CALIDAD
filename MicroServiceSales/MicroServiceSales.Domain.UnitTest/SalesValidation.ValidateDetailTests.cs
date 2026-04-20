using System.Linq;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;
using Xunit;

namespace MicroServiceSales.Domain.UnitTest;

public class SalesValidationValidateDetailTests
{
    [Fact]
    public void ValidateDetail_Should_Return_NoErrors_For_A_Valid_Detail()
    {
        var detail = SalesValidationTestData.CreateValidDetail();

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
        var detail = SalesValidationTestData.CreateValidDetail();
        SalesValidationTestData.ApplyInvalidDetailScenario(detail, scenario);

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
}
