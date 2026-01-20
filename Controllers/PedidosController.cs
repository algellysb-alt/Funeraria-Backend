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

        public PedidosController(FunerariaContext context)
        {
            _context = context;
        }

        // --- ZONA CLIENTE (VER MIS PEDIDOS) ---
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetMisPedidos()
        {
            int usuarioId = ObtenerIdUsuario();

            // Usamos AsNoTracking para evitar carga excesiva en Render
            return await _context.Pedidos
                .AsNoTracking() 
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
        }

        // --- POST: CREAR PEDIDO (CORREGIDO PARA EVITAR ERROR 500) ---
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(CrearPedidoDto pedidoDto)
        {
            int usuarioId = ObtenerIdUsuario();

            // Validación crítica para evitar errores en la base de datos
            if (pedidoDto.Items == null || !pedidoDto.Items.Any())
                return BadRequest("El pedido no puede estar vacío.");

            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId,
                NombreDifunto = pedidoDto.NombreDifunto,
                FechaServicio = pedidoDto.FechaServicio,
                NotasAdicionales = pedidoDto.Notas,
                FechaSolicitud = DateTime.UtcNow, // UTC es mejor para la nube
                Estado = "pendiente",
                TotalEstimado = 0,
                Detalles = new List<DetallePedido>() // Inicialización preventiva
            };

            decimal total = 0;
            foreach (var item in pedidoDto.Items)
            {
                // Buscamos el producto por ID real
                var prod = await _context.Catalogo.FindAsync(item.ProductoId);
                if (prod != null)
                {
                    nuevoPedido.Detalles.Add(new DetallePedido
                    {
                        ItemId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = prod.Precio
                    });
                    total += (prod.Precio * item.Cantidad);
                }
            }
            nuevoPedido.TotalEstimado = total;

            _context.Pedidos.Add(nuevoPedido);
            await _context.SaveChangesAsync();

            return Ok(nuevoPedido);
        }

        // --- ZONA ADMIN (ALIMENTA EL PANEL DE CONTROL) ---
        [Authorize(Roles = "admin")]
        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetTodosLosPedidos()
        {
            // El Include de Usuario es vital para el Panel Admin
            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.Usuario) 
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, [FromBody] string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado.ToLower(); // Estandarizar a minúsculas
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private int ObtenerIdUsuario()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var idClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);
            
            if (idClaim != null && int.TryParse(idClaim.Value, out int id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Usuario no válido.");
        }
    }
}
