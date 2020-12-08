﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Application.Services {
	class AccountService : IAccountService {
		private readonly IUnitOfWork workUnit;

		public AccountService(IUnitOfWork workUnit) {
			this.workUnit = workUnit;
		}

		public async Task<IEnumerable<Account>> GetByNameAsync(int userId, int page, int pageSize, string accountName = null)
			=> await workUnit.AccountRepository.GetByNameAsync(userId, page, pageSize, accountName);

		public async Task<int> GetCountAsync(int userId, string accountName = null)
			=> await workUnit.AccountRepository.GetCountAsync(userId, accountName);

		public async Task<Account> GetFullAsync(int userId, int accountId)
			=> await workUnit.AccountRepository.GetFullAsync(userId, accountId);

		public async Task<Account> GetPasswordAsync(int userId, int accountId)
			=> await workUnit.AccountRepository.GetPasswordAsync(userId, accountId);
	}
}
