﻿using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions {
	public interface IAccountService {

		/// <param name="userId"></param>
		/// <param name="accountName">Account Name to search. If null - count of all accounts will be returned.</param>
		/// <returns>Count of all accouns found by <paramref name="accountName"/></returns>
		Task<int> GetCountAsync(long userId, string accountName = null);
		/// <returns>Full Account info without password</returns>
		Task<Account> GetFullAsync(long userId, long accountId);
		/// <returns>List of basic Account info found by <paramref name="accountName"/></returns>
		Task<IEnumerable<Account>> GetByNameAsync(long userId, int page, int pageSize, string accountName = null);
		/// <returns>Password and its encryption state info</returns>
		Task<Account> GetPasswordAsync(long userId, long accountId);
		/// <returns>True on success</returns>
		Task<bool> DeleteAccountAsync(long userId, long accountId);
		Task<bool> AddAccountAsync(long userId, Account account);
		Task UpdatePasswordAsync(long userId, long accountId, string password, bool encrypted);
		Task UpdateAccountAsync();
	}
}
