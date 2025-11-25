public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long TotalItems { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
}

public class ProductSummaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public bool IsPublished { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? BrandName { get; set; }
}

public class ProductVariantDto
{
    public long Id { get; set; }
    public string Sku { get; set; } = default!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int Quantity { get; set; }
}

public class ProductDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsPublished { get; set; }
    public string CategoryName { get; set; } = default!;
    public string? BrandName { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public byte[] RowVersion { get; set; } = default!;
}

public class CreateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public long CategoryId { get; set; }
    public long? BrandId { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsPublished { get; set; }

    public List<CreateProductVariantRequest> Variants { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
}

public class CreateProductVariantRequest
{
    public string Sku { get; set; } = default!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal AdditionalPrice { get; set; }
    public int InitialQuantity { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public long CategoryId { get; set; }
    public long? BrandId { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsPublished { get; set; }

    public List<UpdateProductVariantRequest> Variants { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public byte[] RowVersion { get; set; } = default!;
}

public class UpdateProductVariantRequest
{
    public long? Id { get; set; }
    public string Sku { get; set; } = default!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; }
    public int Quantity { get; set; }
    public byte[]? RowVersion { get; set; }
}

public class AdjustInventoryRequest
{
    public int Delta { get; set; }
    public byte[] RowVersion { get; set; } = default!;
}
