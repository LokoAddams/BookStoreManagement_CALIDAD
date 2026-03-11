// New file: CategoriesController
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroServiceProduct.Domain.Interfaces;
using MicroServiceProduct.Domain.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace MicroServiceProduct.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repo;

    public CategoriesController(ICategoryRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Obtiene todas las categorías activas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Category>> GetAll()
    {
        var list = _repo.GetAll();
        return Ok(list);
    }

    /// <summary>
    /// Obtiene una categoría por su id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Category> Get(Guid id)
    {
        var c = _repo.Read(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Category> Create([FromBody] Category? model)
    {
        if (model == null) return BadRequest();
        if (string.IsNullOrWhiteSpace(model.Name)) return BadRequest(new { message = "Name is required" });
        model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
        model.CreatedAt = DateTime.UtcNow;
        _repo.Create(model);
        return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
    }

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(Guid id, [FromBody] Category? model)
    {
        if (model == null) return BadRequest();
        var existing = _repo.Read(id);
        if (existing == null) return NotFound();
        existing.Name = model.Name;
        existing.Description = model.Description;
        _repo.Update(existing);
        return NoContent();
    }

    /// <summary>
    /// Elimina una categoría por su id (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        var existing = _repo.Read(id);
        if (existing == null) return NotFound();
        _repo.Delete(id);
        return NoContent();
    }
}
