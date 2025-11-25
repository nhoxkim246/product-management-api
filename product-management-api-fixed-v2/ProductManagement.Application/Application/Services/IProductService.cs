using System.Threading;
using System.Threading.Tasks;

public interface IProductService
{
    Task<PagedResult<ProductSummaryDto>> GetPagedProductsAsync(
        int page,
        int pageSize,
        long? categoryId,
        long? brandId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<ProductDetailDto?> GetDetailAsync(long id, CancellationToken cancellationToken = default);

    Task<ProductDetailDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> UpdateAsync(long id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);

    Task AdjustInventoryAsync(long variantId, AdjustInventoryRequest request, CancellationToken cancellationToken = default);
}
