using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FunerariaAPI.Data;
using FunerariaAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FunerariaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoController : ControllerBase
    {
        private readonly FunerariaContext _context;

        public CatalogoController(FunerariaContext context)
        {
            _context = context;
        }

        // GET: Público
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoCatalogo>>> GetProductos()
        {
            return await _context.Catalogo.Include(p => p.Categoria).ToListAsync();
        }

        // POST: Privado
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ProductoCatalogo>> PostProducto(ProductoCatalogo producto)
        {
            _context.Catalogo.Add(producto);
            await _context.SaveChangesAsync();
            return Ok(producto);
        }

        // DELETE: Privado
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Catalogo.FindAsync(id);
            if (producto == null) return NotFound();
            _context.Catalogo.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}