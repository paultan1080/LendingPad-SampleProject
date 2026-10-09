using System.ComponentModel.DataAnnotations;
using BusinessEntities;
using Core.Services.Products;

namespace WebApi.Models.Products
{
    public class ProductModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        [Range(0, 1000000000, ErrorMessage = "The Price field must be between 0 and 1000000000.")]
        public decimal? Price { get; set; }

        [Required]
        [EnumDataType(typeof(ProductCategories))]
        public ProductCategories? Category { get; set; }

        /// <summary>
        /// Maps the request to the create/update use cases. Call only on a validated model
        /// (ValidateModelAttribute runs globally), since required values are unwrapped.
        /// </summary>
        public ProductDetails ToProductDetails()
        {
            return new ProductDetails
                   {
                       Name = Name,
                       Description = Description,
                       Price = Price.Value,
                       Category = Category.Value
                   };
        }
    }
}