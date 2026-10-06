using System.Collections.Generic;
using System.Linq;
using BusinessEntities;

namespace WebApi.Models.Users
{
    public class UserData : IdObjectData
    {
        public UserData(User user) : base(user)
        {
            Email = user.Email;
            Name = user.Name;
            Type = new EnumData(user.Type);
            MonthlySalary = user.MonthlySalary;
            Age = user.Age;
            Tags = user.Tags.ToList();
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public EnumData Type { get; set; }
        public decimal? MonthlySalary { get; set; }
        public int Age { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}