using MicroServiceUsers.Domain.Results;
using MicroServiceUsers.Domain.Validations;

namespace MicroServiceUsers.Domain.UnitTest;

public class ResultTests
{
    [Theory]
    [InlineData("valor-ok")]
    public void Ok_Scenarios_ReturnExpectedResult(string value)
    {
        var result = Result<string>.Ok(value);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
        Assert.NotNull(result.Errors);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Fail_Scenarios_ReturnExpectedResult(int testCase)
    {
        ValidationError[]? errors = testCase switch
        {
            1 =>
            [
                new ValidationError("Field1", "Error 1"),
                new ValidationError("Field2", "Error 2")
            ],
            2 => null,
            _ => throw new ArgumentOutOfRangeException(nameof(testCase), testCase, "Caso no soportado")
        };

        var result = errors is null
            ? Result<string>.Fail(null!)
            : Result<string>.Fail(errors);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.NotNull(result.Errors);

        if (testCase == 1)
        {
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.Field == "Field1" && e.Message == "Error 1");
            Assert.Contains(result.Errors, e => e.Field == "Field2" && e.Message == "Error 2");
        }
        else
        {
            Assert.Empty(result.Errors);
        }
    }
}
