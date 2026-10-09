using System;
using BusinessEntities;
using Common;
using Xunit;

namespace UnitTests.Entities
{
    public class EntityNamesTests
    {
        [Theory]
        [InlineData(typeof(User), "user", "A user")]
        [InlineData(typeof(Product), "product", "A product")]
        [InlineData(typeof(Order), "order", "An order")]
        public void NamesEntitiesForMessages(Type entityType, string name, string withArticle)
        {
            Assert.Equal(name, EntityNames.Of(entityType));
            Assert.Equal(withArticle, EntityNames.WithArticle(entityType));
        }
    }
}