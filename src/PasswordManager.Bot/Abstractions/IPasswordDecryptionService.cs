using System;

namespace PasswordManager.Bot.Abstractions {
	public interface IPasswordDecryptionService {
		/// <summary>
		/// Saves encryptedPassword of user to return it for decryption later
		void SaveDecryptionRequest(int userId, string encryptedPassword);
		/// <returns>Encrypted password of user</returns>
		/// <exception cref="InvalidOperationException"></exception>
		string GetEncryptedPassword(int userId);
	}
}
