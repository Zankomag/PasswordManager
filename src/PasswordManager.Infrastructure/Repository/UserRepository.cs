using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Infrastructure.Repository {
	public class UserRepository : Repository<User>, IUserRepository{
		public UserRepository(DbContext context) : base(context) { }


	}
}
