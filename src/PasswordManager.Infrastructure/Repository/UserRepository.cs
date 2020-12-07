using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Infrastructure.Repository {
	public class UserRepository : Repository<User>, IUserRepository{
		public UserRepository(DbContext context) : base(context) { }

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
