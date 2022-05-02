using System.Threading.Tasks;

namespace PasswordManager.Core.Repositories; 

public interface IUnitOfWork {

	IAccountRepository AccountRepository { get; }
	IUserRepository UserRepository { get; }

	Task SaveAsync();

}