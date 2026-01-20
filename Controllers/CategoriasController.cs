using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FunerariaAPI.Data;
using FunerariaAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FunerariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly FunerariaContext _context;

        public CategoriasController(FunerariaContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            return await _context.Categorias.ToListAsync();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return Ok(categoria);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var cat = await _context.Categorias.FindAsync(id);
            if (cat == null) return NotFound();
            _context.Categorias.Remove(cat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}