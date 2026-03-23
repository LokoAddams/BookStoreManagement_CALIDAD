using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroServiceClient.Domain.Models;
using MicroServiceClient.Domain.Interfaces;
using MicroServiceClient.Domain.Validations;

namespace MicroServiceClient.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _service;

        public ClientController(IClientService service)
        {
            _service = service;
        }

        // GET: api/client
        [HttpGet]
        [ProducesResponseType(typeof(List<Client>), StatusCodes.Status200OK)]
        public ActionResult<List<Client>> GetAll()
        {
            var list = _service.GetAll();
            return Ok(list);
        }

        // GET: api/client/paged?page=1&pageSize=10
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<Client>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Client>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _service.GetPagedAsync(page, pageSize, ct);
            return Ok(result);
        }

        // GET: api/client/by-ci/{ci}
        [HttpGet("by-ci/{ci}")]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Client>> GetByCi([FromRoute] string ci, CancellationToken ct = default)
        {
            var normalized = TextRules.NormalizeCi(ci);
            var client = await _service.GetByCiAsync(normalized, ct);
            if (client is null) return NotFound();
            return Ok(client);
        }

        // GET: api/client/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Client> GetById(Guid id)
        {
            var client = _service.Read(id);
            if (client is null) return NotFound();
            return Ok(client);
        }

        // POST: api/client
        [HttpPost]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)] 
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public ActionResult Create([FromBody] Client client)
        {
            try
            {
                _service.Create(client);
                return Ok(client);
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

        // PUT: api/client/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public ActionResult Update(Guid id, [FromBody] Client client)
        {
            if (client is null) return BadRequest();

            client.Id = id;

            try
            {
                _service.Update(client);
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

        // DELETE: api/client/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult Delete(Guid id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
