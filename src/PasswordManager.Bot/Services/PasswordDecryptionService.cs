using System.Collections.Generic;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Services {
	public class PasswordDecryptionService : IPasswordDecryptionService {
		private readonly Dictionary<int, Account> encryptedPasswords;

		public PasswordDecryptionService() {
			encryptedPasswords = new Dictionary<int, Account>();
		}

		public void StartDecryptionRequest(int userId, Account account)
			=> encryptedPasswords[userId] = account;

		public void FinishDecryptionRequest(int userId)
			=> encryptedPasswords.Remove(userId);

		public Account GetAccount(int userId) {
			if(encryptedPasswords.TryGetValue(userId, out Account account))
				return account;
			return null;
		}

	}
}
