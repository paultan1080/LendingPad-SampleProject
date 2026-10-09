using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using BusinessEntities;
using Core.Services;
using Core.Services.Orders;
using WebApi.Models.Orders;

namespace WebApi.Controllers
{
    [RoutePrefix("orders")]
    public class OrderController : BaseApiController
    {
        private readonly ICreateOrderService _createOrderService;
        private readonly IDeleteOrderService _deleteOrderService;
        private readonly IGetOrderService _getOrderService;
        private readonly IUpdateOrderService _updateOrderService;

        public OrderController(ICreateOrderService createOrderService, IDeleteOrderService deleteOrderService, IGetOrderService getOrderService, IUpdateOrderService updateOrderService)
        {
            _createOrderService = createOrderService;
            _deleteOrderService = deleteOrderService;
            _getOrderService = getOrderService;
            _updateOrderService = updateOrderService;
        }

        [Route("{orderId:guid}/create")]
        [HttpPost]
        public HttpResponseMessage CreateOrder(Guid orderId, [FromBody] CreateOrderModel model)
        {
            var order = _createOrderService.Create(orderId, model.ToCreateOrderDetails());
            return Found(new OrderData(order));
        }

        [Route("{orderId:guid}/update")]
        [HttpPost]
        public HttpResponseMessage UpdateOrder(Guid orderId, [FromBody] UpdateOrderModel model)
        {
            var order = _getOrderService.GetOrder(orderId);
            if (order == null)
            {
                return DoesNotExist();
            }
            _updateOrderService.Update(order, model.ToUpdateOrderDetails());
            return Found(new OrderData(order));
        }

        [Route("{orderId:guid}/delete")]
        [HttpDelete]
        public HttpResponseMessage DeleteOrder(Guid orderId)
        {
            var order = _getOrderService.GetOrder(orderId);
            if (order == null)
            {
                return DoesNotExist();
            }
            _deleteOrderService.Delete(order);
            return Found();
        }

        [Route("{orderId:guid}")]
        [HttpGet]
        public HttpResponseMessage GetOrder(Guid orderId)
        {
            var order = _getOrderService.GetOrder(orderId);
            if (order == null)
            {
                return DoesNotExist();
            }
            return Found(new OrderData(order));
        }

        /// <summary>Newest first. placedFrom is inclusive, placedBefore exclusive; times without a zone default to UTC.</summary>
        [Route("list")]
        [HttpGet]
        public HttpResponseMessage GetOrders(int skip = 0, int take = Paging.DefaultPageSize, Guid? customerId = null, OrderStatuses? status = null, Guid? productId = null, DateTime? placedFrom = null, DateTime? placedBefore = null)
        {
            var filter = new OrderFilter
                         {
                             CustomerId = customerId,
                             Status = status,
                             ProductId = productId,
                             PlacedFrom = placedFrom,
                             PlacedBefore = placedBefore
                         };
            var orders = _getOrderService.GetOrders(filter, skip, take)
                                         .Select(q => new OrderData(q))
                                         .ToList();
            return Found(orders);
        }
    }
}