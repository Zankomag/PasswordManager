using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {

	public interface IAccountRepository : IRepository<Account> {

		/// <param name="userId"></param>
		/// <param name="accountName">Account Name to search. If null - count of all accounts will be returned.</param>
		/// <returns>Count of all accounts found by <paramref name="accountName" /></returns>
		Task<int> GetCountAsync(int userId, string accountName = null);

		/// <returns>Full Account info without password</returns>
		Task<Account> GetFullAsync(int userId, long accountId);

		/// <returns>List of basic Account info found by <paramref name="accountName" /></returns>
		Task<IEnumerable<Account>> GetByNameAsync(int userId, int page, int pageSize, string accountName = null);

		/// <returns>Password and its encryption state info</returns>
		Task<Account> GetPasswordAsync(int userId, long accountId);

		/// <returns>True on success</returns>
		Task<bool> DeleteAccountAsync(int userId, long accountId);

		/// <summary>
		///     Updates Password and Encrypted fields
		/// </summary>
		/// <param name="account"></param>
		void UpdatePassword(Account account);

		/// <summary></summary>
		/// <returns>Account Id and User Id</returns>
		Task<Account> GetBasicAccountInfo(long accountId);

	}

}