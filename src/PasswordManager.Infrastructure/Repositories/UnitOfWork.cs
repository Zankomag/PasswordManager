using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Core.Repositories;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Infrastructure.Repositories {
	public class UnitOfWork : IUnitOfWork {

		private readonly DbContext context;
		public UnitOfWork(PasswordManagerDbContext dbContext) {
			this.context = dbContext;
		}

		private IAccountRepository accountRepository;
		public IAccountRepository AccountRepository {
			get {
				//todo test solution that resharper suggests
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
