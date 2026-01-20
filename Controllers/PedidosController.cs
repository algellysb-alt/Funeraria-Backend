using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FunerariaAPI.Data;
using FunerariaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FunerariaAPI.Controllers
{
    [Authorize] // 🔒 Todo requiere Token por defecto
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly FunerariaContext _context;

        public PedidosController(FunerariaContext context)
        {
            _context = context;
        }

        // ---------------- ZONA CLIENTE (VER LO SUYO) ----------------

        // GET: api/Pedidos (Devuelve SOLO los pedidos del usuario logueado)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetMisPedidos()
        {
            int usuarioId = ObtenerIdUsuario();

            return await _context.Pedidos
                .Where(p => p.UsuarioId == usuarioId)
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
        }

        // POST: api/Pedidos (Crear nuevo pedido)
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(CrearPedidoDto pedidoDto)
        {
            int usuarioId = ObtenerIdUsuario();

            var nuevoPedido = new Pedido
            {
                UsuarioId = usuarioId,
                NombreDifunto = pedidoDto.NombreDifunto,
                FechaServicio = pedidoDto.FechaServicio,
                NotasAdicionales = pedidoDto.Notas,
                FechaSolicitud = DateTime.Now,
                Estado = "pendiente",
                TotalEstimado = 0
            };

            decimal total = 0;

            foreach (var item in pedidoDto.Items)
            {
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

            return CreatedAtAction(nameof(GetMisPedidos), new { id = nuevoPedido.Id }, nuevoPedido);
        }

        // ---------------- ZONA ADMIN (PODER TOTAL) ----------------

        // GET: api/Pedidos/todos (SOLO ADMIN) - Ve los pedidos de TODO el mundo
        [Authorize(Roles = "admin")]
        [HttpGet("todos")]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetTodosLosPedidos()
        {
            return await _context.Pedidos
                .Include(p => p.Usuario) // Importante: Incluir datos del cliente
                .Include(p => p.Detalles)
                .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaSolicitud)
                .ToListAsync();
        }

        // PUT: api/Pedidos/{id}/estado (SOLO ADMIN) - Cambia el estado (ej: pendiente -> aprobado)
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

        // ---------------- FUNCIONES AUXILIARES ----------------

        private int ObtenerIdUsuario()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var idClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int id))
                {
                    return id;
                }
            }
            // Si llegamos aquí es un error grave de autenticación
            throw new Exception("No se pudo identificar al usuario en el token.");
        }
    }
}