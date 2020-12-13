using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Enums;

namespace PasswordManager.Bot.Services.Abstractions {

	/// <summary>
	/// Saves updating type and account id of user to return it later
	/// </summary>
	public interface IAccountUpdatingService {
		/// <summary>
		/// Saves updating type and account id of user to return it later
		/// </summary>
		void StartUpdatingRequest(int userId, long accountId, AccountUpdatingType accountUpdatingType);
		/// <summary>
		/// Releases held data
		/// </summary>
		void FinishUpdatingRequest(int userId);
		/// <summary></summary>
		/// <returns>Updating type and account id of user or <see langword="null"/> if there is no updating request</returns>
		AccountUpdatingModel GetAccountUpdatingModel(int userId);
	}
}
