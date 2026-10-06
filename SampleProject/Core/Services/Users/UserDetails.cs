using System.Collections.Generic;
using BusinessEntities;

namespace Core.Services.Users
{
    /// <summary>
    /// The editable details of a user, as supplied to the create and update use cases.
    /// </summary>
    public class UserDetails
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public UserTypes Type { get; set; }
        public int Age { get; set; }
        public decimal? AnnualSalary { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}