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
    public class WishListItemController : ControllerBase
    {
        private readonly IWishListItemDAO _wishListItemDAO;
        private readonly IItemDAO _itemDAO;

        public WishListItemController(IWishListItemDAO wishItemDAO, IItemDAO itemDAO)
        {
            _wishListItemDAO = wishItemDAO;
            _itemDAO = itemDAO;
        }

        // GET: api/WishListItem/usuario/5
        [HttpGet("usuario/{IdUsuario}")]
        public async Task<IActionResult> GetAll(int IdUsuario)
        {
            var lista = await _wishListItemDAO.GetAll(IdUsuario);
            return Ok(lista);
        }

        // DELETE: api/WishListItem/10
        [HttpDelete("{IdWishListItem}")]
        public async Task<IActionResult> Delete(int IdWishListItem)
        {
            var ok = await _wishListItemDAO.Delete(IdWishListItem);
            if (!ok) return NotFound(new { mensaje = "Elemento no encontrado" });
            return NoContent();
        }
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] WishListCreateDTO dto)
        {
            if (dto.IdUsuario <= 0 || dto.IdItem <= 0)
                return BadRequest(new { mensaje = "Datos inválidos." });
            if (dto.IdUsuario == 1)
                return BadRequest(new { mensaje = "El usuario administrador no puede tener wishlist." });
            var nuevo = new WishListItem
            {
                IdUsuario = dto.IdUsuario,
                IdItem = dto.IdItem
            };
            var existeItem= await _itemDAO.GetById(dto.IdItem);
            if (existeItem == null) { return BadRequest(new { mensaje = "No existe Item" });}
            var result = await _wishListItemDAO.Add(nuevo);

            if (!result)
                return Conflict(new { mensaje = "El item ya está en la wishlist." });

            return Ok(new { mensaje = "Item agregado a la wishlist." });
        }
    }
}
