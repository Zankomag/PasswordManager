using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Application.Services; 

public class AccountService : IAccountService {
	private readonly IUnitOfWork workUnit;

	public AccountService(IUnitOfWork workUnit) {
		this.workUnit = workUnit;
	}

	public async Task<IEnumerable<Account>> GetAccountsByNameAsync(long userId, int pageIndex, int pageSize, string accountName = null)
		=> await workUnit.AccountRepository.GetByNameAsync(userId, pageIndex, pageSize, accountName);

	public async Task<Account> GetSingleAccountByNameAsync(long userId, string accountName) 
		=> await workUnit.AccountRepository.GetSingleByNameAsync(userId, accountName);

	public async Task<int> GetAccountCountByNameAsync(long userId, string accountName = null)
		=> await workUnit.AccountRepository.GetCountAsync(userId, accountName);

	public async Task<Account> GetAccountAsync(long userId, long accountId)
		=> await workUnit.AccountRepository.GetFullAsync(userId, accountId);

	public async Task<Account> GetPasswordAsync(long userId, long accountId)
		=> await workUnit.AccountRepository.GetPasswordAsync(userId, accountId);

	public async Task<bool> DeleteAccountAsync(long userId, long accountId) {
		if(await workUnit.AccountRepository.DeleteAccountAsync(userId, accountId)) {
			try {
				await workUnit.SaveAsync();
				return true;
			} catch { }
		}
		return false;
	}

	public async Task<bool> AddAccountAsync(Account account) {
		await workUnit.AccountRepository.AddAsync(account);
		try {
			await workUnit.SaveAsync();
			return true;
		} catch(Exception exception) {
			//TODO: Log exception
			return false;
		}
	}

	public async Task UpdatePasswordAsync(long userId, long accountId, string password, bool encrypted) {
		if (password == null)
			throw new ArgumentNullException(nameof(password));

		Account account = await workUnit.AccountRepository.GetBasicAccountInfo(accountId);
		if(account.UserId == userId) {
			account.Password = password;
			account.Encrypted = encrypted;
			workUnit.AccountRepository.UpdatePassword(account);
			await workUnit.SaveAsync();
		}
	}

	public async Task UpdateAccountAsync() {
		if (workUnit.AccountRepository.HasDataChanged)
			await workUnit.SaveAsync();
	}
}