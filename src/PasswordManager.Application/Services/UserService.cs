using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;
using Passwords;

namespace PasswordManager.Application.Services {
	class UserService : IUserService {
		private readonly IUnitOfWork workUnit;

		public UserService(IUnitOfWork workUnit) {
			this.workUnit = workUnit;
		}

		public async Task<User> AddUserAsync(int userId, string langCode) {
			var user = new User {
				Id = userId,
				Lang = langCode,
				GenPattern = Password.DefaultPasswordGeneratorPattern,
				Action = UserAction.Search,
			};
			await workUnit.UserRepository.AddAsync(user);
			await workUnit.SaveAsync();
			return user;
		}
		public async Task<User> GetUserWithLangAsync(int userId) 
			=> await workUnit.UserRepository.GetUserWithLangAsync(userId);

		public async Task<IList<User>> GetAllBasicInfoAsync()
			=> await workUnit.UserRepository.GetAllBasicInfoAsync();
		

		public async Task UpdateActionAsync(User user) {
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			workUnit.UserRepository.UpdateAction(user);
			await workUnit.SaveAsync();
		}

		public async Task UpdateActionAsync(int userId, UserAction action) 
			=> await UpdateActionAsync(new User { Id = userId, Action = action });

		public async Task<string> GetKeyHint(int userId)
			=> await workUnit.UserRepository.GetKeyHint(userId);

		public async Task UpdateLanguage(int userId, string langCode) {
			if (langCode == null)
				throw new ArgumentNullException(nameof(langCode));
			workUnit.UserRepository.UpdateLanguage(new User { Id = userId, Lang = langCode });
			await workUnit.SaveAsync();
		}
	}
}
