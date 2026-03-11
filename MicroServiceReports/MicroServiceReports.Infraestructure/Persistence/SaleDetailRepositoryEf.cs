namespace MicroServiceReports.Infraestructure.Persistence
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using MicroServiceReports.Domain.Models;
    using MicroServiceReports.Domain.Ports;

    public class SaleDetailRepositoryEf : ISaleDetailRepository
    {
        private readonly MicroServiceReportsDbContext _dbContext;
        private readonly ILogger<SaleDetailRepositoryEf> _logger;

        public SaleDetailRepositoryEf(MicroServiceReportsDbContext dbContext, ILogger<SaleDetailRepositoryEf> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveAsync(SaleDetailRecord record)
        {
            record.SaleId = record.SaleId?.Trim().ToLowerInvariant() ?? string.Empty;
            
            _logger.LogInformation("Saving SaleDetailRecord for SaleId={SaleId}, ProductId={ProductId}", 
                record.SaleId, record.ProductId);
            
            _dbContext.SaleDetailRecords.Add(record);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("SaleDetailRecord saved successfully. SaleId={SaleId}, ProductId={ProductId}", 
                record.SaleId, record.ProductId);
        }

        public async Task SaveManyAsync(IEnumerable<SaleDetailRecord> records)
        {
            foreach (var record in records)
            {
                record.SaleId = record.SaleId?.Trim().ToLowerInvariant() ?? string.Empty;
            }
            
            _logger.LogInformation("Saving multiple SaleDetailRecords");
            
            await _dbContext.SaleDetailRecords.AddRangeAsync(records);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("SaleDetailRecords saved successfully");
        }

        public async Task<IEnumerable<SaleDetailRecord>> GetBySaleIdAsync(string saleId)
        {
            var normalizedSaleId = saleId?.Trim().ToLowerInvariant() ?? string.Empty;
            
            _logger.LogInformation("Getting SaleDetailRecords for SaleId={SaleId}", normalizedSaleId);
            
            return await _dbContext.SaleDetailRecords
                .Where(x => x.SaleId.ToLower() == normalizedSaleId)
                .ToListAsync();
        }
    }
}
