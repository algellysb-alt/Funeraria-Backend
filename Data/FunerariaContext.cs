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
            // Mapeo exacto a los nombres de tus tablas en SQL
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
            modelBuilder.Entity<Categoria>().ToTable("categorias");
            modelBuilder.Entity<ProductoCatalogo>().ToTable("catalogo");
            modelBuilder.Entity<Pedido>().ToTable("pedidos");
            modelBuilder.Entity<DetallePedido>().ToTable("detalle_pedido");
        }
    }
}