using MicroServiceDistributors.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using MicroServiceDistributors.Application.DTOs;
using MicroServiceDistributors.Domain.Models;
using MicroServiceDistributors.Application.Services; // ValidationException
using System;
using System.Linq;

namespace MicroServiceDistributors.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DistributorsController : ControllerBase
    {
        private readonly IDistributorService _distributorService;

        public DistributorsController(IDistributorService distributorService)
        {
            _distributorService = distributorService;
        }

        // GET api/distributors
        [HttpGet]
        public ActionResult<IEnumerable<DistributorDto>> GetAll()
        {
            var list =  _distributorService.GetAll().Select(ToDto);
            return Ok(list);
        }
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<DistributorDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<DistributorDto>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var list = await _distributorService.GetPagedAsync(page, pageSize, ct);
            return Ok(list);
        }

        // GET api/distributors/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<DistributorDto> GetById(Guid id)
        {
            var distributor = _distributorService.Read(id);
            if (distributor == null) return NotFound();
            return Ok(ToDto(distributor));
        }

        // POST api/distributors
        [HttpPost]
        public ActionResult<DistributorDto> Create([FromBody] DistributorDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var distributor = FromDto(dto);
            distributor.Id = Guid.NewGuid(); // asegurar Id
            try
            {
                _distributorService.Create(distributor);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => new { field = e.Field, message = e.Message }) });
            }
            return CreatedAtAction(nameof(GetById), new { id = distributor.Id }, ToDto(distributor));
        }

        // PUT api/distributors/{id}
        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody] DistributorDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            var existing = _distributorService.Read(id);
            if (existing == null) return NotFound();

            // Map cambios
            existing.Name = dto.Name;
            existing.ContactEmail = dto.ContactEmail ?? string.Empty;
            existing.Phone = dto.Phone ?? string.Empty;
            existing.Address = dto.Address ?? string.Empty;
            try
            {
                _distributorService.Update(existing);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { errors = ex.Errors.Select(e => new { field = e.Field, message = e.Message }) });
            }
            return NoContent();
        }

        // DELETE api/distributors/{id}
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var existing = _distributorService.Read(id);
            if (existing == null) return NotFound();
            _distributorService.Delete(id);
            return NoContent();
        }

        private static DistributorDto ToDto(Distributor d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            ContactEmail = d.ContactEmail,
            Phone = d.Phone,
            Address = d.Address,
            CreatedAt = d.CreatedAt
        };

        private static Distributor FromDto(DistributorDto dto) => new()
        {
            Id = dto.Id,
            Name = dto.Name,
            ContactEmail = dto.ContactEmail ?? string.Empty,
            Phone = dto.Phone ?? string.Empty,
            Address = dto.Address ?? string.Empty,
            CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
        };
    }
}
