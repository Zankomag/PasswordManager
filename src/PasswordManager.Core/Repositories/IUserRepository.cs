using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {
	public interface IUserRepository : IRepository<User>{
		void UpdatePasswordPattern(int userId, string passwordPattern);
		void UpdateLanguage(int userId, string langCode);
		void UpdateAction(int userId, UserAction action);
		/// <returns>User with Id and Lang</returns>
		Task<User> GetLangAsync(int userId);
	}
}
