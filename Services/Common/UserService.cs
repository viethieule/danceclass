using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess;
using DataAccess.IdentityAccessor;
using Services.Utils;

namespace Services.Common
{
    public class UserService : BaseService
    {
        protected readonly ApplicationUserManager _userManager;
        protected const string DEFAULT_PASSWORD = "Mistake1234";

        public UserService(ApplicationUserManager userManager, DanceClassDbContext dbContext, IMapper mapper) : base(dbContext, mapper)
        {
            _userManager = userManager;
        }

        protected async Task<string> GenerateUserName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            fullName = StringHelper.ConverToUnsignedString(fullName.Trim());
            string[] names = Regex.Split(fullName.ToLower(), @"\s+");

            if (names.Length == 1)
            {
                string userName = names[0];

                int numberOfExistingUsernames = await _dbContext.Users.CountAsync(u => u.UserName.StartsWith(userName) && u.UserName.Length == userName.Length);
                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
            else
            {
                string userName = names[names.Length - 1] + "." + names[0];

                int numberOfExistingUsernames = await _dbContext.Users.CountAsync(u => u.UserName.StartsWith(userName));
                if (numberOfExistingUsernames == 0)
                {
                    return userName;
                }
                else
                {
                    return userName + "." + numberOfExistingUsernames;
                }
            }
        }
    }
}
