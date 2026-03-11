using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibraryWeb.Pages.Sales
{
    public class IndexModel : PageModel
    {
        private readonly ISalesApiClient _api;
        private readonly IClientsApiClient _clientsApi;
        private readonly IUsersApiClient _usersApi;
        private readonly IConfiguration _config;

        public List<SaleDto> Sales { get; set; } = new();

        [BindProperty]
        public int Page { get; set; } = 1;

        [BindProperty]
        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

        public string SalesApiUrl { get; private set; } = string.Empty;
        public string ReportsApiUrl { get; private set; } = string.Empty;

        public IndexModel(ISalesApiClient api, IClientsApiClient clientsApi, IUsersApiClient usersApi, IConfiguration config)
        {
            _api = api;
            _clientsApi = clientsApi;
            _usersApi = usersApi;
            _config = config;
        }

        public async Task OnGetAsync(CancellationToken ct, int? pageNumber, int? pageSize)
        {
            // Cargar URLs de microservicios
            SalesApiUrl = _config["Microservices:SalesService"] ?? "http://localhost:5126";
            ReportsApiUrl = _config["Microservices:ReportsService"] ?? "http://localhost:5129";

            if (pageSize.HasValue && pageSize.Value > 0) PageSize = pageSize.Value;
            if (pageNumber.HasValue && pageNumber.Value > 0) Page = pageNumber.Value;
            if (PageSize < 1) PageSize = 10;
            if (PageSize > 100) PageSize = 100;

            var paged = await _api.GetPagedAsync(Page, PageSize, ct);
            
            Page = paged.Page;
            PageSize = paged.PageSize;
            TotalItems = paged.TotalItems;
            
            // Enriquecer datos de ventas con información de clientes y usuarios
            var salesList = paged.Items.ToList();
            await EnrichSalesDataAsync(salesList, ct);
            Sales = salesList;
        }

        private async Task EnrichSalesDataAsync(List<SaleDto> sales, CancellationToken ct)
        {
            // Obtener IDs únicos de clientes y usuarios
            var clientIds = sales.Where(s => s.ClientId != Guid.Empty && string.IsNullOrEmpty(s.ClientName))
                                 .Select(s => s.ClientId).Distinct().ToList();
            var userIds = sales.Where(s => s.UserId != Guid.Empty && string.IsNullOrEmpty(s.UserName))
                               .Select(s => s.UserId).Distinct().ToList();

            // Obtener información de clientes
            var clientsDict = new Dictionary<Guid, ClientDto>();
            foreach (var clientId in clientIds)
            {
                try
                {
                    var client = await _clientsApi.GetByIdAsync(clientId, ct);
                    if (client != null)
                        clientsDict[clientId] = client;
                }
                catch { /* Ignorar errores individuales */ }
            }

            // Obtener información de usuarios
            var usersDict = new Dictionary<Guid, UserDto>();
            foreach (var userId in userIds)
            {
                try
                {
                    var user = await _usersApi.GetByIdAsync(userId, ct);
                    if (user != null)
                        usersDict[userId] = user;
                }
                catch { /* Ignorar errores individuales */ }
            }

            // Actualizar las ventas con la información obtenida
            for (int i = 0; i < sales.Count; i++)
            {
                var sale = sales[i];
                string? clientName = sale.ClientName;
                string? clientCi = sale.ClientCi;
                string? userName = sale.UserName;

                // Enriquecer datos del cliente si no están presentes
                if (string.IsNullOrEmpty(clientName) && clientsDict.TryGetValue(sale.ClientId, out var client))
                {
                    clientName = $"{client.FirstName} {client.LastName}".Trim();
                    clientCi = client.Ci;
                }

                // Enriquecer datos del usuario si no están presentes
                if (string.IsNullOrEmpty(userName) && usersDict.TryGetValue(sale.UserId, out var user))
                {
                    userName = user.Username;
                }

                // Crear nuevo DTO con los datos enriquecidos
                sales[i] = new SaleDto(
                    sale.Id,
                    sale.ClientId,
                    clientName,
                    clientCi,
                    sale.UserId,
                    userName,
                    sale.SaleDate,
                    sale.Subtotal,
                    sale.Total,
                    sale.Status,
                    sale.Details
                );
            }
        }
    }
}
