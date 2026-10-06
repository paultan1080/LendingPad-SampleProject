using System;
using BusinessEntities;
using Common;

namespace Core.Services.Users
{
    [AutoRegister(AutoRegisterTypes.Singleton)]
    public class UpdateUserService : IUpdateUserService
    {
        private const int MonthsPerYear = 12;

        public void Update(User user, UserDetails details)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            user.SetEmail(details.Email);
            user.SetName(details.Name);
            user.SetType(details.Type);
            user.SetAge(details.Age);
            // Lifted arithmetic: no annual salary means no monthly salary.
            user.SetMonthlySalary(details.AnnualSalary / MonthsPerYear);
            user.SetTags(details.Tags);
        }
    }
}