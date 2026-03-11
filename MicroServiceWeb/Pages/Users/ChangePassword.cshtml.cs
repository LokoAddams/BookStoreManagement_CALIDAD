using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using MicroServiceWeb.External.Http;

namespace LibraryWeb.Pages.Users
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUsersApiClient _usersApi;
        public ChangePasswordModel(IUsersApiClient usersApi) { _usersApi = usersApi; }

        [BindProperty, Required(ErrorMessage = "La contraseña actual es obligatoria."), Display(Name = "Contraseña actual")] public string CurrentPassword { get; set; } = string.Empty;
        [BindProperty, Required(ErrorMessage = "La nueva contraseña es obligatoria."), MinLength(8, ErrorMessage = "Debe tener al menos 8 caracteres."), Display(Name = "Nueva contraseña")] public string NewPassword { get; set; } = string.Empty;
        [BindProperty, Required(ErrorMessage = "Debe confirmar la nueva contraseña."), Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden."), Display(Name = "Confirmar nueva contraseña")] public string ConfirmPassword { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public bool PasswordChanged { get; private set; } // usado por la vista legacy

        private string? PendingToken => TempData.Peek("PendingToken")?.ToString();

        public IActionResult OnGet()
        {
            // Permitir acceso si:
            // 1) Usuario autenticado (cambio voluntario) OR
            // 2) Flujo forzado de primer login (tenemos PendingUser + PendingToken)
            bool authenticated = User.Identity?.IsAuthenticated == true;
            bool firstLoginFlow = TempData.ContainsKey("PendingUser") && PendingToken != null;

            if (authenticated)
            {
                // Para cambio voluntario no necesitamos TempData, limpiamos cualquier residuo.
                TempData.Remove("PendingUser");
                TempData.Remove("FirstLogin");
            }
            else if (firstLoginFlow)
            {
                // Mantener datos para POST
                TempData.Keep("PendingUser");
                TempData.Keep("FirstLogin");
                TempData.Keep("PendingToken");
            }
            else
            {
                return RedirectToPage("/Auth/Login");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool authenticated = User.Identity?.IsAuthenticated == true;
            bool firstLoginFlow = TempData.ContainsKey("PendingUser") && PendingToken != null;
            if (!authenticated && !firstLoginFlow) return RedirectToPage("/Auth/Login");

            if (!ModelState.IsValid)
            {
                if (firstLoginFlow)
                {
                    TempData.Keep("PendingUser"); TempData.Keep("FirstLogin"); TempData.Keep("PendingToken");
                }
                return Page();
            }

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\p{P}\p{S}]).{8,64}$");
            if (!regex.IsMatch(NewPassword))
            {
                ModelState.AddModelError(nameof(NewPassword), "La nueva contraseña debe incluir mayúsculas, minúsculas, números y un carácter especial.");
                if (firstLoginFlow) { TempData.Keep("PendingUser"); TempData.Keep("FirstLogin"); TempData.Keep("PendingToken"); }
                return Page();
            }

            // Obtener token: del claim (sesión normal) o de TempData (primer login)
            var token = authenticated ? User.FindFirst("access_token")?.Value : PendingToken;
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "No se encontró el token de autenticación. Inicie sesión nuevamente.");
                return Page();
            }

            var result = await _usersApi.ChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            }, token, HttpContext.RequestAborted);

            if (!result.Success)
            {
                var msg = result.Error ?? "No se pudo cambiar la contraseña.";
                if (msg.Contains("actual", System.StringComparison.OrdinalIgnoreCase) || msg.Contains("incorrecta", System.StringComparison.OrdinalIgnoreCase))
                    ModelState.AddModelError(nameof(CurrentPassword), msg);
                else
                    ModelState.AddModelError(string.Empty, msg);
                if (firstLoginFlow) { TempData.Keep("PendingUser"); TempData.Keep("FirstLogin"); TempData.Keep("PendingToken"); }
                return Page();
            }

            // Éxito: cerrar sesión (si había) y redirigir a login con mensaje
            if (authenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            PasswordChanged = true; // para vista que muestra mensaje antes del redirect si se mantiene
            TempData.Clear();
            TempData["PasswordChanged"] = "La contraseña se cambió correctamente. Inicia sesión con tu nueva contraseña.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
