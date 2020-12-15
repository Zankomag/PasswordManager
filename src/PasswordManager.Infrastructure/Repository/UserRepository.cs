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

		public async Task<User> GetUserActionAsync(int userId)
			=> await GetUser(userId)
				.Select(x => new User { 
					Id = userId,
					Lang = x.Lang,
					Action = x.Action})
				.AsNoTracking()
				.FirstOrDefaultAsync();

		public async Task<IList<User>> GetAllBasicInfoAsync()
			//=> await base.Get()
			//	.Select(x => new User { Id = x.Id, Accounts = new List<Account>(x.Accounts.Count) })
			//	.ToListAsync();
			//
			//TODO: 
			//If this call doesn't work or is not optimazed 
			//then try call above
			//If it is too not optimized
			//then use Tuple
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
			if (user.Lang == null)
				throw new ArgumentException("user.Lang cannot be null", nameof(user));
			context.Entry(user).Property(x => x.Lang).IsModified = true;
		}
		public void UpdatePasswordPattern(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			if (user.GenPattern == null)
				throw new ArgumentException("user.GenPattern cannot be null", nameof(user));
			context.Entry(user).Property(x => x.GenPattern).IsModified = true;
		}

		public async Task<string> GetKeyHint(int userId)
			=> await GetUser(userId)
				.Select(x => x.KeyHint)
				.FirstOrDefaultAsync();

		public void UpdateKeyHint(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			//KeyHint can be null so we dont check it for null equality
			context.Entry(user).Property(x => x.KeyHint).IsModified = true;
		}
	}
}
