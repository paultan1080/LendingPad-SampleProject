using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessEntities;
using Core.Factories;
using Core.Services;
using Core.Services.Orders;
using Moq;
using WebApi.Controllers;
using WebApi.Models.Orders;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<ICreateOrderService> _createOrderService = new Mock<ICreateOrderService>();
        private readonly Mock<IDeleteOrderService> _deleteOrderService = new Mock<IDeleteOrderService>();
        private readonly Mock<IGetOrderService> _getOrderService = new Mock<IGetOrderService>();
        private readonly Mock<IUpdateOrderService> _updateOrderService = new Mock<IUpdateOrderService>();

        [Fact]
        public void CreateOrder_PassesTheRequestAsDetails_IgnoringNullLines()
        {
            var id = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var model = new CreateOrderModel { CustomerId = customerId, Lines = new[] { new OrderLineModel { ProductId = productId, Quantity = 2 }, null } };
            _createOrderService.Setup(s => s.Create(id, It.IsAny<CreateOrderDetails>())).Returns(PlacedOrder());

            var response = CreateController().CreateOrder(id, model);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _createOrderService.Verify(s => s.Create(id, It.Is<CreateOrderDetails>(d =>
                d.CustomerId == customerId && d.Lines.Single().ProductId == productId && d.Lines.Single().Quantity == 2)), Times.Once);
        }

        [Fact]
        public void GetOrder_ReturnsLinesAndTotal()
        {
            var order = PlacedOrder();
            _getOrderService.Setup(s => s.GetOrder(order.Id)).Returns(order);

            var data = (OrderData)((ObjectContent)CreateController().GetOrder(order.Id).Content).Value;

            Assert.Equal("Placed", data.Status.Key);
            Assert.Equal(51m, data.Total);
            var line = Assert.Single(data.Lines);
            Assert.Equal(new object[] { "Mouse", 25.5m, 2, 51m }, new object[] { line.ProductName, line.UnitPrice, line.Quantity, line.Total });
        }

        [Fact]
        public void UnknownOrder_GetUpdateAndDelete_Return404WithoutSideEffects()
        {
            var controller = CreateController();
            var id = Guid.NewGuid();

            Assert.Equal(HttpStatusCode.NotFound, controller.GetOrder(id).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, controller.UpdateOrder(id, new UpdateOrderModel()).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, controller.DeleteOrder(id).StatusCode);
            _updateOrderService.Verify(s => s.Update(It.IsAny<Order>(), It.IsAny<UpdateOrderDetails>()), Times.Never);
            _deleteOrderService.Verify(s => s.Delete(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public void GetOrders_BuildsTheFilterAndUsesTheDefaultPage()
        {
            var customerId = Guid.NewGuid();
            var from = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
            _getOrderService.Setup(s => s.GetOrders(It.IsAny<OrderFilter>(), 0, Paging.DefaultPageSize)).Returns(new[] { PlacedOrder() });

            var response = CreateController().GetOrders(customerId: customerId, status: OrderStatuses.Paid, placedFrom: from);

            Assert.Single((IEnumerable<OrderData>)((ObjectContent)response.Content).Value);
            _getOrderService.Verify(s => s.GetOrders(It.Is<OrderFilter>(f =>
                f.CustomerId == customerId && f.Status == OrderStatuses.Paid && f.ProductId == null && f.PlacedFrom == from && f.PlacedBefore == null), 0, Paging.DefaultPageSize));
        }

        private static Order PlacedOrder()
        {
            var mouse = new IdObjectFactory<Product>().Create(Guid.NewGuid());
            mouse.SetName("Mouse");
            mouse.SetPrice(25.5m);
            var order = new IdObjectFactory<Order>().Create(Guid.NewGuid());
            order.Place(Guid.NewGuid(), new[] { new OrderLine(mouse, 2) }, new DateTime(2026, 10, 8, 9, 30, 0, DateTimeKind.Utc));
            return order;
        }

        private OrderController CreateController()
        {
            var request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());
            return new OrderController(_createOrderService.Object, _deleteOrderService.Object, _getOrderService.Object, _updateOrderService.Object)
                   {
                       Request = request
                   };
        }
    }
}