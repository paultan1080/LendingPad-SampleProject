using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extensions;

namespace BusinessEntities
{
    public class Branch : IdObject
    {
        private readonly List<User> _members = new List<User>();
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

        public void SetMembers(IEnumerable<User> members)
        {
            _members.Initialize(members.Where(q => q.Type == UserTypes.Employee));
        }
    }
}