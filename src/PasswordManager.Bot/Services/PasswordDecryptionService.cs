using System.Collections.Generic;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Services {
	public class PasswordDecryptionService : IPasswordDecryptionService {
		private readonly Dictionary<int, Account> accounts;

		public PasswordDecryptionService() {
			accounts = new Dictionary<int, Account>();
		}

		public void StartDecryptionRequest(int userId, Account account)
			=> accounts[userId] = account;

		public void FinishDecryptionRequest(int userId)
			=> accounts.Remove(userId);

		public Account GetAccount(int userId) {
			if(accounts.TryGetValue(userId, out Account account))
				return account;
			return null;
		}

	}
}
