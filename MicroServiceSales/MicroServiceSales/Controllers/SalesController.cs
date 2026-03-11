using Microsoft.AspNetCore.Mvc;
using MicroServiceSales.Domain.Interfaces;
using MicroServiceSales.Domain.Models;
using MicroServiceSales.Domain.Validations;

namespace MicroServiceSales.Controllers
{
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
        public ActionResult<List<Sale>> GetAll()
        {
            var list = _service.GetAll();
            return Ok(list);
        }

        // GET: api/sales/{id}
        [HttpGet("{id:guid}")]
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
        public ActionResult GetStatus(Guid id)
        {
            var sale = _service.Read(id);
            if (sale is null)
            {
                // La venta aún no existe en la DB (puede estar en proceso o nunca existió)
                return Ok(new { 
                    Status = "PENDING", 
                    Message = "La venta está siendo procesada..." 
                });
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
            
            return Ok(new { 
                Status = sale.Status,
                Message = message
            });
        }

        // GET: api/sales/{id}/details
        [HttpGet("{id:guid}/details")]
        public ActionResult<List<SaleDetail>> GetDetails(Guid id)
        {
            var sale = _service.Read(id);
            if (sale is null) return NotFound();
            var details = _service.GetDetails(id);
            return Ok(details);
        }

        // POST: api/sales
        [HttpPost]
        public ActionResult Create([FromBody] Sale sale)
        {
            try
            {
                _service.Create(sale);
                return Ok(sale);
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

        // PUT: api/sales/{id}
        [HttpPut("{id:guid}")]
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
                return BadRequest(new
                {
                    Message = "Errores de validación",
                    Errors = ex.Errors.Select(e => new { e.Field, e.Message })
                });
            }
        }

        // DELETE: api/sales/{id}
        [HttpDelete("{id:guid}")]
        public ActionResult Delete(Guid id)
        {
            _service.Delete(id);
            return NoContent();
        }
    }
}
