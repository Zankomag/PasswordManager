﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Entities;
using PasswordManager.Core.Repositories;

namespace PasswordManager.Infrastructure.Repositories; 

public class UserRepository : Repository<User, long>, IUserRepository {
	public UserRepository(DbContext context) : base(context) { }

	private IQueryable<User> GetUser(long id) => dbSet.Where(x => x.Id == id);

	public async Task<User> GetUserActionAsync(long userId)
		=> await GetUser(userId)
			.Select(x => new User { 
				Id = userId,
				Language = x.Language,
				Action = x.Action})
			.AsNoTracking()
			.FirstOrDefaultAsync();

	public async Task<IList<User>> GetAllBasicInfoAsync()
		//=> await base.Get()
		//	.Select(x => new User { Id = x.Id, Accounts = new List<Account>(x.Accounts.Count) })
		//	.ToListAsync();
		//
		//TODO: 
		//If this call doesn't work or is not optimized 
		//then try call above
		//If it is too not optimized
		//then use Tuple
		=> await base.Get()
			.Select(x => new User { Id = x.Id })
			.Include(u => u.Accounts.Count)
			.ToListAsync();

	public void UpdateAction(User user) {
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		context.Entry(user).Property(x => x.Action).IsModified = true;
	}
	public void UpdateLanguage(User user) {
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		if (user.Language == null)
			throw new ArgumentException("user.Language cannot be null", nameof(user));
		context.Entry(user).Property(x => x.Language).IsModified = true;
	}
	public void UpdatePasswordGeneratorPattern(User user) {
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		if (user.KeyGeneratorSettings == null)
			throw new ArgumentException("user.KeyGeneratorSettings cannot be null", nameof(user));
		context.Entry(user).Property(x => x.KeyGeneratorSettings).IsModified = true;
	}

	public async Task<string> GetKeyHint(long userId)
		=> await GetUser(userId)
			.Select(x => x.KeyHint)
			.FirstOrDefaultAsync();
		
	public async Task<string> GetPasswordGeneratorPattern(long userId)
		=> await GetUser(userId)
			.Select(x => x.KeyGeneratorSettings)
			.FirstOrDefaultAsync();

	public void UpdateKeyHint(User user) {
		if (user == null)
			throw new ArgumentNullException(nameof(user));
		//KeyHint can be null so we dont check it for null equality
		context.Entry(user).Property(x => x.KeyHint).IsModified = true;
	}

	public async Task<User> GetUserOutdatedTimeAsync(long userId)
		=> await GetUser(userId)
			.Select(x => new User {
				Id = userId,
				OutdatedTime = x.OutdatedTime
			})
			.AsNoTracking()
			.FirstOrDefaultAsync();
}