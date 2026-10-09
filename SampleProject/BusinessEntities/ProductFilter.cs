namespace BusinessEntities
{
    /// <summary>Criteria for listing products; unset criteria do not filter.</summary>
    public class ProductFilter
    {
        /// <summary>A fragment of the name, matched ignoring case.</summary>
        public string Name { get; set; }

        public ProductCategories? Category { get; set; }

        /// <summary>Inclusive lower bound of the price.</summary>
        public decimal? MinPrice { get; set; }

        /// <summary>Inclusive upper bound of the price.</summary>
        public decimal? MaxPrice { get; set; }
    }
}