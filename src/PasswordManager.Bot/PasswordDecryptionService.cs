using System;
using System.Collections.Generic;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot {
	public class PasswordDecryptionService : IPasswordDecryptionService {
		private readonly Dictionary<int, string> encryptedPasswords;

		public PasswordDecryptionService() {
			encryptedPasswords = new Dictionary<int, string>();
		}

		public void SaveDecryptionRequest(int userId, string encryptedPassword)
			=> encryptedPasswords[userId] = encryptedPassword;

		public string GetEncryptedPassword(int userId) {
			if(encryptedPasswords.TryGetValue(userId, out string encryptedPassword)){
				return encryptedPassword;
			}
			throw new InvalidOperationException("User does not have password waiting for decryption");
		}
	}
}
