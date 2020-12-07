using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace PasswordManager.Infrastructure.Repository {
	public class UserRepository : Repository<User>, IUserRepository {
		public UserRepository(DbContext context) : base(context) { }

		private IQueryable<User> GetUser(int id) => dbSet.Where(x => x.Id == id);

		public async Task<User> GetLangAsync(int userId) {
			return await GetUser(userId)
				.Select(x => new User { Id = userId, Lang = x.Lang})
				.FirstOrDefaultAsync();
		}

		public void UpdateAction(int userId, UserAction action) {
			User user = new User() { Id = userId, Action = action };
			context.Entry(user).Property(x => x.Action).IsModified = true;
		}
		public void UpdateLanguage(int userId, string langCode) {
			User user = new User() { Id = userId, Lang = langCode };
			context.Entry(user).Property(x => x.Lang).IsModified = true;
		}
		public void UpdatePasswordPattern(int userId, string passwordPattern) {
			User user = new User() { Id = userId, GenPattern = passwordPattern };
			context.Entry(user).Property(x => x.GenPattern).IsModified = true;
		}
	}
}
