using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Linq;

namespace LibraryWeb.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly IClientsApiClient _api;
        public CreateModel(IClientsApiClient api) { _api = api; }
        [BindProperty] public ClientCreateDto Client { get; set; } = new();
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // Normalizar espacios
            Client.FirstName = Client.FirstName?.Trim() ?? string.Empty;
            Client.LastName = Client.LastName?.Trim() ?? string.Empty;
            Client.Ci = Client.Ci?.Trim() ?? string.Empty;
            Client.Email = Client.Email?.Trim();
            Client.Phone = Client.Phone?.Trim();
            Client.Address = Client.Address?.Trim();

            if (!ModelState.IsValid) return Page();
            var result = await _api.CreateAsync(Client, ct);
            if (!result.Success)
            {
                foreach (var kv in result.Errors)
                {
                    var key = kv.Key switch
                    {
                        "firstName" => "Client.FirstName",
                        "lastName" => "Client.LastName",
                        "ci" => "Client.Ci",
                        "email" => "Client.Email",
                        "phone" => "Client.Phone",
                        "address" => "Client.Address",
                        _ => $"Client.{kv.Key}"
                    };
                    foreach (var msg in kv.Value)
                        ModelState.AddModelError(key, msg);
                }
                if (!result.Errors.Any()) ModelState.AddModelError(string.Empty, "Error desconocido al crear cliente.");
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}
