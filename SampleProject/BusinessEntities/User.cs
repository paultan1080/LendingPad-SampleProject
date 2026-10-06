using System;
using System.Collections.Generic;
using Common.Exceptions;
using Common.Extensions;

namespace BusinessEntities
{
    public class User : IdObject
    {
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
            _email = _name;
        }

        public void SetMonthlySalary(decimal? monthlySalary)
        {
            if (monthlySalary < 0)
            {
                throw new DomainValidationException(nameof(monthlySalary), "Salary cannot be negative.");
            }
            _monthlySalary = monthlySalary;
        }

        public void SetTags(IEnumerable<string> tags)
        {
            _tags.Initialize(tags);
        }
    }
}