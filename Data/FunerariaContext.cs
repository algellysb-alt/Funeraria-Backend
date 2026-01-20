using FunerariaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FunerariaAPI.Data
{
    public class FunerariaContext : DbContext
    {
        public FunerariaContext(DbContextOptions<FunerariaContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<ProductoCatalogo> Catalogo { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Mapeo de Tablas
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<ProductoCatalogo>().ToTable("catalogo");
            modelBuilder.Entity<Pedido>().ToTable("pedidos");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");

            // 2. Configuración de Relación Pedido -> Detalles (Clave Foránea)
            // Esto evita el Error 500 al hacer .Include(p => p.Detalles)
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.PedidoId);

            // 3. Configuración de Relación Detalle -> Producto
            modelBuilder.Entity<DetallePedido>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ItemId);

            // 4. Configuración de precisión para decimales (Evita truncado de dinero)
            modelBuilder.Entity<ProductoCatalogo>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<DetallePedido>()
                .Property(d => d.PrecioUnitario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Pedido>()
                .Property(p => p.TotalEstimado)
                .HasPrecision(18, 2);
        }
    }
}
