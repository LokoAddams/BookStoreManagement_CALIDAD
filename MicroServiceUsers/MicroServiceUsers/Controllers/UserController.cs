using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MicroServiceUsers.Domain.Models;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Validations;
using System.Threading;
using Microsoft.AspNetCore.Authorization;

namespace MicroServiceUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll(CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Ok(list);
        }

        // GET: api/user/paged?page=1&pageSize=10
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<User>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<User>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _service.GetPagedAsync(page, pageSize, ct);
            return Ok(result);
        }

        // GET: api/user/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<User>> GetById(Guid id, CancellationToken ct)
        {
            var user = await _service.GetByIdAsync(id, ct);
            if (user is null) return NotFound();
            return Ok(user);
        }

        // GET: api/user/search/{userOrEmail}
        [HttpGet("search/{userOrEmail}")]
        public async Task<ActionResult<User>> GetByUserOrEmail(string userOrEmail, CancellationToken ct)
        {
            var user = await _service.GetByUserOrEmailAsync(userOrEmail, ct);
            if (user is null) return NotFound();
            return Ok(user);
        }

        // GET: api/user/{id}/roles
        [HttpGet("{id:guid}/roles")]
        public async Task<ActionResult<List<string>>> GetRoles(Guid id, CancellationToken ct)
        {
            var roles = await _service.GetRolesAsync(id, ct);
            return Ok(roles);
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
        {
            try
            {
                // Crear el objeto User desde el request
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName ?? string.Empty,
                    LastName = request.LastName ?? string.Empty,
                    MiddleName = request.MiddleName,
                    IsActive = true,
                    MustChangePassword = true,
                    PasswordHash = string.Empty // Se generará en el repositorio
                };

                await _service.CreateAsync(user, request.Password, request.Roles, ct);
                return Ok(user);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    Message = "Errores de validación",
                    Errors = ex.Errors.Select(e => new { e.Field, e.Message })
                });
            }
        }

        // PUT: api/user/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Update(Guid id, [FromBody] User user, CancellationToken ct)
        {
            if (user is null) return BadRequest();

            user.Id = id;

            try
            {
                await _service.UpdateAsync(user, ct);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                return BadRequest(new
                {
                    Message = "Errores de validación",
                    Errors = ex.Errors.Select(e => new { e.Field, e.Message })
                });
            }
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
