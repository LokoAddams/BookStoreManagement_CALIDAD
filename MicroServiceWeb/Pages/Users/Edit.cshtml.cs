using MicroServiceWeb.External.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using System.Threading;

namespace LibraryWeb.Pages.Users
{
    public class EditModel : PageModel
    {
        private readonly IUsersApiClient _api;

        [BindProperty]
        public Guid UserId { get; set; }

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string SelectedRole { get; set; } = string.Empty;

        [TempData]
        public Guid EditUserId { get; set; }

        public EditModel(IUsersApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            if (EditUserId == Guid.Empty)
                return RedirectToPage("Index");

            var user = await _api.GetByIdAsync(EditUserId, ct);

            if (user == null)
                return RedirectToPage("Index");

            UserId = user.Id;
            Email = user.Email ?? string.Empty;

            // buscar roles completos
            var full = await _api.SearchAsync(user.Username, ct);
            SelectedRole = full?.Roles?.FirstOrDefault() ?? string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            Email = (Email ?? string.Empty).Trim().ToLowerInvariant();
            SelectedRole = (SelectedRole ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(Email))
                ModelState.AddModelError("Email", "El correo electrónico es requerido");

            if (string.IsNullOrWhiteSpace(SelectedRole))
                ModelState.AddModelError("SelectedRole", "El rol es requerido");

            if (!ModelState.IsValid)
                return Page();

            var update = new UserUpdateRequest
            {
                Email = Email,
                Roles = new List<string> { SelectedRole }
            };

            var result = await _api.UpdateAsync(UserId, update, ct);

            if (!result.Success)
            {
                foreach (var kv in result.Errors)
                {
                    foreach (var msg in kv.Value)
                    {
                        ModelState.AddModelError(kv.Key, msg);
                    }
                }

                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}
