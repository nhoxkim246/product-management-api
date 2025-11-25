using System.ComponentModel.DataAnnotations;

public class Category
{
    public long Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = default!;

    [Required, StringLength(255)]
    public string Slug { get; set; } = default!;

    public long? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class Brand
{
    public long Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = default!;

    [Required, StringLength(255)]
    public string Slug { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class Product
{
    public long Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = default!;

    [Required, StringLength(255)]
    public string Slug { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    public long CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public long? BrandId { get; set; }
    public Brand? Brand { get; set; }

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}

public class ProductVariant
{
    public long Id { get; set; }

    [Required]
    public long ProductId { get; set; }
    public Product Product { get; set; } = default!;

    [Required, StringLength(64)]
    public string Sku { get; set; } = default!;

    [StringLength(64)]
    public string? Color { get; set; }

    [StringLength(64)]
    public string? Size { get; set; }

    [Range(0, double.MaxValue)]
    public decimal AdditionalPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

    public Inventory? Inventory { get; set; }
}

public class ProductImage
{
    public long Id { get; set; }

    [Required]
    public long ProductId { get; set; }
    public Product Product { get; set; } = default!;

    [Required]
    public string ImageUrl { get; set; } = default!;

    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class Inventory
{
    public long Id { get; set; }

    [Required]
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = default!;

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue)]
    public int Reserved { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
}
