using System;
using System.Linq;
using BusinessEntities;
using Common.Exceptions;
using Core.Factories;
using Data.InMemory;
using Xunit;

namespace UnitTests.Persistence
{
    public class InMemorySessionTests
    {
        private readonly InMemoryStore _store = new InMemoryStore();

        [Fact]
        public void Commit_ThenLoadInAnotherSession_RoundTripsTheWholeState()
        {
            var john = SeedJohn();

            var loaded = NewSession().Load<User>(john.Id);

            Assert.NotSame(john, loaded);
            Assert.Equal(john.Id, loaded.Id);
            Assert.Equal("John Smith", loaded.Name);
            Assert.Equal("john@companya.com", loaded.Email);
            Assert.Equal(UserTypes.Employee, loaded.Type);
            Assert.Equal(27, loaded.Age);
            Assert.Equal(7166.6666666666666666666666667m, loaded.MonthlySalary);
            Assert.Equal(new[] { "A", "C" }, loaded.Tags);
        }

        [Fact]
        public void ProductsAndOrders_RoundTripTheirWholeState()
        {
            var product = new IdObjectFactory<Product>().Create(Guid.NewGuid());
            product.SetName("Laptop");
            product.SetDescription("15-inch");
            product.SetPrice(1399.99m);
            product.SetCategory(ProductCategories.Hardware);
            var order = new IdObjectFactory<Order>().Create(Guid.NewGuid());
            var placedOn = new DateTime(2026, 10, 8, 9, 30, 0, DateTimeKind.Utc);
            order.Place(Guid.NewGuid(), new[] { new OrderLine(product, 2) }, placedOn);
            order.ChangeStatus(OrderStatuses.Paid);
            var session = NewSession();
            session.Store(product);
            session.Store(order);
            session.Commit();

            var loadedProduct = NewSession().Load<Product>(product.Id);
            var loadedOrder = NewSession().Load<Order>(order.Id);

            Assert.Equal(new object[] { "Laptop", "15-inch", 1399.99m, ProductCategories.Hardware },
                         new object[] { loadedProduct.Name, loadedProduct.Description, loadedProduct.Price, loadedProduct.Category });
            Assert.Equal(order.CustomerId, loadedOrder.CustomerId);
            Assert.Equal(placedOn, loadedOrder.PlacedOn);
            Assert.Equal(DateTimeKind.Utc, loadedOrder.PlacedOn.Kind);
            Assert.Equal(OrderStatuses.Paid, loadedOrder.Status);
            var line = Assert.Single(loadedOrder.Lines);
            Assert.Equal(new object[] { product.Id, "Laptop", 1399.99m, 2 }, new object[] { line.ProductId, line.ProductName, line.UnitPrice, line.Quantity });
            Assert.Equal(2799.98m, loadedOrder.Total);
        }

        [Fact]
        public void Load_SameIdTwice_ReturnsTheSameInstance()
        {
            var id = SeedJohn().Id;
            var session = NewSession();

            Assert.Same(session.Load<User>(id), session.Load<User>(id));
        }

        [Fact]
        public void Load_UnknownId_ReturnsNull()
        {
            Assert.Null(NewSession().Load<User>(Guid.NewGuid()));
        }

        [Fact]
        public void ChangesToALoadedEntity_AreInvisibleUntilCommitted()
        {
            var id = SeedJohn().Id;
            var writer = NewSession();
            writer.Load<User>(id).SetName("John Changed");

            Assert.Equal("John Smith", NewSession().Load<User>(id).Name);

            writer.Commit();

            Assert.Equal("John Changed", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void SessionThatNeverCommits_LeavesTheStoreUntouched()
        {
            var id = SeedJohn().Id;
            var failedRequest = NewSession();
            var user = failedRequest.Load<User>(id);
            user.SetName("Half");
            user.SetType(UserTypes.Customer);

            Assert.Equal("John Smith", NewSession().Load<User>(id).Name);
            Assert.Equal(UserTypes.Employee, NewSession().Load<User>(id).Type);
        }

        [Fact]
        public void Store_IdAlreadyInStore_CommitThrowsConflict()
        {
            var id = SeedJohn().Id;
            var session = NewSession();
            session.Store(NewUser(id, "Mary"));

            var exception = Assert.Throws<ConflictException>(() => session.Commit());

            Assert.Contains("already exists", exception.Message);
            Assert.Equal("John Smith", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void ConcurrentUpdates_TheSecondCommitThrowsConflict()
        {
            var id = SeedJohn().Id;
            var first = NewSession();
            var second = NewSession();
            first.Load<User>(id).SetName("First");
            second.Load<User>(id).SetName("Second");
            first.Commit();

            Assert.Throws<ConflictException>(() => second.Commit());

            Assert.Equal("First", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void ReadOnlySession_DoesNotConflictWithAConcurrentWriter()
        {
            var id = SeedJohn().Id;
            var reader = NewSession();
            reader.Load<User>(id);
            var writer = NewSession();
            writer.Load<User>(id).SetName("Changed");
            writer.Commit();

            reader.Commit();

            Assert.Equal("Changed", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void StaleUpdate_AfterTheEntityWasDeletedAndCreatedAgain_ThrowsConflict()
        {
            var id = SeedJohn().Id;
            var stale = NewSession();
            stale.Load<User>(id).SetName("Stale");
            DeleteAndCreateAgain(id, "Created again");

            Assert.Throws<ConflictException>(() => stale.Commit());

            Assert.Equal("Created again", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void StaleDelete_AfterTheEntityWasDeletedAndCreatedAgain_ThrowsConflict()
        {
            var id = SeedJohn().Id;
            var stale = NewSession();
            stale.Delete(stale.Load<User>(id));
            DeleteAndCreateAgain(id, "Created again");

            Assert.Throws<ConflictException>(() => stale.Commit());

            Assert.Equal("Created again", NewSession().Load<User>(id).Name);
        }

        [Fact]
        public void Delete_RemovesTheEntityOnCommit()
        {
            var id = SeedJohn().Id;
            var session = NewSession();
            session.Delete(session.Load<User>(id));

            Assert.Null(session.Load<User>(id));
            Assert.NotNull(NewSession().Load<User>(id));

            session.Commit();

            Assert.Null(NewSession().Load<User>(id));
        }

        [Fact]
        public void Delete_EntityChangedByAnotherRequest_ThrowsConflict()
        {
            var id = SeedJohn().Id;
            var deleter = NewSession();
            deleter.Delete(deleter.Load<User>(id));
            var writer = NewSession();
            writer.Load<User>(id).SetName("Changed");
            writer.Commit();

            Assert.Throws<ConflictException>(() => deleter.Commit());
        }

        [Fact]
        public void Delete_EntityNotTrackedBySession_Throws()
        {
            var id = SeedJohn().Id;
            var copyFromElsewhere = NewSession().Load<User>(id);

            Assert.Throws<InvalidOperationException>(() => NewSession().Delete(copyFromElsewhere));
        }

        [Fact]
        public void FailedCommit_AppliesNoneOfItsChanges()
        {
            var id = SeedJohn().Id;
            var session = NewSession();
            var newUser = NewUser(Guid.NewGuid(), "Mary");
            session.Store(newUser);
            session.Load<User>(id).SetName("Stale");
            var writer = NewSession();
            writer.Load<User>(id).SetName("Changed");
            writer.Commit();

            Assert.Throws<ConflictException>(() => session.Commit());

            Assert.Null(NewSession().Load<User>(newUser.Id));
        }

        [Fact]
        public void Store_AnotherInstanceWithATrackedId_Throws()
        {
            var id = SeedJohn().Id;
            var session = NewSession();
            session.Load<User>(id);

            Assert.Throws<InvalidOperationException>(() => session.Store(NewUser(id, "Impostor")));
        }

        [Fact]
        public void EntitiesOfDifferentTypes_WithTheSameId_AreStoredSeparately()
        {
            var john = SeedJohn();
            var gadget = new IdObjectFactory<Gadget>().Create(john.Id);
            gadget.Rename("Head office");
            var session = NewSession();
            session.Store(gadget);

            session.Commit();

            Assert.Equal("Head office", NewSession().Load<Gadget>(john.Id).Name);
            Assert.Equal("John Smith", NewSession().Load<User>(john.Id).Name);
        }

        [Fact]
        public void Query_ReturnsCommittedEntitiesOfTheRequestedTypeOnly()
        {
            var john = SeedJohn();
            var gadgets = NewSession();
            var gadget = new IdObjectFactory<Gadget>().Create(Guid.NewGuid());
            gadget.Rename("Head office");
            gadgets.Store(gadget);
            gadgets.Commit();
            NewSession().Store(NewUser(Guid.NewGuid(), "Never committed"));

            var users = NewSession().Query<User>().ToList();

            Assert.Equal(john.Id, Assert.Single(users).Id);
        }

        [Fact]
        public void Query_ExcludesEntitiesDeletedInTheSession()
        {
            var id = SeedJohn().Id;
            var session = NewSession();
            session.Delete(session.Load<User>(id));

            Assert.Empty(session.Query<User>());
        }

        private InMemorySession NewSession()
        {
            return new InMemorySession(_store);
        }

        private User SeedJohn()
        {
            var john = NewUser(Guid.NewGuid(), "John Smith");
            john.SetAge(27);
            john.SetMonthlySalary(86000m / 12);
            john.SetTags(new[] { "A", "C" });
            var session = NewSession();
            session.Store(john);
            session.Commit();
            return john;
        }

        private void DeleteAndCreateAgain(Guid id, string name)
        {
            var deleter = NewSession();
            deleter.Delete(deleter.Load<User>(id));
            deleter.Commit();
            var creator = NewSession();
            creator.Store(NewUser(id, name));
            creator.Commit();
        }

        private static User NewUser(Guid id, string name)
        {
            var user = new IdObjectFactory<User>().Create(id);
            user.SetName(name);
            user.SetEmail("john@companya.com");
            user.SetType(UserTypes.Employee);
            return user;
        }
    }

    /// <summary>An entity type that exists only in tests, so storage tests do not depend on domain types.</summary>
    public class Gadget : IdObject
    {
        public string Name { get; private set; }

        public void Rename(string name)
        {
            Name = name;
        }
    }
}