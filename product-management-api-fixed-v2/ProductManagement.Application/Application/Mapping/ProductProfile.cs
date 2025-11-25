using AutoMapper;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductSummaryDto>()
            .ForMember(d => d.CategoryName,
                opt => opt.MapFrom(s => Guard.NotNull(s.Category, "Product.Category").Name))
            .ForMember(d => d.BrandName,
                opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null));

        CreateMap<ProductImage, string>()
            .ConvertUsing(src => src.ImageUrl);

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForMember(d => d.Price,
                opt => opt.MapFrom(s =>
                    (s.Product != null ? s.Product.BasePrice : 0m) + s.AdditionalPrice))
            .ForMember(d => d.Quantity,
                opt => opt.MapFrom(s => s.Inventory != null ? s.Inventory.Quantity : 0));

        CreateMap<Product, ProductDetailDto>()
            .ForMember(d => d.CategoryName,
                opt => opt.MapFrom(s => Guard.NotNull(s.Category, "Product.Category").Name))
            .ForMember(d => d.BrandName,
                opt => opt.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
            .ForMember(d => d.ImageUrls,
                opt => opt.MapFrom(s =>
                    (s.Images ?? new List<ProductImage>())
                        .OrderBy(i => i.SortOrder)));

        CreateMap<CreateProductRequest, Product>()
            .ForMember(d => d.Slug, opt => opt.Ignore())
            .ForMember(d => d.Images, opt => opt.Ignore())
            .ForMember(d => d.Variants, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore());

        CreateMap<CreateProductVariantRequest, ProductVariant>()
            .ForMember(d => d.Inventory, opt => opt.Ignore())
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore())
            .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(d => d.RowVersion, opt => opt.Ignore())
            .ForMember(d => d.Images, opt => opt.Ignore())
            .ForMember(d => d.Variants, opt => opt.Ignore())
            .ForMember(d => d.Slug, opt => opt.Ignore());

        CreateMap<UpdateProductVariantRequest, ProductVariant>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.ProductId, opt => opt.Ignore())
            .ForMember(d => d.Inventory, opt => opt.Ignore())
            .ForMember(d => d.RowVersion, opt => opt.Ignore());
    }
}
