using PasswordManager.Core.Entities;

namespace PasswordManager.Telegram.Services.Abstractions; 

public interface IPasswordDecryptionService {
	/// <summary>
	/// Saves account with encrypted password of user to return it for decryption later
	/// </summary>
	void StartDecryptionRequest(long userId, Account account);
	/// <summary>
	/// Releases held account
	/// </summary>
	void FinishDecryptionRequest(long userId);
	/// <summary></summary>
	/// <returns>Account with encrypted password of user or <see langword="null"/> if there is no decryption request</returns>
	Account GetAccount(long userId);
}