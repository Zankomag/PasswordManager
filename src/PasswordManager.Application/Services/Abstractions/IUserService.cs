using PasswordManager.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions {
	public interface IUserService {
		Task<User> AddUserAsync(int userId, string langCode);
		/// <returns>User with Id and Lang</returns>
		Task<User> GetUserWithLangAsync(int userId);
		/// <returns>List of user Ids and theis Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();
		Task UpdateActionAsync(int userId, UserAction action);
		Task UpdateActionAsync(User user);
		Task<string> GetKeyHint(int userId);
		Task UpdateKeyHint(int userId, string keyHint);
		Task UpdateLanguage(int userId, string langCode);
	}
}
