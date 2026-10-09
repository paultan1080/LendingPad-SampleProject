using System;

namespace BusinessEntities
{
    /// <summary>Criteria for listing orders; unset criteria do not filter.</summary>
    public class OrderFilter
    {
        public Guid? CustomerId { get; set; }

        public OrderStatuses? Status { get; set; }

        /// <summary>Orders with a line for this product.</summary>
        public Guid? ProductId { get; set; }

        /// <summary>Inclusive lower bound of when the order was placed (UTC).</summary>
        public DateTime? PlacedFrom { get; set; }

        /// <summary>Exclusive upper bound of when the order was placed (UTC).</summary>
        public DateTime? PlacedBefore { get; set; }
    }
}