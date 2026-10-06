using Common;
using Raven.Client;

namespace Data
{
    [AutoRegister]
    public class RavenUnitOfWork : IUnitOfWork
    {
        private readonly IDocumentSession _documentSession;

        public RavenUnitOfWork(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public void Commit()
        {
            _documentSession.SaveChanges();
        }
    }
}