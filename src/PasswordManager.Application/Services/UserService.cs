using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;
using Passwords;

namespace PasswordManager.Application.Services {
	public class UserService : IUserService {
		private readonly IUnitOfWork workUnit;
		private readonly IApplicationService applicationService;

		public UserService(IUnitOfWork workUnit, IApplicationService applicationService) {
			this.workUnit = workUnit;
			this.applicationService = applicationService;
		}

		public async Task<User> AddUserAsync(int userId, string langCode) {
			var user = new User {
				Id = userId,
				Lang = langCode,
				GenPattern = Password.DefaultPasswordGeneratorPattern,
				Action = UserAction.Search
			};
			await workUnit.UserRepository.AddAsync(user);
			await workUnit.SaveAsync();
			return user;
		}
		public async Task<User> GetUserActionAsync(int userId) 
			=> await workUnit.UserRepository.GetUserActionAsync(userId);

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

		public async Task UpdateKeyHint(int userId, string keyHint) {
			//keyHint can be null so we dont check it for null equality
			workUnit.UserRepository.UpdateKeyHint(new User { Id = userId, KeyHint = keyHint });
			await workUnit.SaveAsync();
		}

		public async Task<bool> DeleteUser(int userId) {
			try {
				if (!applicationService.Admins.Contains(userId)) {
					if (workUnit.UserRepository.Delete(new User { Id = userId })) {
						await workUnit.SaveAsync();
						return true;
					}
				}
			} catch { }
			return false;
		}
	}
}
