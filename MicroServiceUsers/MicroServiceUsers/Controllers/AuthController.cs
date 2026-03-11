using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroServiceUsers.Application.DTOs;
using MicroServiceUsers.Application.Facade;

namespace MicroServiceUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserFacade _facade;

        public AuthController(IUserFacade facade)
        {
            _facade = facade;
        }

        /// <summary>
        /// Login de usuario - Genera token JWT
        /// </summary>
        /// <param name="request">Credenciales de usuario (username/email y password)</param>
        /// <returns>Token JWT y datos del usuario</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthTokenDto>> Login([FromBody] AuthRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _facade.LoginAsync(request);
            
            if (result is null)
                return Unauthorized(new { Message = "Credenciales inválidas o usuario inactivo." });

            return Ok(result);
        }

        /// <summary>
        /// Registro de nuevo usuario - Genera username y password automáticamente
        /// </summary>
        /// <param name="dto">Email y rol del usuario</param>
        /// <returns>Usuario creado con credenciales generadas</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserReadDto>> Register([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _facade.CreateUserAsync(dto);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error al crear usuario", Error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener todos los usuarios (requiere autenticación)
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet("users")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<UserReadDto>>> GetAllUsers()
        {
            var users = await _facade.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        /// <param name="dto">Contraseña actual y nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Obtener el ID del usuario autenticado desde el token
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized(new { Message = "No se pudo identificar al usuario." });

            try
            {
                var result = await _facade.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
                
                if (!result)
                    return BadRequest(new { Message = "La contraseña actual es incorrecta." });

                return Ok(new { Message = "Contraseña cambiada exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Error al cambiar la contraseña", Error = ex.Message });
            }
        }
    }
}
