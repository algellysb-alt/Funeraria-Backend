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
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();

            int usuarioId = int.Parse(idClaim.Value);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null) return NotFound();

            // Seguridad: Ocultar password
            usuario.Password = "";
            return usuario;
        }

        // ---------------- ZONA ADMIN ----------------

        // GET: api/Usuarios (SOLO ADMIN) - Lista todos los clientes registrados
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            // Limpiamos passwords por seguridad
            usuarios.ForEach(u => u.Password = "");
            return usuarios;
        }

        // DELETE: api/Usuarios/{id} (SOLO ADMIN) - Borrar usuario problemático
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> BorrarUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}