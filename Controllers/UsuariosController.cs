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
            // Extraer ID de forma segura desde el Claim NameIdentifier
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null || !int.TryParse(idClaim.Value, out int usuarioId)) 
                return Unauthorized("No se pudo identificar al usuario en el token.");

            // AsNoTracking mejora el rendimiento al no rastrear cambios en el objeto
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            // Seguridad: El password nunca debe viajar al Frontend
            usuario.Password = ""; 
            return Ok(usuario);
        }

        // ---------------- ZONA ADMIN (ALIMENTA TU PANEL) ----------------

        // GET: api/Usuarios (SOLO ADMIN) - Lista para la pestaña "Directorio de Clientes"
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Traemos todos los usuarios mapeando a los nombres de columna en minúscula definidos en tu Modelo
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .ToListAsync();

            // Limpiamos los passwords de todos los registros por seguridad
            usuarios.ForEach(u => u.Password = "");
            
            return Ok(usuarios);
        }

        // DELETE: api/Usuarios/{id} (SOLO ADMIN) - Borrar usuario desde el Panel
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound("El usuario no existe.");

            // Protección: Evitar que el administrador se borre a sí mismo
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
