using System.Linq;
using BusinessEntities;
using Core.Factories;
using Data.InMemory;
using Data.Repositories;
using Xunit;

namespace UnitTests.Persistence
{
    public class UserRepositoryTests
    {
        private readonly InMemoryStore _store = new InMemoryStore();

        public UserRepositoryTests()
        {
            var session = new InMemorySession(_store);
            var repository = new UserRepository(session);
            repository.Save(NewUser("John Smith", "john@companya.com", UserTypes.Employee, "A", "C", "D"));
            repository.Save(NewUser("Mark Williams", "MARK@companya.com", UserTypes.Admin, "b", "c", "e"));
            repository.Save(NewUser("Andy", "andy@companya.com", UserTypes.Employee, "D"));
            session.Commit();
        }

        [Fact]
        public void Get_NoFilter_ReturnsEveryUserOrderedByName()
        {
            Assert.Equal(new[] { "Andy", "John Smith", "Mark Williams" }, NewRepository().Get().Select(u => u.Name));
        }

        [Fact]
        public void Get_ByType()
        {
            Assert.Equal(new[] { "Andy", "John Smith" }, NewRepository().Get(UserTypes.Employee).Select(u => u.Name));
        }

        [Fact]
        public void Get_ByName_MatchesAFragmentIgnoringCase()
        {
            Assert.Equal(new[] { "John Smith" }, NewRepository().Get(name: "SMI").Select(u => u.Name));
        }

        [Fact]
        public void Get_ByEmail_MatchesExactlyIgnoringCase()
        {
            Assert.Equal(new[] { "Mark Williams" }, NewRepository().Get(email: "mark@COMPANYA.com").Select(u => u.Name));
            Assert.Empty(NewRepository().Get(email: "mark@"));
        }

        [Fact]
        public void Get_CombinesFilters()
        {
            Assert.Equal(new[] { "Andy" }, NewRepository().Get(UserTypes.Employee, "an").Select(u => u.Name));
        }

        [Fact]
        public void GetByTag_MatchesIgnoringCaseAndPages()
        {
            var repository = NewRepository();

            Assert.Equal(new[] { "John Smith", "Mark Williams" }, repository.GetByTag("C", 0, 10).Select(u => u.Name));
            Assert.Equal(new[] { "Mark Williams" }, repository.GetByTag("c", 1, 10).Select(u => u.Name));
            Assert.Equal(new[] { "Andy" }, repository.GetByTag("d", 0, 1).Select(u => u.Name));
        }

        [Fact]
        public void DeleteAll_RemovesEveryUserOnCommit()
        {
            var session = new InMemorySession(_store);
            new UserRepository(session).DeleteAll();

            Assert.Equal(3, NewRepository().Get().Count());

            session.Commit();

            Assert.Empty(NewRepository().Get());
        }

        private UserRepository NewRepository()
        {
            return new UserRepository(new InMemorySession(_store));
        }

        private static User NewUser(string name, string email, UserTypes type, params string[] tags)
        {
            var user = new IdObjectFactory<User>().Create(System.Guid.NewGuid());
            user.SetName(name);
            user.SetEmail(email);
            user.SetType(type);
            user.SetTags(tags);
            return user;
        }
    }
}