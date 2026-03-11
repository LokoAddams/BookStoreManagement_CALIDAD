namespace MicroServiceReports.Infraestructure.Persistence
{
    using Microsoft.EntityFrameworkCore;
    using MicroServiceReports.Domain.Models;

    public class MicroServiceReportsDbContext : DbContext
    {
        public MicroServiceReportsDbContext(DbContextOptions<MicroServiceReportsDbContext> options)
            : base(options)
        {
        }

        public DbSet<SaleEventRecord> SaleEventRecords { get; set; } = null!;
        public DbSet<SaleDetailRecord> SaleDetailRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entity to table with PascalCase column names (as created in DB)
            modelBuilder.Entity<SaleEventRecord>(b =>
            {
                b.ToTable("sale_event_records");
                b.HasKey(x => x.Id);

                // Map properties to PascalCase column names with quotes
                b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                b.Property(x => x.SaleId).HasColumnName("SaleId").IsRequired().HasMaxLength(50);
                b.Property(x => x.Payload).HasColumnName("Payload").IsRequired();
                b.Property(x => x.Exchange).HasColumnName("Exchange").HasMaxLength(200);
                b.Property(x => x.RoutingKey).HasColumnName("RoutingKey").HasMaxLength(200);
                b.Property(x => x.ReceivedAt).HasColumnName("ReceivedAt").IsRequired();

                b.HasIndex(x => x.SaleId).IsUnique(false);
            });

            // Map SaleDetailRecord to sale_details table
            modelBuilder.Entity<SaleDetailRecord>(b =>
            {
                b.ToTable("sale_details");
                b.HasKey(x => x.Id);

                b.Property(x => x.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                b.Property(x => x.SaleId).HasColumnName("SaleId").IsRequired().HasMaxLength(50);
                b.Property(x => x.ProductId).HasColumnName("ProductId").IsRequired().HasMaxLength(50);
                b.Property(x => x.ProductName).HasColumnName("ProductName").HasMaxLength(200);
                b.Property(x => x.Quantity).HasColumnName("Quantity").IsRequired();
                b.Property(x => x.UnitPrice).HasColumnName("UnitPrice").HasColumnType("decimal(18,2)").IsRequired();
                b.Property(x => x.Subtotal).HasColumnName("Subtotal").HasColumnType("decimal(18,2)").IsRequired();
                b.Property(x => x.CreatedAt).HasColumnName("CreatedAt").IsRequired();

                b.HasIndex(x => x.SaleId).IsUnique(false);
            });
        }
    }
}
