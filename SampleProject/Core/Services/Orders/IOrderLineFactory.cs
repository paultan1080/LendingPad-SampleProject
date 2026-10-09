using System.Collections.Generic;
using BusinessEntities;

namespace Core.Services.Orders
{
    public interface IOrderLineFactory
    {
        /// <summary>
        /// Builds order lines for the requested items. Products already on the order keep the name and
        /// price agreed when they were added (only the quantity changes).
        /// </summary>
        IReadOnlyList<OrderLine> Create(IEnumerable<OrderLineDetails> lines, IEnumerable<OrderLine> currentLines);
    }
}