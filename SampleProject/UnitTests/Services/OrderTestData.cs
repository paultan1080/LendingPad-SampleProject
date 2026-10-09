using System;
using BusinessEntities;
using Core.Factories;

namespace UnitTests.Services
{
    internal static class OrderTestData
    {
        public static readonly DateTime Now = new DateTime(2026, 10, 8, 9, 30, 0, DateTimeKind.Utc);

        public static Product Product(string name, decimal price)
        {
            var product = new IdObjectFactory<Product>().Create(Guid.NewGuid());
            product.SetName(name);
            product.SetPrice(price);
            return product;
        }

        public static User User(UserTypes type)
        {
            var user = new IdObjectFactory<User>().Create(Guid.NewGuid());
            user.SetName("Clara Customer");
            user.SetEmail("clara@customer.com");
            user.SetType(type);
            return user;
        }

        public static Order PlacedOrder(params OrderLine[] lines)
        {
            var order = new IdObjectFactory<Order>().Create(Guid.NewGuid());
            order.Place(Guid.NewGuid(), lines, Now);
            return order;
        }
    }
}