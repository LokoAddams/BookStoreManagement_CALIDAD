using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;
using System.Linq;

namespace LibraryWeb.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly IClientsApiClient _api;
        [BindProperty] public Guid ClientId { get; set; }
        [BindProperty] public ClientUpdateDto Client { get; set; } = new();
        [TempData] public Guid EditClientId { get; set; }
        public EditModel(IClientsApiClient api) { _api = api; }
        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            if (EditClientId == Guid.Empty) return RedirectToPage("Index");
            var dto = await _api.GetByIdAsync(EditClientId, ct);
            if (dto == null) return RedirectToPage("Index");
            ClientId = dto.Id;
            Client = new ClientUpdateDto { FirstName = dto.FirstName, LastName = dto.LastName, Ci = dto.Ci, Email = dto.Email, Phone = dto.Phone, Address = dto.Address };
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            Client.FirstName = Client.FirstName?.Trim() ?? string.Empty;
            Client.LastName = Client.LastName?.Trim() ?? string.Empty;
            Client.Ci = Client.Ci?.Trim() ?? string.Empty;
            Client.Email = Client.Email?.Trim();
            Client.Phone = Client.Phone?.Trim();
            Client.Address = Client.Address?.Trim();

            if (!ModelState.IsValid) return Page();
            var result = await _api.UpdateAsync(ClientId, Client, ct);
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
                if (!result.Errors.Any()) ModelState.AddModelError(string.Empty, "Error desconocido al actualizar cliente.");
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}
