using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Linq;

namespace LibraryWeb.Pages.Distributors
{
    public class CreateModel : PageModel
    {
        private readonly IDistributorsApiClient _api;
        [BindProperty] public DistributorCreateDto Distributor { get; set; } = new();
        public CreateModel(IDistributorsApiClient api) { _api = api; }
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            Distributor.Name = Distributor.Name?.Trim() ?? string.Empty;
            Distributor.ContactEmail = Distributor.ContactEmail?.Trim();
            Distributor.Phone = Distributor.Phone?.Trim();
            Distributor.Address = Distributor.Address?.Trim();

            if (!ModelState.IsValid) return Page();
            var result = await _api.CreateAsync(Distributor, ct);
            if (!result.Success)
            {
                foreach (var kv in result.Errors)
                {
                    var key = kv.Key switch
                    {
                        "name" => "Distributor.Name",
                        "contactEmail" => "Distributor.ContactEmail",
                        "phone" => "Distributor.Phone",
                        "address" => "Distributor.Address",
                        _ => $"Distributor.{kv.Key}"
                    };
                    foreach (var msg in kv.Value) ModelState.AddModelError(key, msg);
                }
                if (result.Errors.Count == 0) ModelState.AddModelError(string.Empty, "No se pudo crear el distribuidor.");
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}
