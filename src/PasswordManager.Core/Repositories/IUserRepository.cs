using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {
	public interface IUserRepository : IRepository<User>{
		/// <returns>User with Id and Lang</returns>
		Task<User> GetUserWithLangAsync(int userId);
		/// <returns>List of user Ids and theis Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();
		void UpdateAction(User user);
		void UpdateLanguage(User user);
		void UpdatePasswordPattern(User user);
	}
}
