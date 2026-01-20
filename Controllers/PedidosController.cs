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
    public class PedidosController : ControllerBase
    {
        private readonly FunerariaContext _context;
        public PedidosController(FunerariaContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetMisPedidos()
        {
            int usuarioId = ObtenerIdUsuario();
            return await _context.Pedidos.AsNoTracking()
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(CrearPedidoDto pedidoDto)
        {
            int usuarioId = ObtenerIdUsuario();
            if (pedidoDto.Items == null || !pedidoDto.Items.Any()) return BadRequest("Pedido vacío.");

            var nuevoPedido = new Pedido {
                UsuarioId = usuarioId,
                NombreDifunto = pedidoDto.NombreDifunto,
                FechaServicio = DateTime.SpecifyKind(pedidoDto.FechaServicio, DateTimeKind.Utc),
                NotasAdicionales = pedidoDto.Notas,
                FechaSolicitud = DateTime.UtcNow,
                Estado = "pendiente",
                TotalEstimado = 0,
                Detalles = new List<DetallePedido>()
            };

            decimal total = 0;
            foreach (var item in pedidoDto.Items) {
                var prod = await _context.Catalogo.FindAsync(item.ProductoId);
                if (prod != null) {
                    nuevoPedido.Detalles.Add(new DetallePedido {
                        ItemId = item.ProductoId, Cantidad = item.Cantidad, PrecioUnitario = prod.Precio
                    });
                    total += (prod.Precio * item.Cantidad);
                }
            }
            nuevoPedido.TotalEstimado = total;
            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();
            return Ok(nuevoPedido);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetTodosLosPedidos()
        {
            return await _context.Pedidos.AsNoTracking()
                .Include(p => p.Usuario)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud).ToListAsync();
        }

        // --- MÉTODO PARA EL BOTÓN ACTUALIZAR DEL PANEL ---
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int ObtenerIdUsuario() {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int id)) return id;
            throw new UnauthorizedAccessException();
        }
    }
}
