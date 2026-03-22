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
            // 1. Guardias de Seguridad (Early Return)
            bool authenticated = User.Identity?.IsAuthenticated == true;
            bool firstLoginFlow = TempData.ContainsKey("PendingUser") && PendingToken != null;

            if (!authenticated && !firstLoginFlow)
                return RedirectToPage("/Auth/Login");

            // 2. Validaciones Previas
            if (!ModelState.IsValid)
            {
                return MantenerEstadoYRegresar(firstLoginFlow);
            }

            if (!ValidarSeguridadPassword())
            {
                return MantenerEstadoYRegresar(firstLoginFlow);
            }

            // 3. Obtención de Token
            var token = authenticated ? User.FindFirst("access_token")?.Value : PendingToken;
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Sesión expirada. Inicie sesión nuevamente.");
                return Page();
            }

            // 4. Ejecución del Cambio de Contraseña
            var result = await _usersApi.ChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            }, token, HttpContext.RequestAborted);

            if (!result.Success)
            {
                ProcesarErrorApi(result.Error);
                return MantenerEstadoYRegresar(firstLoginFlow);
            }

            // 5. Finalización Exitosa
            return await FinalizarCambioPassword(authenticated);
        }


        private bool ValidarSeguridadPassword()
        {
            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\p{P}\p{S}]).{8,64}$");
            if (regex.IsMatch(NewPassword)) return true;

            ModelState.AddModelError(nameof(NewPassword), "La nueva contraseña debe incluir mayúsculas, minúsculas, números y un carácter especial.");
            return false;
        }

        private void ProcesarErrorApi(string? error)
        {
            var msg = error ?? "No se pudo cambiar la contraseña.";
            bool esErrorPasswordActual = msg.Contains("actual", StringComparison.OrdinalIgnoreCase) ||
                                         msg.Contains("incorrecta", StringComparison.OrdinalIgnoreCase);

            if (esErrorPasswordActual)
                ModelState.AddModelError(nameof(CurrentPassword), msg);
            else
                ModelState.AddModelError(string.Empty, msg);
        }

        private IActionResult MantenerEstadoYRegresar(bool firstLoginFlow)
        {
            if (firstLoginFlow)
            {
                TempData.Keep("PendingUser");
                TempData.Keep("FirstLogin");
                TempData.Keep("PendingToken");
            }
            return Page();
        }

        private async Task<IActionResult> FinalizarCambioPassword(bool authenticated)
        {
            if (authenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            PasswordChanged = true;
            TempData.Clear();
            TempData["PasswordChanged"] = "La contraseña se cambió correctamente. Inicia sesión con tu nueva contraseña.";
            return RedirectToPage("/Auth/Login");
        }
    }
}
