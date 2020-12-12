using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IPasswordEncryptionService {
		/// <summary>
		/// Saves account with encrypted or not password of user to return it for recryption later
		/// </summary>
		void StartEncryptionRequest(int userId, Account account);
		/// <summary>
		/// Releases held account
		/// </summary>
		void FinishEncryptionRequest(int userId);
		/// <summary></summary>
		/// <returns>Account with encrypted or not password of user or <see langword="null"/> if there is no encryption request</returns>
		Account GetAccount(int userId);
	}
}
