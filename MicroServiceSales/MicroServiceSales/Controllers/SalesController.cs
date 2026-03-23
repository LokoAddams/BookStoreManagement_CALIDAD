using Microsoft.AspNetCore.Mvc;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;

namespace MicroServiceSales.Controllers
{
    public record SaleStatusResponse(string Status, string Message);
    public record ValidationErrorResponse(string Message, IEnumerable<ValidationErrorDetail> Errors);
    public record ValidationErrorDetail(string Field, string Message);

    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _service;

        public SalesController(ISalesService service)
        {
            _service = service;
        }

        // GET: api/sales
        [HttpGet]
        [ProducesResponseType(typeof(List<Sale>), StatusCodes.Status200OK)]
        public ActionResult<List<Sale>> GetAll()
        {
            var list = _service.GetAll();
            return Ok(list);
        }

        // GET: api/sales/{id}
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(Sale), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Sale> GetById(Guid id)
        {
            var sale = _service.Read(id);
            if (sale is null) return NotFound();
            // eager-load details for convenience
            sale.Details = _service.GetDetails(id);
            return Ok(sale);
        }

        // GET: api/sales/{id}/status
        // Endpoint para verificar el estado de una venta (usado por el frontend para polling)
        [HttpGet("{id:guid}/status")]
        [ProducesResponseType(typeof(SaleStatusResponse), StatusCodes.Status200OK)]
        public ActionResult GetStatus(Guid id)
        {
            var sale = _service.Read(id);
            if (sale is null)
            {
                return Ok(new SaleStatusResponse("PENDING", "La venta está siendo procesada..."));
            }
            
            // La venta existe, retornar su estado con mensaje específico
            var message = sale.Status switch
            {
                "COMPLETED" => "Venta completada exitosamente",
                "CANCELLED" => sale.CancellationReason ?? "La venta fue cancelada por falta de stock",
                "PENDING" => "La venta está siendo procesada...",
                "REFUNDED" => "La venta fue reembolsada",
                _ => $"Estado: {sale.Status}"
            };
            
            return Ok(new SaleStatusResponse(sale.Status, message));
        }

        // GET: api/sales/{id}/details
        [HttpGet("{id:guid}/details")]
        [ProducesResponseType(typeof(List<SaleDetail>), StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<SaleDetail>> GetDetails(Guid id)
        {
            var sale = _service.Read(id);
            if (sale is null) return NotFound();
            var details = _service.GetDetails(id);
            return Ok(details);
        }

        // POST: api/sales
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Create([FromBody] Sale sale)
        {
            try
            {
                _service.Create(sale);
                return Ok(sale);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new ValidationErrorDetail(e.Field, e.Message));
                return BadRequest(new ValidationErrorResponse("Errores de validación", errors));
            }
        }

        // PUT: api/sales/{id}
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public ActionResult Update(Guid id, [FromBody] Sale sale)
        {
            if (sale is null) return BadRequest();
            sale.Id = id;
            try
            {
                _service.Update(sale);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => new ValidationErrorDetail(e.Field, e.Message));
                return BadRequest(new ValidationErrorResponse("Errores de validación", errors));
            }
        }

        // DELETE: api/sales/{id}
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult Delete(Guid id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
