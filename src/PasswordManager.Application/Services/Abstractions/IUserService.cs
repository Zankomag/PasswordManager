using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Application.Services.Abstractions {

	public interface IUserService {

		Task<User> AddUserAsync(long userId, string langCode);

		/// <summary></summary>
		/// <returns>User with Action and Lang</returns>
		Task<User> GetUserActionAsync(long userId);

		/// <summary></summary>
		/// <returns>List of user Ids and theis Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();

		Task UpdateActionAsync(long userId, UserAction action);
		
		Task UpdateActionAsync(User user);
		
		Task<string> GetKeyHint(long userId);
		
		Task UpdateKeyHint(long userId, string keyHint);

		Task<string> GetPasswordGeneratorPattern(long userId);

		Task UpdatePasswordGeneratorPattern(long userId, string passwordGeneratorPattern);
		
		Task UpdateLanguage(long userId, string langCode);
		
		/// <summary>
		///     Deletes user if he is not admin
		/// </summary>
		Task<bool> DeleteUser(long userId);

		Task<User> GetUserOutdatedTimeAsync(long userId);

	}

}