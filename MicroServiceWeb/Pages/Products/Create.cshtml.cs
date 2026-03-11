using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading;

namespace LibraryWeb.Pages.Products
{
    public class CreateModel : PageModel
    {
        private readonly IProductsApiClient _api;

        [BindProperty]
        public ProductCreateDto Product { get; set; } = new();
        
        public List<SelectListItem> Categories { get; set; } = new();

        public CreateModel(IProductsApiClient api)
        {
            _api = api;
        }

        public async Task OnGetAsync(CancellationToken ct)
        {
            // Precio inicial 1 sin separadores (input number lo maneja)
            Product.Price = 1m;
            await LoadCategoriesAsync(ct);
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // Normalizar espacios básicos
            Product.Name = Product.Name?.Trim() ?? string.Empty;
            Product.Description = Product.Description?.Trim();

            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(ct);
                return Page();
            }

            var result = await _api.CreateAsync(Product, ct);
            if (!result.Success)
            {
                foreach (var kv in result.Errors)
                {
                    var key = kv.Key switch
                    {
                        "name" => "Product.Name",
                        "description" => "Product.Description",
                        "categoryId" => "Product.CategoryId",
                        "price" => "Product.Price",
                        "stock" => "Product.Stock",
                        _ => $"Product.{kv.Key}"
                    };
                    foreach (var msg in kv.Value) ModelState.AddModelError(key, msg);
                }
                if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, "No se pudo crear el producto.");
                await LoadCategoriesAsync(ct);
                return Page();
            }
            return RedirectToPage("Index");
        }

        private async Task LoadCategoriesAsync(CancellationToken ct)
        {
            var categories = await _api.GetCategoriesAsync(ct);
            Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
            // No insertamos opción por defecto aquí; la vista ya la muestra
        }
    }
}
