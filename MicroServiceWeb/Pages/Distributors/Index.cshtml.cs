using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServiceDistributors.Domain.Models;
using System.Linq;
using System.Threading;

namespace LibraryWeb.Pages.Distributors
{
    public class IndexModel : PageModel
    {
        private readonly IDistributorsApiClient _api;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<DistributorDto> Distributors { get; set; } = new();
        public IndexModel(IDistributorsApiClient api) { _api = api; }
        public async Task OnGetAsync(CancellationToken ct, int? pageNumber, int? pageSize)
        {

            if (pageSize.HasValue && pageSize.Value > 0)
                PageSize = pageSize.Value;
            if (pageNumber.HasValue && pageNumber.Value > 0)
                Page = pageNumber.Value;

            if (PageSize < 1) PageSize = 10;
            if (PageSize > 100) PageSize = 100;
            var all = await _api.GetPagedAsync(Page, PageSize, ct);
            TotalItems = all.TotalItems;
            if (TotalItems == 0)
            {
                Distributors = new List<DistributorDto>();
                Page = 1;
                return;
            }
            var maxPage = (int)Math.Ceiling((double)TotalItems / PageSize);
            if (Page > maxPage) Page = maxPage;

            Distributors = all.Items.ToList();
        }    
        public IActionResult OnPostEdit(Guid id) { TempData["EditDistributorId"] = id; return RedirectToPage("Edit"); }
        public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct)
        {
            var ok = await _api.DeleteAsync(id, ct);
            if (!ok) TempData["ErrorMessage"] = "No se pudo eliminar"; 
            return RedirectToPage(new { pageNumber = Page, pageSize = PageSize });
        }
    }
}
