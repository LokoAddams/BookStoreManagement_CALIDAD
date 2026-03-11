using Microsoft.AspNetCore.Mvc;
using MicroServiceUsers.Domain.Interfaces;
using MicroServiceUsers.Domain.Models;
using System.Threading;

namespace MicroServiceUsers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _repository;

        public RoleController(IRoleRepository repository)
        {
            _repository = repository;
        }

        // GET: api/role
        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetAll(CancellationToken ct)
        {
            var roles = await _repository.GetAllAsync(ct);
            return Ok(roles);
        }

        // GET: api/role/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Role>> GetById(Guid id, CancellationToken ct)
        {
            var role = await _repository.GetByIdAsync(id, ct);
            if (role is null) return NotFound();
            return Ok(role);
        }

        // GET: api/role/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Role>> GetByName(string name, CancellationToken ct)
        {
            var role = await _repository.GetByNameAsync(name, ct);
            if (role is null) return NotFound();
            return Ok(role);
        }

        // POST: api/role
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Role role, CancellationToken ct)
        {
            if (role is null) return BadRequest();

            await _repository.CreateAsync(role, ct);
            return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
        }
    }
}
