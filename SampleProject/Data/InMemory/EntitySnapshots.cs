using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Data.InMemory
{
    /// <summary>
    /// Converts entities to and from their stored form. Snapshots give readers their own copies.
    /// Change detection can be a string comparison.
    /// </summary>
    internal static class EntitySnapshots
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
                                                                  {
                                                                      ContractResolver = new PersistentStateContractResolver(),
                                                                      ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                                                                      // Always rebuild collections through their setters, so entities can keep
                                                                      // exposing read-only views of their internal lists.
                                                                      ObjectCreationHandling = ObjectCreationHandling.Replace,
                                                                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                                                                      TypeNameHandling = TypeNameHandling.None
                                                                  };

        public static string Serialize(object entity)
        {
            return JsonConvert.SerializeObject(entity, Settings);
        }

        public static object Deserialize(string snapshot, Type entityType)
        {
            return JsonConvert.DeserializeObject(snapshot, entityType, Settings);
        }

        /// <summary>
        /// Persists every property that has a setter of any visibility; properties without one are
        /// computed (e.g. an order's total) and not part of the stored state.
        /// </summary>
        private class PersistentStateContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                var propertyInfo = member as PropertyInfo;
                if (propertyInfo == null)
                {
                    return property;
                }

                // A private setter declared on a base class (e.g. IdObject.Id) is invisible through the
                // derived type, so resolve the property where it is declared.
                var declared = propertyInfo.DeclaringType.GetProperty(propertyInfo.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (declared?.GetSetMethod(true) == null)
                {
                    property.Ignored = true;
                    return property;
                }

                property.Writable = true;
                property.ValueProvider = new ReflectionValueProvider(declared);
                return property;
            }
        }
    }
}