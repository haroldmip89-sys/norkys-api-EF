using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorkysAPI.DTO;
using NorkysAPI.Interfaces;
using NorkysAPI.Models;

namespace NorkysWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarritoController : ControllerBase
    {
        private readonly ICarritoDAO _carritoDAO;
        private readonly IUsuarioDAO _usuarioDAO;//solo para verificar si existe el id

        public CarritoController(ICarritoDAO carritoDAO, IUsuarioDAO usuarioDAO)
        {
            _carritoDAO = carritoDAO;
            _usuarioDAO = usuarioDAO;
        }
        //Obtener solo carritos por Usuario
        [HttpGet("usuario/{IdUsuario}")]
        public async Task<IActionResult> GetCarritosByUsuario(int IdUsuario)
        {
            // Verificar si el usuario existe
            var usuario = await _usuarioDAO.GetById(IdUsuario);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            var carritos = await _carritoDAO.GetCarritosByUsuario(IdUsuario);
            return Ok(carritos);
        }
        //Obtener carritoDetalles por IdCarrito
        [HttpGet("detalle/{IdCarrito}")]
        public async Task<IActionResult> GetCarritoDetalles(int IdCarrito)
        {
            // Verificar si el carrito existe
            var carrito = await _carritoDAO.GetById(IdCarrito);
            if (carrito == null)
                return NotFound(new { mensaje = "Carrito no encontrado" });

            var detalles = await _carritoDAO.GetCarritoDetalles(IdCarrito);
            return Ok(detalles);
        }
        // Crear Carrito
        [HttpPost]
        public async Task<IActionResult> CrearCarrito([FromBody] CrearCarritoInputDTO input)
        {
            // ───────────────────────────────────────────────────────────────
            // VALIDACIÓN GENERAL
            // ───────────────────────────────────────────────────────────────

            // 1. Validación: O ES usuario registrado o ES invitado
            bool esUsuarioRegistrado = input.IdUsuario.HasValue && input.IdUsuario > 0;
            bool esInvitado = !esUsuarioRegistrado;

            // 2. Bloquear administrador
            if (input.IdUsuario == 1)
                return BadRequest(new { mensaje = "El usuario administrador no puede crear carritos." });


            // ───────────────────────────────────────────────────────────────
            // VALIDAR USUARIO REGISTRADO
            // ───────────────────────────────────────────────────────────────
            if (esUsuarioRegistrado)
            {
                var usuario = await _usuarioDAO.GetById(input.IdUsuario.Value);

                if (usuario == null)
                    return NotFound(new { mensaje = "El usuario no existe." });

                // Si es usuario registrado → ignoramos cualquier dato de invitado
                input.NombreCliente = null;
                input.ApellidoCliente = null;
                input.EmailCliente = null;
                input.DNICliente = null;
            }


            // ───────────────────────────────────────────────────────────────
            // VALIDAR USUARIO INVITADO
            // ───────────────────────────────────────────────────────────────
            if (esInvitado)
            {
                if (string.IsNullOrWhiteSpace(input.NombreCliente) ||
                    string.IsNullOrWhiteSpace(input.ApellidoCliente) ||
                    string.IsNullOrWhiteSpace(input.EmailCliente) || 
                    string.IsNullOrWhiteSpace(input.DNICliente) ||
                    string.IsNullOrWhiteSpace(input.Telefono1))
                {
                    return BadRequest(new
                    {
                        mensaje = "Para invitados son obligatorios NombreCliente, ApellidoCliente,EmailCliente, DNICliente y Telefono1."
                    });
                }
            }


            // ───────────────────────────────────────────────────────────────
            // VALIDAR DIRECCIÓN
            // ───────────────────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(input.TituloDireccion))
                return BadRequest(new { mensaje = "El título de dirección es obligatorio." });

            if (string.IsNullOrWhiteSpace(input.Direccion))
                return BadRequest(new { mensaje = "La dirección es obligatoria." });

            if (string.IsNullOrWhiteSpace(input.Telefono1))
                return BadRequest(new { mensaje = "Debe ingresar un teléfono." });


            // ───────────────────────────────────────────────────────────────
            // VALIDAR DETALLES DEL CARRITO
            // ───────────────────────────────────────────────────────────────
            if (input.Detalles == null || !input.Detalles.Any())
                return BadRequest(new { mensaje = "Debe enviar al menos un producto en el carrito." });


            // ───────────────────────────────────────────────────────────────
            // CREAR CARRITO POR SP
            // ───────────────────────────────────────────────────────────────
            var idCarrito = await _carritoDAO.CrearCarritoConSp(
                input.IdUsuario,
                input.NombreCliente,
                input.ApellidoCliente,
                input.EmailCliente,
                input.DNICliente,
                input.TituloDireccion,
                input.Direccion,
                input.Referencia,
                input.Telefono1,
                input.Telefono2,
                input.LatY,
                input.LongX,
                input.Detalles
            );

            return Ok(new { IdCarrito = idCarrito });
        }

        //Cambiar estado
        [HttpPut("estado/{IdCarrito}")]
        public async Task<IActionResult> CambiarEstado(int IdCarrito, [FromBody] string estado)
        {
            var result = await _carritoDAO.UpdateEstado(IdCarrito, estado);
            if (!result) return NotFound(new { mensaje = "Carrito no encontrado" });
            return NoContent();
        }

        // PUT: api/Carrito/metodoPago/{IdCarrito}
        [HttpPut("metodoPago/{IdCarrito}")]
        public async Task<IActionResult> CambiarMetodoPago(int IdCarrito, [FromBody] string metodoPago)
        {
            var result = await _carritoDAO.UpdateMetodoPago(IdCarrito, metodoPago);

            if (!result)
                return NotFound(new { mensaje = "Carrito no encontrado" });

            return NoContent();
        }


        //Borrar carrito
        [HttpDelete("{IdCarrito}")]
        public async Task<IActionResult> DeleteCarrito(int IdCarrito)
        {
            var result = await _carritoDAO.DeleteCarrito(IdCarrito);
            if (!result) return NotFound(new { mensaje = "Carrito no encontrado" });
            return NoContent();
        }

        // Seleccionar Dirección para un Carrito
        [HttpPut("{IdCarrito}/direccion/{IdDireccion}")]
        public async Task<IActionResult> SeleccionarDireccion(int IdCarrito, int IdDireccion)
        {
            var result = await _carritoDAO.UpdateDireccion(IdCarrito, IdDireccion);

            if (!result)
                return NotFound(new { mensaje = "Carrito o Dirección no encontrados, o no pertenece al usuario." });

            return NoContent();
        }
        // Editar Dirección almacenada en un Carrito
        //[HttpPut("{idCarrito}/EditarDireccion")]
        //public async Task<IActionResult> EditarDireccion(int idCarrito, [FromBody] EditarDireccionCarritoDTO dto)
        //{
        //    var carrito = await _carritoDAO.GetById(idCarrito);

        //    if (carrito == null)
        //        return NotFound(new { message = "El carrito no existe." });

        //    // Editar solo los campos de dirección almacenados dentro del carrito
        //    carrito.TituloDireccion = dto.TituloDireccion;
        //    carrito.Direccion = dto.Direccion;
        //    carrito.Referencia = dto.Referencia;
        //    carrito.Telefono1 = dto.Telefono1;
        //    carrito.Telefono2 = dto.Telefono2;
        //    carrito.LatY = dto.LatY;
        //    carrito.LongX = dto.LongX;

        //    var actualizado = await _carritoDAO.ActualizarCarritoDireccion(carrito);

        //    if (!actualizado)
        //        return StatusCode(500, new { message = "No se pudo actualizar la dirección del carrito." });

        //    return Ok(new { message = "Dirección del carrito actualizada correctamente." });
        //}
        //[HttpGet("{IdCarrito}")]
        //public async Task<IActionResult> GetCarritoCompleto(int IdCarrito)
        //{
        //    var carrito = await _carritoDAO.GetById(IdCarrito);
        //    if (carrito == null)
        //        return NotFound(new { mensaje = "Carrito no encontrado" });

        //    var items = await _carritoDAO.GetCarritoDetalles(IdCarrito);

        //    var response = new
        //    {
        //        carrito.IdCarrito,
        //        carrito.DNI,
        //        carrito.NombreCliente,
        //        carrito.ApellidoCliente,
        //        carrito.EmailCliente,
        //        carrito.FechaCreacion,
        //        carrito.Estado,
        //        carrito.MetodoPago,
        //        carrito.LatY,
        //        carrito.LongX,
        //        carrito.Total,
        //        Items = items
        //    };

        //    return Ok(response);
        //}
        // Listado de carritos por estado
        [HttpGet]
        public async Task<IActionResult> GetCarritosPorEstado([FromQuery] string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return BadRequest(new { mensaje = "Debe enviar el estado" });

            var carritos = await _carritoDAO.GetAllPorEstado(estado);

            if (carritos.Count == 0)
                return NotFound(new { mensaje = "no se encontro resultados" }); // o NotFound si prefieres

            return Ok(carritos);
        }


    }
}
