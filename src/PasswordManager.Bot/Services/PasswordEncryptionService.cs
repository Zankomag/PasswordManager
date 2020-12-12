using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;
using System.Collections.Generic;

namespace PasswordManager.Bot.Services {
	public class PasswordEncryptionService : IPasswordEncryptionService {
		private readonly Dictionary<int, Account> accounts;

		public PasswordEncryptionService() {
			accounts = new Dictionary<int, Account>();
		}

		public void StartEncryptionRequest(int userId, Account account)
			=> accounts[userId] = account;

		public void FinishEncryptionRequest(int userId)
			=> accounts.Remove(userId);

		public Account GetAccount(int userId) {
			if (accounts.TryGetValue(userId, out Account account))
				return account;
			return null;
		}
	}
}
