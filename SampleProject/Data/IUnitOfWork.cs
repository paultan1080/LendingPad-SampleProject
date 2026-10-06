namespace Data
{
    /// <summary>
    /// Persists every change tracked during the current request as a single unit.
    /// </summary>
    public interface IUnitOfWork
    {
        void Commit();
    }
}