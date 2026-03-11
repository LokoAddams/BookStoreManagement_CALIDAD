// New file: ProductsController
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Domain.Models;
using Microsoft.AspNetCore.Http;
using ServiceCommon.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MicroServiceProduct.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;

        public ProductsController(IProductService svc)
        {
            _svc = svc;
        }

        /// <summary>
        /// Obtiene todos los productos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            var list = _svc.GetAll();
            return Ok(list);
        }

        /// <summary>
        /// Obtiene un producto por su id.
        /// </summary>
        [HttpGet("{id:guid}")] // 👈 restricción: solo coincide si id es GUID
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Product> Get(Guid id)
        {
            var p = _svc.Read(id);
            if (p == null) return NotFound();
            return Ok(p);
        }

        /// <summary>
        /// Obtiene productos paginados.
        /// </summary>
        [HttpGet("paged")]
        [ProducesResponseType(typeof(PagedResult<Product>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<Product>>> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var list = await _svc.GetPagedAsync(page, pageSize, ct);
            return Ok(list);
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> Create([FromBody] Product? model)
        {
            if (model == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(model.Name))
                return BadRequest(new { message = "Name is required" });
            if (model.CategoryId == Guid.Empty)
                return BadRequest(new { message = "CategoryId is required" });

            model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
            model.CreatedAt = DateTime.UtcNow;

            _svc.Create(model);
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(Guid id, [FromBody] Product? model)
        {
            if (model == null) return BadRequest();

            var existing = _svc.Read(id);
            if (existing == null) return NotFound();

            existing.Name = model.Name;
            existing.Description = model.Description;
            existing.Price = model.Price;
            existing.Stock = model.Stock;
            existing.CategoryId = model.CategoryId;

            _svc.Update(existing);
            return NoContent();
        }

        /// <summary>
        /// Elimina un producto por su id.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Delete(Guid id)
        {
            var existing = _svc.Read(id);
            if (existing == null) return NotFound();

            _svc.Delete(id);
            return NoContent();
        }
    }
}
