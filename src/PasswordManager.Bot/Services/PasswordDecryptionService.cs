using System.Collections.Generic;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Services {
	public class PasswordDecryptionService : IPasswordDecryptionService {
		private readonly Dictionary<long, Account> accounts;

		public PasswordDecryptionService() {
			accounts = new Dictionary<long, Account>();
		}

		public void StartDecryptionRequest(long userId, Account account)
			=> accounts[userId] = account;

		public void FinishDecryptionRequest(long userId)
			=> accounts.Remove(userId);

		public Account GetAccount(long userId) {
			if(accounts.TryGetValue(userId, out Account account))
				return account;
			return null;
		}

	}
}
