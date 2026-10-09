using System;
using BusinessEntities;
using Common;

namespace Data.Repositories
{
    [AutoRegister]
    public class Repository<T> : IRepository<T> where T : IdObject
    {
        public Repository(IDataSession session)
        {
            Session = session;
        }

        protected IDataSession Session { get; }

        public void Save(T entity)
        {
            Session.Store(entity);
        }

        public void Delete(T entity)
        {
            Session.Delete(entity);
        }

        public T Get(Guid id)
        {
            return Session.Load<T>(id);
        }
    }
}