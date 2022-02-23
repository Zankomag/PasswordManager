using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Infrastructure.Repositories {
	public class AccountRepository : Repository<Account, long>, IAccountRepository {
		public AccountRepository(DbContext context) : base(context) { }

		private IQueryable<Account> GetByUser(long userId) => dbSet.Where(x => x.UserId == userId);

		public async Task<int> GetCountAsync(long userId, string accountName = null) {
			var query = GetByUser(userId);
			if (accountName != null)
				query = query.Where(x => x.AccountName.Contains(accountName));
			return await query.AsNoTracking().CountAsync();
		}

		public async Task<IEnumerable<Account>> GetByNameAsync(long userId, int page, int pageSize, string accountName = null) {
			var query = GetByUser(userId);
			if (accountName != null)
				query = query.Where(x => x.AccountName.Contains(accountName));
			return await query
				.AsNoTracking()
				.Select(a => new Account() {
					Id = a.Id,
					AccountName = a.AccountName,
					Link = a.Link,
					Login = a.Login,
				})
				.Skip(page * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}
		public async Task<Account> GetFullAsync(long userId, long accountId) {
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

		public async Task<Account> GetPasswordAsync(long userId, long accountId) 
			=> await GetByUser(userId)
				.Where(x => x.Id == accountId)
				.Select(a => new Account() {
					Id = accountId,
					Encrypted = a.Encrypted,
					Password = a.Password
				})
				.AsNoTracking()
				.FirstOrDefaultAsync();
		

		public async Task<bool> DeleteAccountAsync(long userId, long accountId) {
			var account = await dbSet
				.Where(x => x.Id == accountId && x.UserId == userId)
				.FirstOrDefaultAsync();
			return base.Delete(account);
		}

		public void UpdatePassword(Account account) {
			if (account == null)
				throw new ArgumentNullException(nameof(account));
			if (account.Password == null)
				throw new ArgumentException("user.PasswordGeneratorPattern cannot be null", nameof(account));
			context.Entry(account).Property(x => x.Password).IsModified = true;
			context.Entry(account).Property(x => x.Encrypted).IsModified = true;
		}

		public async Task<Account> GetBasicAccountInfo(long accountId)
			=> await dbSet.Where(x => x.Id == accountId)
				.Select(a => new Account() {
					Id = accountId,
					UserId = a.UserId
				})
				.AsNoTracking()
				.FirstOrDefaultAsync();
		
	}
}
