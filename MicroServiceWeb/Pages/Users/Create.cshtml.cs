using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace LibraryWeb.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly IUsersApiClient _api;
        public CreateModel(IUsersApiClient api) { _api = api; }
        [BindProperty, Required(ErrorMessage="El correo electrónico es obligatorio."), EmailAddress(ErrorMessage="Correo inválido.")] public string Email { get; set; } = string.Empty;
        [BindProperty, Required(ErrorMessage="El rol es obligatorio.")] public string SelectedRole { get; set; } = string.Empty;
        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            Email = (Email ?? string.Empty).Trim().ToLowerInvariant(); SelectedRole = (SelectedRole ?? string.Empty).Trim();
            if (!ModelState.IsValid) return Page();
            var dto = new UserCreateRequest { Email = Email, Role = SelectedRole };
            // Preferir endpoint de registro si existe
            var result = await _api.RegisterAsync(dto, ct);
            if (!result.Success)
            {
                // fallback create
                var createResult = await _api.CreateAsync(dto, ct);
                if (!createResult.Success)
                {
                    foreach (var kv in createResult.Errors) foreach (var msg in kv.Value) ModelState.AddModelError(kv.Key, msg);
                    if (!createResult.Errors.Any()) ModelState.AddModelError(string.Empty, "Error al crear usuario.");
                    return Page();
                }
            }
            TempData["SuccessMessage"] = "Usuario creado exitosamente."; return RedirectToPage("Index");
        }
    }
}
