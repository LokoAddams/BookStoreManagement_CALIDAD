using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryWeb.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly IProductsApiClient _api;

        public List<ProductDto> Products { get; set; } = new();
        [BindProperty]
        public int Page { get; set; } = 1;
        [BindProperty]
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalItems / PageSize);

        public IndexModel(IProductsApiClient api) { _api = api; }

        public async Task OnGetAsync(CancellationToken ct, int? pageNumber, int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value > 0) PageSize = pageSize.Value;
            if (pageNumber.HasValue && pageNumber.Value > 0) Page = pageNumber.Value;
            if (PageSize < 1) PageSize = 10; if (PageSize > 100) PageSize = 100;

            var paged = await _api.GetPagedAsync(Page, PageSize, ct);
            // Asegurar que backend corrige page/pageSize si salen de rango
            Page = paged.Page; PageSize = paged.PageSize; TotalItems = paged.TotalItems;
            Products = paged.Items.ToList();

            // Fallback UI: si vienen productos sin CategoryName, completar desde /api/categories
            if (Products.Any(p => p.CategoryId != Guid.Empty && string.IsNullOrWhiteSpace(p.CategoryName)))
            {
                var cats = await _api.GetCategoriesAsync(ct);
                var dict = cats.ToDictionary(c => c.Id, c => c.Name);
                for (int i = 0; i < Products.Count; i++)
                {
                    var p = Products[i];
                    if (p.CategoryId != Guid.Empty && string.IsNullOrWhiteSpace(p.CategoryName) && dict.TryGetValue(p.CategoryId, out var name))
                    {
                        Products[i] = new ProductDto(p.Id, p.Name, p.Description, p.CategoryId, name, p.Price, p.Stock);
                    }
                }
            }
        }

        public IActionResult OnPostEdit(Guid id)
        {
            TempData["EditProductId"] = id;
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
