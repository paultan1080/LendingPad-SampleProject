using System;
using System.Collections.Generic;
using System.Linq;
using BusinessEntities;
using Common;

namespace Data.Repositories
{
    [AutoRegister]
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(IDataSession session) : base(session)
        {
        }

        // Text matches are case-insensitive (RavenDB default).
        public IEnumerable<User> Get(UserTypes? userType = null, string name = null, string email = null)
        {
            IEnumerable<User> users = Session.Query<User>();
            if (userType != null)
            {
                users = users.Where(user => user.Type == userType);
            }
            if (name != null)
            {
                users = users.Where(user => user.Name != null && user.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (email != null)
            {
                users = users.Where(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
            }
            return InStableOrder(users).ToList();
        }

        public IEnumerable<User> GetByTag(string tag, int skip, int take)
        {
            var users = Session.Query<User>().Where(user => user.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            return InStableOrder(users).Skip(skip).Take(take).ToList();
        }

        public void DeleteAll()
        {
            foreach (var user in Session.Query<User>())
            {
                Delete(user);
            }
        }

        private static IEnumerable<User> InStableOrder(IEnumerable<User> users)
        {
            return users.OrderBy(user => user.Name, StringComparer.OrdinalIgnoreCase).ThenBy(user => user.Id);
        }
    }
}