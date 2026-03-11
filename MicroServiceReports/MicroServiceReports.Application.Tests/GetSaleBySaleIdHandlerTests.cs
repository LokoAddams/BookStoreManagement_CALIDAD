using System.Threading.Tasks;
using Moq;
using Xunit;
using MicroServiceReports.Application.UseCases;
using MicroServiceReports.Domain.Ports;
using MicroServiceReports.Domain.Models;

namespace MicroServiceReports.Application.Tests
{
    public class GetSaleBySaleIdHandlerTests
    {
        [Fact]
        public async Task HandleAsync_ReturnsRecord_WhenRepositoryHasRecord()
        {
            var mockRepo = new Mock<ISaleEventRepository>();
            var expected = new SaleEventRecord { Id = System.Guid.NewGuid(), SaleId = "123", Payload = "{}" };
            mockRepo.Setup(r => r.GetBySaleIdAsync("123")).ReturnsAsync(expected);

            var handler = new GetSaleBySaleIdHandler(mockRepo.Object);

            var result = await handler.HandleAsync("123");

            Assert.NotNull(result);
            Assert.Equal(expected.SaleId, result!.SaleId);
        }

        [Fact]
        public async Task HandleAsync_ReturnsNull_WhenNotFound()
        {
            var mockRepo = new Mock<ISaleEventRepository>();
            mockRepo.Setup(r => r.GetBySaleIdAsync("999")).ReturnsAsync((SaleEventRecord?)null);

            var handler = new GetSaleBySaleIdHandler(mockRepo.Object);

            var result = await handler.HandleAsync("999");

            Assert.Null(result);
        }
    }
}

