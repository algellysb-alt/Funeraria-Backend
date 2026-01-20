using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FunerariaAPI.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("nombre_completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        public string Password { get; set; } = string.Empty;

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("rol")]
        public string Rol { get; set; } = "cliente";

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow; // Ajustado a UTC
    }

    [Table("categorias")]
    public class Categoria
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }

    [Table("catalogo")]
    public class ProductoCatalogo
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("categoria_id")]
        public int CategoriaId { get; set; }
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
        [Column("precio")]
        public decimal Precio { get; set; }
        [Column("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;
        [Column("activo")]
        public bool Activo { get; set; } = true;

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }
    }

    [Table("pedidos")]
    public class Pedido
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("fecha_solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow; // Ajustado a UTC

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("nombre_difunto")]
        public string NombreDifunto { get; set; } = string.Empty;

        [Column("fecha_servicio")]
        public DateTime FechaServicio { get; set; }

        [Column("total_estimado")]
        public decimal TotalEstimado { get; set; }

        [Column("notas_adicionales")]
        public string? NotasAdicionales { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // AJUSTE 1: Mapear explícitamente la relación inversa
        [InverseProperty("Pedido")]
        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }

    [Table("detalle_pedido")]
    public class DetallePedido
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("pedido_id")]
        public int PedidoId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [JsonIgnore]
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        // AJUSTE 2: Mapear la relación con Producto
        [ForeignKey("ItemId")]
        public ProductoCatalogo? Producto { get; set; }
    }

    // --- DTOs ---
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CrearPedidoDto
    {
        public string NombreDifunto { get; set; } = string.Empty;
        public DateTime FechaServicio { get; set; }
        public string Notas { get; set; } = string.Empty;
        public List<DetallePedidoDto> Items { get; set; } = new List<DetallePedidoDto>();
    }

    public class DetallePedidoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }
}
