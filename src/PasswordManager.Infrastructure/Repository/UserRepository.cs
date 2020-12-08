using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PasswordManager.Infrastructure.Repository {
	public class UserRepository : Repository<User>, IUserRepository {
		public UserRepository(DbContext context) : base(context) { }

		private IQueryable<User> GetUser(int id) => dbSet.Where(x => x.Id == id);

		public async Task<User> GetUserWithLangAsync(int userId) {
			return await GetUser(userId)
				.Select(x => new User { Id = userId, Lang = x.Lang})
				.AsNoTracking()
				.FirstOrDefaultAsync();
		}

		public async Task<IList<User>> GetAllBasicInfoAsync()
			//=> await base.Get()
			//	.Select(x => new User { Id = x.Id, Accounts = new List<Account>(x.Accounts.Count) })
			//	.Include(u => u.Accounts.Count)
			//	.ToListAsync();
			//
			//TODO: 
			//If this call doesn't work or is not optimazed 
			//then try call above
			//If it is too not optimized
			//then use special UserInfo model with AccountField and cast x.Accounts.Count to it
			=> await base.Get()
				.Select(x => new User { Id = x.Id })
				.Include(u => u.Accounts.Count)
				.ToListAsync();

		public void UpdateAction(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			context.Entry(user).Property(x => x.Action).IsModified = true;
		}
		public void UpdateLanguage(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if(user.Lang == null)
				throw new ArgumentNullException(nameof(user.Lang));
			context.Entry(user).Property(x => x.Lang).IsModified = true;
		}
		public void UpdatePasswordPattern(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if (user.Lang == null)
				throw new ArgumentNullException(nameof(user.Lang));
			context.Entry(user).Property(x => x.GenPattern).IsModified = true;
		}
	}
}
