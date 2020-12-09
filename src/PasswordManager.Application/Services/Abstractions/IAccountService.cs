using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions {
	public interface IAccountService {
		/// <param name="accountName">Account Name to search. If null - count of all accounts will be returned.</param>
		/// <returns>Count of all accouns found by <paramref name="accountName"/></returns>
		Task<int> GetCountAsync(int userId, string accountName = null);
		/// <returns>Full Account info without password</returns>
		Task<Account> GetFullAsync(int userId, int accountId);
		/// <returns>List of basic Account info found by <paramref name="accountName"/></returns>
		Task<IEnumerable<Account>> GetByNameAsync(int userId, int page, int pageSize, string accountName = null);
		/// <returns>Password and its encryption state info</returns>
		Task<Account> GetPasswordAsync(int userId, int accountId);
		/// <returns>True on success</returns>
		Task<bool> DeleteAccountAsync(int userId, int accountId);
		Task<bool> AddAccountAsync(int userId, Account account);
	}
}
