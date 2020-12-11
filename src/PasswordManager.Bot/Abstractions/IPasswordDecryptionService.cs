using System;

namespace PasswordManager.Bot.Abstractions {
	public interface IPasswordDecryptionService {
		/// <summary>
		/// Saves encryptedPassword of user to return it for decryption later
		/// </summary>
		void StartDecryptionRequest(int userId, string encryptedPassword);
		/// <summary>
		/// Releases held encrypted password
		/// </summary>
		void FinishDecryptionRequest(int userId);
		/// <summary></summary>
		/// <returns>Encrypted password of user or <see langword="null"/> if there is no decryption request</returns>
		string GetEncryptedPassword(int userId);
	}
}
