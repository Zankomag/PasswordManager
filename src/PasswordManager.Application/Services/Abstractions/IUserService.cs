using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions {
	public interface IUserService {
		Task<User> AddUserAsync(int userId, string langCode);
		///<summary></summary>
		/// <returns>User with Action and Lang</returns>
		Task<User> GetUserActionAsync(int userId);
		///<summary></summary>
		/// <returns>List of user Ids and theis Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();
		Task UpdateActionAsync(int userId, UserAction action);
		Task UpdateActionAsync(User user);
		Task<string> GetKeyHint(int userId);
		Task UpdateKeyHint(int userId, string keyHint);
		Task UpdateLanguage(int userId, string langCode);
		/// <summary>
		/// Deletes user if he is not admin
		/// </summary>
		Task<bool> DeleteUser(int userId);
	}
}
