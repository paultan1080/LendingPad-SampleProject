using System;
using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;
using Common.Extensions;

namespace BusinessEntities
{
    public class User : IdObject
    {
        public const int MaxAge = 150;

        private readonly List<string> _tags = new List<string>();
        private int _age;
        private string _email;
        private decimal? _monthlySalary;
        private string _name;
        private UserTypes _type = UserTypes.Employee;

        public string Email
        {
            get => _email;
            private set => _email = value;
        }

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        public UserTypes Type
        {
            get => _type;
            private set => _type = value;
        }

        public decimal? MonthlySalary
        {
            get => _monthlySalary;
            private set => _monthlySalary = value;
        }

        public int Age
        {
            get => _age;
            private set => _age = value;
        }

        public IEnumerable<string> Tags
        {
            get => _tags;
            private set => _tags.Initialize(value);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainValidationException(nameof(name), "Name is required.");
            }
            _name = name;
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainValidationException(nameof(email), "Email is required.");
            }
            _email = email;
        }

        public void SetType(UserTypes type)
        {
            if (!Enum.IsDefined(typeof(UserTypes), type))
            {
                throw new DomainValidationException(nameof(type), $"'{type}' is not a valid user type.");
            }
            _type = type;
        }

        public void SetAge(int age)
        {
            if (age < 0 || age > MaxAge)
            {
                throw new DomainValidationException(nameof(age), $"Age must be between 0 and {MaxAge}.");
            }
            _age = age;
        }

        public void SetMonthlySalary(decimal? monthlySalary)
        {
            if (monthlySalary < 0)
            {
                throw new DomainValidationException(nameof(monthlySalary), "Salary cannot be negative.");
            }
            _monthlySalary = monthlySalary;
        }

        /// <summary>
        /// Replaces all tags; pass an empty collection to remove them. Blank entries are dropped,
        /// values are trimmed and duplicates removed ignoring case, since tags match case-insensitively.
        /// </summary>
        public void SetTags(IEnumerable<string> tags)
        {
            if (tags == null)
            {
                throw new DomainValidationException(nameof(tags), "Tags are required; use an empty list to remove all tags.");
            }

            // Materialise before clearing, so passing the current tags (or a query over them) is safe.
            var normalized = tags.Where(tag => !string.IsNullOrWhiteSpace(tag))
                                 .Select(tag => tag.Trim())
                                 .Distinct(StringComparer.OrdinalIgnoreCase)
                                 .ToList();
            _tags.Initialize(normalized);
        }
    }
}