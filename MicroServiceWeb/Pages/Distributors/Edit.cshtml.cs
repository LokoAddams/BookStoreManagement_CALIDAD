using ServiceDistributors.Domain.Models;
using ServiceDistributors.Domain.Interfaces;
using ServiceDistributors.Domain.Validations;
using ServiceCommon.Application.Services;
using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading;

namespace LibraryWeb.Pages.Distributors
{
    public class EditModel : PageModel
    {
        private readonly IDistributorsApiClient _api;

        [BindProperty]
        public DistributorUpdateDto Distributor { get; set; } = new();

        [BindProperty]
        public Guid Id { get; set; }

        [TempData]
        public Guid EditDistributorId { get; set; }

        public EditModel(IDistributorsApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var obj = TempData["EditDistributorId"];
            if (obj == null)
                return RedirectToPage("Index");

            Guid id = Guid.Empty;

            if (obj is Guid g)
            {
                id = g;
            }
            else if (obj != null && Guid.TryParse(obj.ToString(), out var p))
            {
                id = p;
            }

            if (id == Guid.Empty)
                return RedirectToPage("Index");

            var d = await _api.GetByIdAsync(id, ct);
            if (d == null)
                return RedirectToPage("Index");

            Id = d.Id;
            Distributor = new DistributorUpdateDto { Name = d.Name, ContactEmail = d.ContactEmail, Phone = d.Phone, Address = d.Address };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await _api.UpdateAsync(Id, Distributor, ct);
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
                    foreach (var msg in kv.Value)
                        ModelState.AddModelError(key, msg);
                }
                if (result.Errors.Count == 0)
                    ModelState.AddModelError(string.Empty, "No se pudo actualizar el distribuidor.");
                return Page();
            }
            return RedirectToPage("Index");
        }
    }
}
