using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Abstractions {

	/// <summary>
	/// This service is responsible for assembling account 
	/// by Account parts in sequential order
	/// </summary>
	public interface IAccountAssemblingService {
		/// <summary>
		/// Deletes assembling account of user
		/// </summary>
		void Cancel(int userId);
		/// <summary>
		/// Releases assembled Account.
		/// </summary>
		/// <returns>null if there is no corresponding assembling account or it's not completely assembled</returns>
		Account Release(int userId);

		//TODO:
		//Add method to assembly by hardcoRe (/add <accountName> \n [link] \n <login> \n <password> etc.
	}
}
