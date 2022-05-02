using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories; 

public interface IAccountRepository : IRepository<Account, long> {

	/// <param name="userId"></param>
	/// <param name="accountName">Account Name to search. If null - count of all accounts will be returned.</param>
	/// <returns>Count of all accounts found by <paramref name="accountName" /></returns>
	Task<int> GetCountAsync(long userId, string accountName = null);

	/// <returns>Full Account info without password</returns>
	Task<Account> GetFullAsync(long userId, long accountId);

	/// <returns>List of basic Account info found by <paramref name="accountName" /></returns>
	Task<IEnumerable<Account>> GetByNameAsync(long userId, int page, int pageSize, string accountName = null);

	/// <returns>Basic single Account info found by <paramref name="accountName" /></returns>
	Task<Account> GetSingleByNameAsync(long userId, string accountName);

	
	/// <returns>Password and its encryption state info</returns>
	Task<Account> GetPasswordAsync(long userId, long accountId);

	/// <returns>True on success</returns>
	Task<bool> DeleteAccountAsync(long userId, long accountId);

	/// <summary>
	///     Updates Password and Encrypted fields
	/// </summary>
	/// <param name="account"></param>
	void UpdatePassword(Account account);

	/// <summary></summary>
	/// <returns>Account Id and User Id</returns>
	Task<Account> GetBasicAccountInfo(long accountId);

}