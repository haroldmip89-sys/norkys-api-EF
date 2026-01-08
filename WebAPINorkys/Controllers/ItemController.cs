using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorkysAPI.Interfaces;
using NorkysAPI.Models;
using NorkysAPI.DTO;

namespace NorkysWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemDAO _itemDAO;

        public ItemController(IItemDAO itemDAO)
        {
            _itemDAO = itemDAO;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _itemDAO.GetAll();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _itemDAO.GetById(id);
            if (item == null) return NotFound(new { mensaje = "Item no encontrado" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CrearItemDTO dto)
        {
            string? imagenUrl = null;

            if (dto.Imagen != null)
            {
                var extension = Path.GetExtension(dto.Imagen.FileName).ToLower();
                var contentType = dto.Imagen.ContentType;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                const long maxFileSize = 2 * 1024 * 1024; // 2MB

                if (!allowedExtensions.Contains(extension) || !allowedMimeTypes.Contains(contentType))
                    return BadRequest("Formato de imagen no permitido");

                if (dto.Imagen.Length > maxFileSize)
                    return BadRequest("La imagen excede el tamaño máximo permitido (2MB)");

                var uploadsPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "items"
                );

                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Imagen.CopyToAsync(stream);

                imagenUrl = $"/uploads/items/{fileName}";
            }

            var item = new Item
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                ImagenUrl = imagenUrl,
                IdCategoria = dto.IdCategoria
            };

            var saved = await _itemDAO.Add(item);
            return CreatedAtAction(nameof(GetById), new { id = saved.IdItem }, saved);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromForm] ActualizarItemDTO dto)
        {
            var itemDb = await _itemDAO.GetById(id);
            if (itemDb == null)
                return NotFound(new { mensaje = "Item no encontrado" });

            // Actualizar campos permitidos
            itemDb.Nombre = dto.Nombre;
            itemDb.Descripcion = dto.Descripcion;
            itemDb.Precio = dto.Precio;

            if (dto.Imagen != null)
            {
                // Validaciones de seguridad
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(dto.Imagen.FileName).ToLower();

                if (!allowedTypes.Contains(dto.Imagen.ContentType) ||
                    !allowedExtensions.Contains(extension))
                {
                    return BadRequest("Formato de imagen no permitido");
                }

                if (dto.Imagen.Length > 2 * 1024 * 1024)
                {
                    return BadRequest("La imagen no debe superar los 2MB");
                }

                var uploadsPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "items"
                );

                // Asegurar carpeta
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // 1️⃣ Eliminar imagen anterior
                if (!string.IsNullOrEmpty(itemDb.ImagenUrl))
                {
                    var oldImagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        itemDb.ImagenUrl.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // 2️⃣ Guardar nueva imagen
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dto.Imagen.CopyToAsync(stream);

                itemDb.ImagenUrl = $"/uploads/items/{fileName}";
            }

            var updated = await _itemDAO.Update(itemDb);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _itemDAO.GetById(id);
            if (item == null)
                return NotFound(new { mensaje = "Item no encontrado" });

            // Eliminar imagen del disco
            if (!string.IsNullOrEmpty(item.ImagenUrl))
            {
                var imagePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    item.ImagenUrl.TrimStart('/')
                );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            await _itemDAO.Delete(id);
            return NoContent();
        }
        [HttpGet("categoria/{idCategoria}")]
        public async Task<IActionResult> GetByIdCategory(int idCategoria)
        {
            var items = await _itemDAO.GetByCategory(idCategoria);
            if (items == null || items.Count == 0) return NotFound(new { mensaje = "Categoria no encontrada" });
            return Ok(items);
        }
    }
}
