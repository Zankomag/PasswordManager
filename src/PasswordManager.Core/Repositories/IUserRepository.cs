using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {
	public interface IUserRepository : IRepository<User>{
		/// <returns>User with Id and Lang</returns>
		Task<User> GetLangAsync(int userId);
		void UpdateAction(User user);
		void UpdateLanguage(User user);
		void UpdatePasswordPattern(User user);
	}
}
