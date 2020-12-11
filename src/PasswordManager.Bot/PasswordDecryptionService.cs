using System;
using System.Collections.Generic;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot {
	public class PasswordDecryptionService : IPasswordDecryptionService {
		private readonly Dictionary<int, string> encryptedPasswords;

		public PasswordDecryptionService() {
			encryptedPasswords = new Dictionary<int, string>();
		}

		public void StartDecryptionRequest(int userId, string encryptedPassword)
			=> encryptedPasswords[userId] = encryptedPassword;

		public void FinishDecryptionRequest(int userId)
			=> encryptedPasswords.Remove(userId);

		public string GetEncryptedPassword(int userId) {
			if(encryptedPasswords.TryGetValue(userId, out string encryptedPassword))
				return encryptedPassword;
			return null;
		}

	}
}
