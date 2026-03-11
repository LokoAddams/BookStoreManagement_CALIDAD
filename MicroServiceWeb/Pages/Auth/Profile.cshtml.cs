using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryWeb.Pages.Auth
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        public string UserName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;

        public void OnGet()
        {
            UserName = User.Identity?.Name ?? string.Empty;
            Email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            // Solo mostrar usuario y correo por requerimiento
        }
    }
}
