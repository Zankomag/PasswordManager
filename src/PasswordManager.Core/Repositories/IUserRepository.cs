﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Core.Entities;

namespace PasswordManager.Core.Repositories {

	public interface IUserRepository : IRepository<User> {

		/// <summary></summary>
		/// <returns>User with Action and Lang</returns>
		Task<User> GetUserActionAsync(int userId);

		/// <summary></summary>
		/// <returns>List of user Ids and theirs Accounts Count</returns>
		Task<IList<User>> GetAllBasicInfoAsync();

		void UpdateAction(User user);
		
		void UpdateLanguage(User user);
		
		void UpdatePasswordGeneratorPattern(User user);
		
		Task<string> GetKeyHint(int userId);

		Task<string> GetPasswordGeneratorPattern(int userId);
		
		void UpdateKeyHint(User user);
		
		Task<User> GetUserOutdatedTimeAsync(int userId);
		

	}

}