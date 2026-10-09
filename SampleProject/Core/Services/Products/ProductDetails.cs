using BusinessEntities;

namespace Core.Services.Products
{
    /// <summary>The editable details of a product, as supplied to the create and update use cases.</summary>
    public class ProductDetails
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public ProductCategories Category { get; set; }
    }
}