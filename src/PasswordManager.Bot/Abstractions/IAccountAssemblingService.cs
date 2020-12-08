using PasswordManager.Core.Entities;
using PasswordManager.Bot.Enums;

namespace PasswordManager.Bot.Abstractions {

	/// <summary>
	/// This service is responsible for assembling account 
	/// by Account parts in sequential order
	/// </summary>
	public interface IAccountAssemblingService {
		/// <summary>
		/// Deletes assembling account of user
		/// </summary>
		void Cancel(int userId);
		/// <summary>
		/// Releases assembled Account.
		/// </summary>
		/// <returns>null if there is no corresponding assembling account or it's not completely assembled</returns>
		Account Release(int userId);
		AccountAssemblingStage GetCurrentStage(int userId);
		AccountAssemblingStage GetNextStage(int userId);
		/// <summary>
		/// Creates new AssemblingAccoun even if there already is one
		/// </summary>
		/// <returns>Next AssemblingState</returns>
		AccountAssemblingStage Create(int userId);
		/// <summary>
		/// Creates new AssemblingAccoun even if there already is one
		/// </summary>
		/// <param name="args">Arguments of inline assembling</param>
		/// <returns>Next AssemblingState</returns>
		AccountAssemblingStage Create(int userId, string[] args);
		/// <summary>
		/// Adds property of Current AssemblingStage to AccountAssemblingModel
		/// </summary>
		/// <param name="property">value of property to add to assembling</param>
		/// <returns>Next AssemblingStage</returns>
		AccountAssemblingStage Assemble(int userId, string property);
		/// <summary>
		/// Skips allowed AccountAssemblingStage
		/// </summary>
		/// <param name="accountAssemblingStage">Stage to skip. Allowed values are:
		/// <see cref="AccountAssemblingStage.AddLink"/>
		/// <see cref="AccountAssemblingStage.AddNote"/>
		/// <see cref="AccountAssemblingStage.EncryptPassword"/></param>
		/// <returns>Next AssemblingStage</returns>
		AccountAssemblingStage SkipStage(int userId, AccountAssemblingStage accountAssemblingStage);

	}
}
