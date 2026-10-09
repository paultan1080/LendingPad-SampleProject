using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BusinessEntities;
using Xunit;

namespace UnitTests.Persistence
{
    /// <summary>
    /// The in-memory store persists every property that has a setter (of any visibility). A field no such
    /// property exposes would be silently lost on the first round trip, so persisted types must not have one.
    /// </summary>
    public class PersistenceConventionTests
    {
        [Theory]
        [InlineData(typeof(User))]
        [InlineData(typeof(Product))]
        [InlineData(typeof(Order))]
        [InlineData(typeof(OrderLine))]
        public void EveryFieldIsExposedByAPropertyWithASetter(Type persistedType)
        {
            var unexposed = new List<string>();
            for (var type = persistedType; type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var propertyName = field.Name.TrimStart('_');
                    var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.IgnoreCase);
                    if (property?.GetSetMethod(true) == null)
                    {
                        unexposed.Add($"{type.Name}.{field.Name}");
                    }
                }
            }

            Assert.Empty(unexposed);
        }
    }
}