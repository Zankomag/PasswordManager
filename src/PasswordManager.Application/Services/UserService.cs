using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Application.Services; 

public class UserService : IUserService {
	private readonly IUnitOfWork workUnit;
	private readonly IApplicationService applicationService;

	public UserService(IUnitOfWork workUnit, IApplicationService applicationService) {
		this.workUnit = workUnit;
		this.applicationService = applicationService;
	}

	public async Task<User> AddUserAsync(long userId, string langCode) {
		var user = new User {
			Id = userId,
			Lang = langCode,
			PasswordGeneratorPattern = Password.DefaultPasswordGeneratorPattern,
			Action = UserAction.Search
		};
		await workUnit.UserRepository.AddAsync(user);
		await workUnit.SaveAsync();
		return user;
	}
	public async Task<User> GetUserActionAsync(long userId) 
		=> await workUnit.UserRepository.GetUserActionAsync(userId);

	public async Task<IList<User>> GetAllBasicInfoAsync()
		=> await workUnit.UserRepository.GetAllBasicInfoAsync();
		

	public async Task UpdateActionAsync(User user) {
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		workUnit.UserRepository.UpdateAction(user);
		await workUnit.SaveAsync();
	}

	public async Task UpdateActionAsync(long userId, UserAction action) 
		=> await UpdateActionAsync(new User { Id = userId, Action = action });

	public async Task<string> GetKeyHint(long userId)
		=> await workUnit.UserRepository.GetKeyHint(userId);

	public async Task<string> GetPasswordGeneratorPattern(long userId) 
		=> await workUnit.UserRepository.GetPasswordGeneratorPattern(userId);

	public async Task UpdateLanguage(long userId, string langCode) {
		if (langCode == null) throw new ArgumentNullException(nameof(langCode));
		workUnit.UserRepository.UpdateLanguage(new User { Id = userId, Lang = langCode });
		await workUnit.SaveAsync();
	}

	public async Task UpdateKeyHint(long userId, string keyHint) {
		// keyHint can be null so we dont check it for null equality
		workUnit.UserRepository.UpdateKeyHint(new User { Id = userId, KeyHint = keyHint });
		await workUnit.SaveAsync();
	}

	public async Task UpdatePasswordGeneratorPattern(long userId, string passwordGeneratorPattern) {
		if(passwordGeneratorPattern is null) throw new ArgumentNullException(nameof(passwordGeneratorPattern));
		workUnit.UserRepository.UpdatePasswordGeneratorPattern(new User { Id = userId, PasswordGeneratorPattern = passwordGeneratorPattern });
		await workUnit.SaveAsync();
	}

	public async Task<bool> DeleteUser(long userId) {
		try {
			if (!applicationService.AdminIds.Contains(userId)) {
				if (workUnit.UserRepository.Delete(new User { Id = userId })) {
					await workUnit.SaveAsync();
					return true;
				}
			}
		} catch {
			// TODO log exception
		}
		return false;
	}

	public async Task<User> GetUserOutdatedTimeAsync(long userId)
		=> await workUnit.UserRepository.GetUserOutdatedTimeAsync(userId);
}