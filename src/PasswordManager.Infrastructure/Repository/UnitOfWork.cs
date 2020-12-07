using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Repositories;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Infrastructure.Repository {
	public class UnitOfWork : IUnitOfWork {

		private DbContext context;
		public UnitOfWork(PasswordManagerDbContext dbContext) {
			this.context = dbContext;
		}

		private IAccountRepository accountRepository;
		public IAccountRepository AccountRepository {
			get {
				if (accountRepository == null)
					accountRepository = new AccountRepository(context);
				return accountRepository;
			}
		}

		private IUserRepository userRepository;
		public IUserRepository UserRepository {
			get {
				if (userRepository == null)
					userRepository = new UserRepository(context);
				return userRepository;
			}
		}

		public async Task SaveAsync() => await context.SaveChangesAsync();
	}
}
