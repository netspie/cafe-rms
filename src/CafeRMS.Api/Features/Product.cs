namespace CafeRMS.Api.Features;

public class Product
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Barcode { get; init; } = "";
    public int TaxRateId { get; set; }
}

public class ProductImage
{
    public int Id { get; private init; }
    public int ProductId { get; set; }
    public string Url { get; init; } = "";
}

public class ProductTag
{
    public int ProductId { get; set; }
    public int TagId { get; set; }
}

public class ProductAllergen
{
    public int ProductId { get; set; }
    public int AllergenId { get; set; }
}

public class ProductPrice
{
    public int Id { get; private init; }
    public int ProductId { get; set; }
    public int PriceGroupId { get; set; }
    public decimal Net { get; set; }
}

public class ProductModifierGroup
{
    public int ProductId { get; set; }
    public int ModifierGroupId { get; set; }
}

public class ProductList
{
    public int Id { get; private init; }
    public string Name { get; init; } = "";
}

public class ProductListItem
{
    public int ProductListId { get; set; }
    public int ProductId { get; set; }
}
