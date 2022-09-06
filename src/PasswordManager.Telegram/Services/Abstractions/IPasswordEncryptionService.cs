
namespace PasswordManager.Telegram.Services.Abstractions; 

public interface IPasswordEncryptionService {
	/// <summary>
	/// Saves accountId of user to return it for recryption later
	/// </summary>
	void StartEncryptionRequest(long userId, long accountId);
	/// <summary>
	/// Releases held accountId
	/// </summary>
	void FinishEncryptionRequest(long userId);
	/// <summary></summary>
	/// <returns>accountId of user or <see langword="null"/> if there is no encryption request</returns>
	long? GetAccountId(long userId);
}