using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FunerariaAPI.Data;
using FunerariaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FunerariaAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly FunerariaContext _context;

        public UsuariosController(FunerariaContext context)
        {
            _context = context;
        }

        // ---------------- ZONA CLIENTE ----------------

        // GET: api/Usuarios/mi-perfil
        [HttpGet("mi-perfil")]
        public async Task<ActionResult<Usuario>> GetMiPerfil()
        {
            // Extraer ID de forma segura
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null || !int.TryParse(idClaim.Value, out int usuarioId)) 
                return Unauthorized("Usuario no identificado en el token.");

            // Usamos AsNoTracking porque solo vamos a mostrar los datos, no a editarlos
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null) return NotFound("Usuario no encontrado en la base de datos.");

            // Seguridad: Ocultar password antes de enviar al frontend
            usuario.Password = "";
            return Ok(usuario);
        }

        // ---------------- ZONA ADMIN ----------------

        // GET: api/Usuarios (SOLO ADMIN) - Lista todos los clientes registrados
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .ToListAsync();

            // Limpiamos passwords de toda la lista por seguridad
            usuarios.ForEach(u => u.Password = "");
            
            return Ok(usuarios);
        }

        // DELETE: api/Usuarios/{id} (SOLO ADMIN)
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("El usuario que intentas borrar no existe.");

            // Evitar que el admin se borre a sí mismo por error
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (adminIdClaim != null && int.Parse(adminIdClaim.Value) == id)
            {
                return BadRequest("No puedes eliminar tu propia cuenta de administrador.");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
