using Microsoft.EntityFrameworkCore;

public class ShopDbContext : DbContext
{
    public ShopDbContext(DbContextOptions<ShopDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Inventory> Inventories => Set<Inventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        modelBuilder.Entity<Brand>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(x => x.Slug)
            .IsUnique();

        modelBuilder.Entity<ProductVariant>()
            .HasIndex(x => new { x.ProductId, x.Sku })
            .IsUnique();

        modelBuilder.Entity<Inventory>()
            .HasIndex(x => x.ProductVariantId)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(x => x.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<ProductVariant>()
            .Property(x => x.RowVersion)
            .IsRowVersion();

        modelBuilder.Entity<Inventory>()
            .Property(x => x.RowVersion)
            .IsRowVersion();
    }
}
