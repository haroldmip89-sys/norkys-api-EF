using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NorkysAPI.DTO;
using NorkysAPI.Interfaces;
using NorkysAPI.Models;

namespace WebAPINorkys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DireccionesController : ControllerBase
    {
        private readonly IDireccionesDAO _direccionesDAO;

        public DireccionesController(IDireccionesDAO direccionesDAO)
        {
            _direccionesDAO = direccionesDAO;
        }

        // GET api/Direcciones/usuario/5
        [HttpGet("usuario/{IdUsuario}")]
        public async Task<IActionResult> GetAll(int IdUsuario)
        {
            var lista = await _direccionesDAO.GetAll(IdUsuario);
            return Ok(lista);
        }

        // GET api/Direcciones/5
        [HttpGet("{IdDireccion}")]
        public async Task<IActionResult> GetById(int IdDireccion)
        {
            var entity = await _direccionesDAO.GetById(IdDireccion);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        // POST api/Direcciones
        [HttpPost]
        public async Task<IActionResult> CrearDireccion(CrearDireccionDTO dto)
        {
            var direccion = new Direcciones
            {
                IdUsuario = dto.IdUsuario,
                TituloDireccion = dto.TituloDireccion,
                Direccion = dto.Direccion,
                Referencia = dto.Referencia,
                Telefono1 = dto.Telefono1,
                Telefono2 = dto.Telefono2,
                LatY = dto.LatY,
                LongX = dto.LongX
            };
            await _direccionesDAO.Create(direccion);
            return Ok(dto);
        }

        // PUT api/Direcciones/5
        [HttpPut("{IdDireccion}")]
        public async Task<IActionResult> Update(int IdDireccion, [FromBody] EditarDireccionUsuarioDTO dto)
        {
            //verificar que sea un IdDireccion valido
            if (IdDireccion <= 0)
            {
                return BadRequest();
            }
            //mapear el dto a la entidad
            var direccion = new Direcciones
            {  
                TituloDireccion = dto.TituloDireccion,
                IdUsuario= dto.IdUsuario,
                Direccion = dto.Direccion,
                Referencia = dto.Referencia,
                Telefono1 = dto.Telefono1,
                Telefono2 = dto.Telefono2,
                LatY = dto.LatY,
                LongX = dto.LongX
            };
            //actualizar la direccion
            var ok = await _direccionesDAO.Update(IdDireccion,direccion);
            //si no se actualizo retornar notfound
            if (!ok) return NotFound();
            return NoContent();
        }

        // DELETE api/Direcciones/5
        [HttpDelete("{IdDireccion}")]
        public async Task<IActionResult> Delete(int IdDireccion)
        {
            var ok = await _direccionesDAO.Delete(IdDireccion);
            if (!ok) return NotFound();

            return NoContent();
        }
    }
}
