namespace MicroServiceReports.Infraestructure.Persistence
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using MicroServiceReports.Domain.Models;
    using MicroServiceReports.Domain.Ports;

    public class SaleEventRepositoryEf : ISaleEventRepository
    {
        private readonly MicroServiceReportsDbContext _dbContext;
        private readonly ILogger<SaleEventRepositoryEf> _logger;

        public SaleEventRepositoryEf(MicroServiceReportsDbContext dbContext, ILogger<SaleEventRepositoryEf> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveAsync(SaleEventRecord record)
        {
            // Normalizar SaleId a minúsculas y sin espacios
            record.SaleId = record.SaleId?.Trim().ToLowerInvariant() ?? string.Empty;
            
            _logger.LogInformation("Saving SaleEventRecord with SaleId={SaleId}", record.SaleId);
            _dbContext.SaleEventRecords.Add(record);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("SaleEventRecord saved successfully. SaleId={SaleId}", record.SaleId);
        }

        public async Task<SaleEventRecord?> GetBySaleIdAsync(string saleId)
        {
            // Normalizar el SaleId de búsqueda
            var normalizedSaleId = saleId?.Trim().ToLowerInvariant() ?? string.Empty;
            
            _logger.LogInformation("Searching for SaleEventRecord with SaleId={SaleId} (normalized={NormalizedSaleId})", 
                saleId, normalizedSaleId);
            
            // Buscar el registro con comparación case-insensitive
            var record = await _dbContext.SaleEventRecords
                .FirstOrDefaultAsync(x => x.SaleId.ToLower() == normalizedSaleId);
            
            if (record == null)
            {
                // Log todos los SaleIds para depuración
                var allRecords = await _dbContext.SaleEventRecords.Select(x => x.SaleId).ToListAsync();
                _logger.LogWarning("SaleEventRecord not found. SaleId={SaleId}. Available SaleIds: [{AvailableSaleIds}]", 
                    saleId, 
                    string.Join(", ", allRecords));
            }
            else
            {
                _logger.LogInformation("SaleEventRecord found. SaleId={SaleId}", saleId);
            }
            
            return record;
        }

        public async Task<IEnumerable<SaleEventRecord>> GetAllAsync()
        {
            return await _dbContext.SaleEventRecords
                .OrderByDescending(x => x.ReceivedAt)
                .ToListAsync();
        }
    }
}
