using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NorkysAPI.DTO;
using NorkysAPI.DTO.NorkysWebAPI.DTOs;
using NorkysAPI.Interfaces;
using NorkysAPI.Models;
using NorkysAPI.Operaciones;

namespace NorkysWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioDAO _usuarioDAO;

        public UsuarioController(IUsuarioDAO usuarioDAO)
        {
            _usuarioDAO = usuarioDAO;
        }

        // POST: api/Usuario/Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginInputDTO input)
        {
            var usuario = await _usuarioDAO.ValidateLogin(input.Email, input.Password);
            if (usuario == null) return Unauthorized("Email o contraseña incorrectos.");

            // Aquí podrías generar un token JWT si quieres autenticación real
            return Ok(usuario);
        }

        // POST: api/Usuario
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CrearUsuarioDTO dto)
        {
            try
            {
                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    Email = dto.Email,
                    PasswordHash = dto.PasswordHash,
                    EsAdmin = dto.EsAdmin
                };

                var saved = await _usuarioDAO.Add(usuario);

                if (saved == null)
                {
                    // aquí indicamos explícitamente que hubo un error
                    return BadRequest(new { mensaje = "El correo ya está registrado." });
                }

                return Ok(saved); // O Created("", saved)
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear usuario", error = ex.Message });
            }
        }


        // GET: api/Usuario/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _usuarioDAO.GetById(id); // si no tienes, usar FindAsync
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });
            return Ok(usuario);
        }

        // DELETE: api/Usuario/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _usuarioDAO.Delete(id);
            if (!result) return NotFound(new { mensaje = "Usuario no encontrado" });
            return NoContent();
        }

        // GET: api/Usuario/IsAdmin/{id}
        [HttpGet("IsAdmin/{id}")]
        public async Task<IActionResult> IsAdmin(int id)
        {
            var user = await _usuarioDAO.GetById(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no existe" });
            }

            var esAdmin = user.EsAdmin; // o user.IsAdmin, según tu campo

            return Ok(esAdmin);
        }
        // GET: api/Usuario/
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var items = await _usuarioDAO.GetAll();
            return Ok(items);
        }
        // PUT: api/Usuario/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _usuarioDAO.GetById(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            // Mapear DTO → Entidad
            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.DNI = dto.DNI;
            usuario.Email = dto.Email;

            var ok = await _usuarioDAO.UpdateUser(usuario);
            if (!ok)
                return BadRequest(new { mensaje = "No se pudo actualizar el usuario" });

            return NoContent(); // 204
        }

    }
}
