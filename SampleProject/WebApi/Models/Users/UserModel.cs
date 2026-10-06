using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusinessEntities;
using Core.Services.Users;

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

        [Required]
        [Range(0, User.MaxAge)]
        public int? Age { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "The AnnualSalary field cannot be negative.")]
        public decimal? AnnualSalary { get; set; }

        // Required: null is ambiguous for a full replacement. Send [] to remove all tags.
        [Required]
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Maps the request to the create/update use cases. Call only on a validated model
        /// (ValidateModelAttribute runs globally), since required values are unwrapped.
        /// </summary>
        public UserDetails ToUserDetails()
        {
            return new UserDetails
                   {
                       Name = Name,
                       Email = Email,
                       Type = Type.Value,
                       Age = Age.Value,
                       AnnualSalary = AnnualSalary,
                       Tags = Tags
                   };
        }
    }
}