using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure.Repository {
	public class UserRepository : Repository<User>, IUserRepository {
		public UserRepository(DbContext context) : base(context) { }

		private IQueryable<User> GetUser(int id) => dbSet.Where(x => x.Id == id);

		public async Task<User> GetLangAsync(int userId) {
			return await GetUser(userId)
				.Select(x => new User { Id = userId, Lang = x.Lang})
				.AsNoTracking()
				.FirstOrDefaultAsync();
		}

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
