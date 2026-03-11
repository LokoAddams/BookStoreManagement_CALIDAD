using ServiceClients.Domain.Models;
using ServiceClients.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using MicroServiceWeb.External.Http;
using System.Threading;

namespace LibraryWeb.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly IClientsApiClient _api;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<ClientDto> Clients { get; set; } = new();

        public IndexModel(IClientsApiClient api)
        {
            _api = api;
        }

        public async Task OnGetAsync(CancellationToken ct, int? pageNumber, int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value > 0)
                PageSize = pageSize.Value;
            if (pageNumber.HasValue && pageNumber.Value > 0)
                Page = pageNumber.Value;

            if (PageSize < 1) PageSize = 10;
            if (PageSize > 100) PageSize = 100;

            var paged = await _api.GetPagedAsync(Page, PageSize, ct);
            TotalItems = paged.TotalItems;
            Page = paged.Page;
            PageSize = paged.PageSize;
            Clients = paged.Items.ToList();
        }

        public IActionResult OnPostEdit(Guid id)
        {
            TempData["EditClientId"] = id;
            return RedirectToPage("Edit");
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
        {
            var ok = await _api.DeleteAsync(id, ct);
            if (!ok) TempData["ErrorMessage"] = "No se pudo eliminar";
            return RedirectToPage(new { pageNumber = Page, pageSize = PageSize });
        }
    }
}
