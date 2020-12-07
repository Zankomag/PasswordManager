using PasswordManager.Core.Entities;
using System.Threading.Tasks;

namespace PasswordManager.Application.Services.Abstractions {
	public interface IUserService {
		Task<User> AddUserAsync(int userId, string langCode);
		/// <returns>User with Id and Lang</returns>
		Task<User> GetLangAsync(int userId);
		Task UpdateActionAsync(int userId, UserAction action);
		Task UpdateActionAsync(User user);
	}
}
