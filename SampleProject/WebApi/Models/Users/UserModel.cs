using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessEntities;

namespace WebApi.Models.Users
{
    public class UserModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        // Nullable so a missing value is reported by [Required] instead of silently becoming 0,
        // which is not a valid UserTypes member.
        [Required]
        [EnumDataType(typeof(UserTypes))]
        public UserTypes? Type { get; set; }

        public decimal? AnnualSalary { get; set; }
        public IEnumerable<string> Tags { get; set; }
    }
}