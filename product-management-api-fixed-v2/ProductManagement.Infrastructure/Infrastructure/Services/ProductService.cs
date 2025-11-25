using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

public class ProductService : IProductService
{
    private readonly ShopDbContext _db;
    private readonly IProductRepository _repo;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;

    private static readonly TimeSpan ProductCacheDuration = TimeSpan.FromMinutes(10);

    public ProductService(
        ShopDbContext db,
        IProductRepository repo,
        IMemoryCache cache,
        IMapper mapper)
    {
        _db = db;
        _repo = repo;
        _cache = cache;
        _mapper = mapper;
    }

    public Task<PagedResult<ProductSummaryDto>> GetPagedProductsAsync(
        int page,
        int pageSize,
        long? categoryId,
        long? brandId,
        string? search,
        CancellationToken cancellationToken = default)
        => _repo.GetPagedAsync(page, pageSize, categoryId, brandId, search, cancellationToken);

    public async Task<ProductDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"product:detail:{id}";

        if (_cache.TryGetValue(cacheKey, out ProductDetailDto cached))
            return cached;

        var product = await _repo.GetProductWithDetailsAsync(id, cancellationToken);
        if (product == null)
            return null;

        var dto = _mapper.Map<ProductDetailDto>(product);

        _cache.Set(cacheKey, dto, ProductCacheDuration);
        return dto;
    }

    public async Task<ProductDetailDto> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _repo.CategoryExistsAsync(request.CategoryId, cancellationToken))
            throw new InvalidOperationException("Category not found.");

        if (request.BrandId.HasValue &&
            !await _repo.BrandExistsAsync(request.BrandId.Value, cancellationToken))
            throw new InvalidOperationException("Brand not found.");

        var now = DateTimeOffset.UtcNow;

        var product = _mapper.Map<Product>(request);

        product.Slug = string.IsNullOrWhiteSpace(request.Slug)
            ? Slugify(request.Name)
            : request.Slug!;

        product.CreatedAt = now;
        product.UpdatedAt = now;

        if (request.ImageUrls is not null)
        {
            foreach (var img in request.ImageUrls.Distinct())
            {
                if (string.IsNullOrWhiteSpace(img))
                    continue;

                product.Images.Add(new ProductImage
                {
                    ImageUrl = img,
                    IsPrimary = false,
                    SortOrder = product.Images.Count
                });
            }
        }

        if (request.Variants is not null)
        {
            foreach (var vReq in request.Variants)
            {
                var variant = _mapper.Map<ProductVariant>(vReq);
                variant.CreatedAt = now;
                variant.UpdatedAt = now;

                if (vReq.InitialQuantity > 0)
                {
                    variant.Inventory = new Inventory
                    {
                        Quantity = vReq.InitialQuantity,
                        Reserved = 0,
                        UpdatedAt = now
                    };
                }

                product.Variants.Add(variant);
            }
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        InvalidateListCache();

        var freshProduct = await _repo.GetProductWithDetailsAsync(product.Id, cancellationToken);
        return _mapper.Map<ProductDetailDto>(Guard.NotNull(freshProduct, "Product"));
    }

    public async Task<ProductDetailDto?> UpdateAsync(
        long id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
                .ThenInclude(v => v.Inventory)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (product == null)
            return null;

        _db.Entry(product).Property(p => p.RowVersion).OriginalValue = request.RowVersion;

        if (!await _repo.CategoryExistsAsync(request.CategoryId, cancellationToken))
            throw new InvalidOperationException("Category not found.");

        if (request.BrandId.HasValue &&
            !await _repo.BrandExistsAsync(request.BrandId.Value, cancellationToken))
            throw new InvalidOperationException("Brand not found.");

        var now = DateTimeOffset.UtcNow;

        _mapper.Map(request, product);
        product.UpdatedAt = now;

        product.Images.Clear();
        if (request.ImageUrls is not null)
        {
            foreach (var img in request.ImageUrls.Distinct())
            {
                if (string.IsNullOrWhiteSpace(img))
                    continue;

                product.Images.Add(new ProductImage
                {
                    ImageUrl = img,
                    IsPrimary = false,
                    SortOrder = product.Images.Count
                });
            }
        }

        var existingVariants = product.Variants.ToDictionary(v => v.Id);
        var requestIds = new HashSet<long>();

        if (request.Variants is not null)
        {
            foreach (var vReq in request.Variants)
            {
                if (vReq.Id.HasValue && existingVariants.TryGetValue(vReq.Id.Value, out var existing))
                {
                    if (vReq.RowVersion != null)
                    {
                        _db.Entry(existing).Property(x => x.RowVersion).OriginalValue = vReq.RowVersion;
                    }

                    _mapper.Map(vReq, existing);
                    existing.UpdatedAt = now;

                    if (existing.Inventory == null)
                    {
                        existing.Inventory = new Inventory
                        {
                            Quantity = vReq.Quantity,
                            Reserved = 0,
                            UpdatedAt = now
                        };
                    }
                    else
                    {
                        existing.Inventory.Quantity = vReq.Quantity;
                        existing.Inventory.UpdatedAt = now;
                    }

                    requestIds.Add(existing.Id);
                }
                else
                {
                    var newVariant = _mapper.Map<ProductVariant>(vReq);
                    newVariant.CreatedAt = now;
                    newVariant.UpdatedAt = now;

                    newVariant.Inventory = new Inventory
                    {
                        Quantity = vReq.Quantity,
                        Reserved = 0,
                        UpdatedAt = now
                    };

                    product.Variants.Add(newVariant);
                }
            }
        }

        var removed = product.Variants
            .Where(v => requestIds.Count > 0 && !requestIds.Contains(v.Id))
            .ToList();

        foreach (var r in removed)
        {
            _db.ProductVariants.Remove(r);
        }

        await _db.SaveChangesAsync(cancellationToken);

        InvalidateDetailCache(id);
        InvalidateListCache();

        var freshProduct = await _repo.GetProductWithDetailsAsync(id, cancellationToken);
        return _mapper.Map<ProductDetailDto>(Guard.NotNull(freshProduct, "Product"));
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product == null)
            return false;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        InvalidateDetailCache(id);
        InvalidateListCache();

        return true;
    }

    public async Task AdjustInventoryAsync(
        long variantId,
        AdjustInventoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var inventory = await _db.Inventories
            .SingleOrDefaultAsync(x => x.ProductVariantId == variantId, cancellationToken);

        inventory = Guard.NotNull(inventory, "Inventory");

        _db.Entry(inventory).Property(i => i.RowVersion).OriginalValue = request.RowVersion;

        var newQty = inventory.Quantity + request.Delta;
        if (newQty < 0)
            throw new InvalidOperationException("Insufficient stock.");

        inventory.Quantity = newQty;
        inventory.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var variant = await _db.ProductVariants
            .AsNoTracking()
            .SingleAsync(v => v.Id == variantId, cancellationToken);

        InvalidateDetailCache(variant.ProductId);
    }

    private static string Slugify(string name)
    {
        return name.ToLower().Replace(" ", "-");
    }

    private void InvalidateDetailCache(long productId)
    {
        _cache.Remove($"product:detail:{productId}");
    }

    private void InvalidateListCache()
    {
        // extend if you cache list
    }
}
