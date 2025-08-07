using Microsoft.EntityFrameworkCore;
using NotaFiscal.Models;

namespace NotaFiscal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações da entidade Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CpfCnpj)
                    .IsRequired()
                    .HasMaxLength(14)
                    .IsUnicode(false);

                entity.Property(e => e.NomeRazaoSocial)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                entity.Property(e => e.NomeFantasia)
                    .HasMaxLength(150)
                    .IsUnicode(true);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Telefone)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                // Índices
                entity.HasIndex(e => e.CpfCnpj)
                    .IsUnique()
                    .HasDatabaseName("IX_Cliente_CpfCnpj");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Cliente_Email");
            });

            // Configurações da entidade Venda
            modelBuilder.Entity<Venda>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdCliente)
                    .IsRequired();

                entity.Property(e => e.FormaPagamento)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.Data)
                    .IsRequired()
                    .HasColumnType("datetime2");

                entity.Property(e => e.ValorTotal)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)")
                    .HasPrecision(18, 2);

                // Relacionamento com Cliente
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Vendas)
                    .HasForeignKey(e => e.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Venda_Cliente");

                // Índices
                entity.HasIndex(e => e.IdCliente)
                    .HasDatabaseName("IX_Venda_IdCliente");

                entity.HasIndex(e => e.Data)
                    .HasDatabaseName("IX_Venda_Data");
            });

            // Configurações da entidade Endereco
            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.IdCliente)
                    .IsRequired();

                entity.Property(e => e.TipoEndereco)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(true);

                entity.Property(e => e.Logradouro)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(true);

                // Relacionamento com Cliente
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Enderecos)
                    .HasForeignKey(e => e.IdCliente)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Endereco_Cliente");

                // Índices
                entity.HasIndex(e => e.IdCliente)
                    .HasDatabaseName("IX_Endereco_IdCliente");
            });
        }
    }
}
