using PasswordManager.Core.Entities;
using System;

namespace PasswordManager.Bot.Services.Abstractions {
	public interface IPasswordDecryptionService {
		/// <summary>
		/// Saves account with encrypted password of user to return it for decryption later
		/// </summary>
		void StartDecryptionRequest(int userId, Account account);
		/// <summary>
		/// Releases held account
		/// </summary>
		void FinishDecryptionRequest(int userId);
		/// <summary></summary>
		/// <returns>Account with encrypted password of user or <see langword="null"/> if there is no decryption request</returns>
		Account GetAccount(int userId);
	}
}
