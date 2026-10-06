using Common.Exceptions;

namespace BusinessEntities
{
    public class IdNameObject : IdObject
    {
        private string _name;

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException(nameof(name), "Name is required.");
            }
            _name = name;
        }
    }
}