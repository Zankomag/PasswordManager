using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {

	public interface IUserRepository : IRepository<User, long> {

		/// <summary></summary>
		/// <returns>User with Action and Lang</returns>
		Task<User> GetUserActionAsync(long userId);

		/// <summary></summary>
		/// <returns>List of user Ids and theirs Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();

		void UpdateAction(User user);
		
		void UpdateLanguage(User user);
		
		void UpdatePasswordGeneratorPattern(User user);
		
		Task<string> GetKeyHint(long userId);

		Task<string> GetPasswordGeneratorPattern(long userId);
		
		void UpdateKeyHint(User user);
		
		Task<User> GetUserOutdatedTimeAsync(long userId);
		

	}

}