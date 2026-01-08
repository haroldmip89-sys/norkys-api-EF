using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorkysAPI.DTO;
using NorkysAPI.Interfaces;
using NorkysAPI.Models;
using NorkysAPI.Operaciones;

namespace WebAPINorkys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaDAO _categoriaDAO;
        public CategoriaController(ICategoriaDAO categoriaDAO) 
        {
            _categoriaDAO = categoriaDAO;
        }
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _categoriaDAO.GetAll();
            var categoriasDto = items.Select(c => new CategoriaDTO
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre
            });
            return Ok(categoriasDto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNombre(int id, [FromBody] CategoriaUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return BadRequest("El nombre es obligatorio");

            var updated = await _categoriaDAO.UpdateNombre(id, dto.Nombre);

            if (!updated)
                return NotFound("Categoría no encontrada");

            return NoContent(); // 204
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoriaById(int id)
        {
            var categoria = await _categoriaDAO.GetById(id);

            if (categoria == null)
                return NotFound(new { mensaje = "Categoría no encontrada" });

            return Ok(categoria);
        }

    }
}
