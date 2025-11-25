using FluentValidation;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0);

        RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantRequestValidator());
    }
}

public class CreateProductVariantRequestValidator : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.AdditionalPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.InitialQuantity)
            .GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.RowVersion)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Variants).SetValidator(new UpdateProductVariantRequestValidator());
    }
}

public class UpdateProductVariantRequestValidator : AbstractValidator<UpdateProductVariantRequest>
{
    public UpdateProductVariantRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.AdditionalPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);
    }
}

public class AdjustInventoryRequestValidator : AbstractValidator<AdjustInventoryRequest>
{
    public AdjustInventoryRequestValidator()
    {
        RuleFor(x => x.Delta)
            .NotEqual(0);

        RuleFor(x => x.RowVersion)
            .NotNull()
            .NotEmpty();
    }
}
