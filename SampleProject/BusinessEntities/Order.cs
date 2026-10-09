using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extensions;

namespace BusinessEntities
{
    /// <summary>
    /// A customer's order. Always has at least one line and at most one line per product; its
    /// lines can change only while it is Placed, and its status follows a fixed lifecycle:
    /// Placed -> Paid -> Shipped -> Delivered, with cancellation possible until it ships.
    /// </summary>
    public class Order : IdObject
    {
        private static readonly IDictionary<OrderStatuses, OrderStatuses[]> AllowedTransitions =
            new Dictionary<OrderStatuses, OrderStatuses[]>
            {
                { OrderStatuses.Placed, new[] { OrderStatuses.Paid, OrderStatuses.Cancelled } },
                { OrderStatuses.Paid, new[] { OrderStatuses.Shipped, OrderStatuses.Cancelled } },
                { OrderStatuses.Shipped, new[] { OrderStatuses.Delivered } },
                { OrderStatuses.Delivered, new OrderStatuses[0] },
                { OrderStatuses.Cancelled, new OrderStatuses[0] }
            };

        private readonly List<OrderLine> _lines = new List<OrderLine>();
        private Guid _customerId;
        private DateTime _placedOn;
        private OrderStatuses _status = OrderStatuses.Placed;

        public Guid CustomerId
        {
            get => _customerId;
            private set => _customerId = value;
        }

        /// <summary>When the order was placed, in UTC.</summary>
        public DateTime PlacedOn
        {
            get => _placedOn;
            private set => _placedOn = value;
        }

        public OrderStatuses Status
        {
            get => _status;
            private set => _status = value;
        }

        public IEnumerable<OrderLine> Lines
        {
            get => _lines.AsReadOnly();
            private set => _lines.Initialize(value);
        }

        public decimal Total => _lines.Sum(line => line.Total);

        public bool CanBeDeleted => _status == OrderStatuses.Placed || _status == OrderStatuses.Cancelled;

        public void Place(Guid customerId, IEnumerable<OrderLine> lines, DateTime placedOnUtc)
        {
            if (_customerId != Guid.Empty)
            {
                throw new InvalidOperationException($"Order '{Id}' has already been placed.");
            }
            if (customerId == Guid.Empty)
            {
                throw new DomainValidationException(nameof(customerId), "A customer is required.");
            }

            SetLines(lines);
            _customerId = customerId;
            _placedOn = placedOnUtc;
            _status = OrderStatuses.Placed;
        }

        public void ReplaceLines(IEnumerable<OrderLine> lines)
        {
            if (_status != OrderStatuses.Placed)
            {
                throw new ConflictException($"The lines of an order cannot change once it is {_status}.");
            }
            SetLines(lines);
        }

        public void ChangeStatus(OrderStatuses status)
        {
            if (!Enum.IsDefined(typeof(OrderStatuses), status))
            {
                throw new DomainValidationException(nameof(status), $"'{status}' is not a valid order status.");
            }
            if (status == _status)
            {
                return;
            }
            if (!AllowedTransitions[_status].Contains(status))
            {
                throw new ConflictException($"An order that is {_status} cannot become {status}.");
            }
            _status = status;
        }

        public void EnsureCanBeDeleted()
        {
            if (!CanBeDeleted)
            {
                throw new ConflictException($"An order that is {_status} cannot be deleted; only placed or cancelled orders can.");
            }
        }

        private void SetLines(IEnumerable<OrderLine> lines)
        {
            var newLines = lines?.ToList() ?? new List<OrderLine>();
            if (newLines.Count == 0 || newLines.Any(line => line == null))
            {
                throw new DomainValidationException("lines", "An order needs at least one line.");
            }
            if (newLines.GroupBy(line => line.ProductId).Any(group => group.Count() > 1))
            {
                throw new DomainValidationException("lines", "Each product can appear on only one line.");
            }
            _lines.Initialize(newLines);
        }
    }
}