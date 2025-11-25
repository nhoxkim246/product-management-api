using Microsoft.EntityFrameworkCore;

public interface IProductRepository
{
    Task<PagedResult<ProductSummaryDto>> GetPagedAsync(
        int page,
        int pageSize,
        long? categoryId,
        long? brandId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<Product?> GetProductWithDetailsAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> CategoryExistsAsync(long categoryId, CancellationToken cancellationToken = default);
    Task<bool> BrandExistsAsync(long brandId, CancellationToken cancellationToken = default);
}

public class ProductRepository : IProductRepository
{
    private readonly ShopDbContext _db;

    public ProductRepository(ShopDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProductSummaryDto>> GetPagedAsync(
        int page,
        int pageSize,
        long? categoryId,
        long? brandId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLower();
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{keyword}%") ||
                EF.Functions.ILike(p.Slug, $"%{keyword}%"));
        }

        var total = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                BasePrice = p.BasePrice,
                IsPublished = p.IsPublished,
                CategoryName = p.Category.Name,
                BrandName = p.Brand != null ? p.Brand.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<Product?> GetProductWithDetailsAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants)
                .ThenInclude(v => v.Inventory)
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(long categoryId, CancellationToken cancellationToken = default)
        => _db.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);

    public Task<bool> BrandExistsAsync(long brandId, CancellationToken cancellationToken = default)
        => _db.Brands.AnyAsync(x => x.Id == brandId, cancellationToken);
}
