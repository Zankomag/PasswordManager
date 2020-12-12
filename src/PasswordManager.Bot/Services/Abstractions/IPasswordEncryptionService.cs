using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IPasswordEncryptionService {
		/// <summary>
		/// Saves accountId of user to return it for recryption later
		/// </summary>
		void StartEncryptionRequest(int userId, long accountId);
		/// <summary>
		/// Releases held accountId
		/// </summary>
		void FinishEncryptionRequest(int userId);
		/// <summary></summary>
		/// <returns>accountId of user or <see langword="null"/> if there is no encryption request</returns>
		long? GetAccountId(int userId);
	}
}
