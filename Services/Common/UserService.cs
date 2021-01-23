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
        protected readonly IConfigurationProvider _mappingConfig;
        protected const string DEFAULT_PASSWORD = "Mistake1234";

        public UserService(
            ApplicationUserManager userManager,
            DanceClassDbContext dbContext,
            IMapper mapper,
            IConfigurationProvider mappingConfig) : base(dbContext, mapper)
        {
            _userManager = userManager;
            _mappingConfig = mappingConfig;
        }

        protected async Task<string> GenerateUserName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return string.Empty;
            }

            fullName = fullName.Trim().NormalizeVietnameseDiacritics();
            string[] names = Regex.Split(fullName.ToLower(), @"\s+");

            if (names.Length == 1)
            {
                string userName = names[0];

                List<string> existingUsernames = await GetUsernamesStartWith(userName);
                int numberOfExistingUsernames = existingUsernames.Count(u =>
                {
                    string[] part = u.Split('.');
                    return part[0] == userName && 
                        (part.Length == 1 || (part.Length == 2 && int.TryParse(part[1], out _)));
                });

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

                List<string> existingUsernames = await GetUsernamesStartWith(userName);
                int numberOfExistingUsernames = existingUsernames.Count(u =>
                {
                    string[] part = u.Split('.');
                    return part[0] + "." + part[1] == userName;
                });

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

        private async Task<List<string>> GetUsernamesStartWith(string start)
        {
            return await _dbContext.Users
                .Where(u => u.UserName.StartsWith(start))
                .Select(u => u.UserName)
                .ToListAsync();
        }
    }
}
