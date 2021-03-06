using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCasino.DataContext;
using WebCasino.Entities;
using WebCasino.Service.Abstract;
using WebCasino.Service.Exceptions;
using WebCasino.Service.Utility.Validator;

namespace WebCasino.Service
{
    public class UserService : IUserService
    {
        private readonly CasinoContext context;
        public UserService(CasinoContext context)
        {
            this.context = context ?? throw new NullReferenceException();
        }

        public async Task<User> EditUserAlias(string alias, string id)
        {
            ServiceValidator.IsInputStringEmptyOrNull(alias);
            var user = await this.context.Users.FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);
            ServiceValidator.ObjectIsNotEqualNull(user);
            user.Alias = alias;
            await this.context.SaveChangesAsync();
            return user;

        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await this.context.Users.Where(us => !us.IsDeleted).ToListAsync();
        }

        public async Task<User> LockUser(string id)
        {
            var user = await this.context.Users.FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);
            ServiceValidator.ObjectIsNotEqualNull(user);
            var role = context.UserRoles.FirstOrDefault(ur => ur.UserId == id);
            ServiceValidator.ObjectIsNotEqualNull(role);
            if(role.RoleId == "1")
            {
                throw new InvalidAdministratorOperationException("You cannot lock other administrators");
            }
            user.Locked = true;
            await this.context.SaveChangesAsync();
            return user;
        }

        public async Task<User> PromoteUser(string id)
        {
            var user = await this.context.Users.FirstOrDefaultAsync(us => us.Id == id && !us.IsDeleted);
            var userRole = await this.context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == id);

            ServiceValidator.ObjectIsNotEqualNull(user);
            ServiceValidator.ObjectIsNotEqualNull(userRole);

            this.context.UserRoles.Remove(userRole);
            this.context.UserRoles.Add(new IdentityUserRole<string>() { UserId = id, RoleId = "1" });

            await this.context.SaveChangesAsync();
            return user;

        }

        public async Task<User> RetrieveUser(string id)
        {
            var user = await this.context.Users.FirstOrDefaultAsync(us => us.Id == id && !us.Locked);
            ServiceValidator.ObjectIsNotEqualNull(user);
            return user;
        }
    }
}
