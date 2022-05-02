using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions; 

public interface IAccountService {

	/// <param name="userId"></param>
	/// <param name="accountName">Account Name to search. If null - count of all accounts will be returned.</param>
	/// <returns>Count of all accounts found by <paramref name="accountName"/></returns>
	Task<int> GetAccountsCountByNameAsync(long userId, string accountName = null);
	/// <returns>Full Account info without password</returns>
	Task<Account> GetAccountAsync(long userId, long accountId);
	/// <returns>List of basic Account info found by <paramref name="accountName"/>.
	/// Returns all accounts if <paramref name="accountName"/> is null</returns>
	Task<IEnumerable<Account>> GetAccountsByNameAsync(long userId, int page, int pageSize, string accountName = null);
	/// <returns>Single Account basic info found by <paramref name="accountName"/></returns>
	Task<Account> GetSingleAccountByNameAsync(long userId, string accountName);
	/// <returns>Password and its encryption state info</returns>
	Task<Account> GetPasswordAsync(long userId, long accountId);
	/// <returns>True on success</returns>
	Task<bool> DeleteAccountAsync(long userId, long accountId);
	Task<bool> AddAccountAsync(Account account);
	Task UpdatePasswordAsync(long userId, long accountId, string password, bool encrypted);
	Task UpdateAccountAsync();
}