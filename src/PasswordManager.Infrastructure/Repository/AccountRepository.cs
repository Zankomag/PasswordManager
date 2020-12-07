using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Infrastructure.Repository {
	public class AccountRepository : Repository<Account>, IAccountRepository {
		public AccountRepository(DbContext context) : base(context) { }

		private IQueryable<Account> GetByUser(int userId) => dbSet.Where(x => x.UserId == userId);

		public async Task<int> GetCountAsync(int userId, string accountName = null) {
			var query = GetByUser(userId);
			if (accountName != null)
				query = query.Where(x => x.AccountName.Contains(accountName));
			return await query.CountAsync();
		}

		public async Task<IEnumerable<Account>> GetByNameAsync(int userId, string accountName = null) {
			var query = GetByUser(userId);
			if (accountName != null)
				query = query.Where(x => x.AccountName.Contains(accountName))
					;
			return await query
				.Select(a => new Account() {
					Id = a.Id,
					AccountName = a.AccountName,
					Link = a.Link,
					Login = a.Login,
				})
				.ToListAsync();
		}
		public async Task<Account> GetFullAsync(int userId, int accountId) {
			return await GetByUser(userId)
				.Where(x => x.Id == accountId)
				.Select(a => new Account() {
					Id = accountId,
					AccountName = a.AccountName,
					Link = a.Link,
					Login = a.Login,
					Note = a.Note,
					Encrypted = a.Encrypted,
					OutdatedTime = a.OutdatedTime,
					PasswordUpdatedDate = a.PasswordUpdatedDate,
					UserId = userId
				})
				.FirstOrDefaultAsync();
		}

		public async Task<Account> GetPasswordAsync(int userId, int accountId) {
			return await GetByUser(userId)
				.Where(x => x.Id == accountId)
				.Select(a => new Account() {
					Id = accountId,
					Encrypted = a.Encrypted,
					Password = a.Password,
					UserId = userId
				})
				.FirstOrDefaultAsync();
		}
	}
}
