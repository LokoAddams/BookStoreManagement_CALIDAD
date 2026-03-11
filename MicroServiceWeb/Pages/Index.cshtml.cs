using Microsoft.AspNetCore.Mvc.RazorPages;
using MicroServiceWeb.External.Http;
using System.Threading;
using System.Linq;

namespace LibraryWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IClientsApiClient _clients;
        private readonly IProductsApiClient _products;
        private readonly IDistributorsApiClient _distributors;

        public int Clientes { get; private set; }
        public int Productos { get; private set; }
        public int Distribuidores { get; private set; }

        public IndexModel(IClientsApiClient clients, IProductsApiClient products, IDistributorsApiClient distributors)
        {
            _clients = clients;
            _products = products;
            _distributors = distributors;
        }

        public async Task OnGetAsync(CancellationToken ct)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var clientes = await _clients.GetAllAsync(ct);
                var productos = await _products.GetAllAsync(ct);
                var distribuidores = await _distributors.GetAllAsync(ct);
                Clientes = clientes.Count;
                Productos = productos.Count;
                Distribuidores = distribuidores.Count;
            }
        }
    }
}
