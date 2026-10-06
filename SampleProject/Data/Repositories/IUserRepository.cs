using System.Collections.Generic;
using BusinessEntities;

namespace Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        IEnumerable<User> Get(UserTypes? userType = null, string name = null, string email = null);
        IEnumerable<User> GetByTag(string tag, int skip, int take);
        void DeleteAll();
    }
}