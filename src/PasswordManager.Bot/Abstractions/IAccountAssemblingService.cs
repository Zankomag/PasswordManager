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
		/// <returns>Assembled Account or null if there is no corresponding assembling account or it's not completely assembled</returns>
		Account Release(int userId);
		AccountAssemblingStage GetCurrentStage(int userId);
		AccountAssemblingStage GetNextStage(int userId);
		/// <summary>
		/// Creates new AssemblingAccoun even if there already is one
		/// </summary>
		/// <param name="args">inline Arguments of account to assemble</param>
		/// <returns>Next AssemblingState</returns>
		AccountAssemblingStage Create(int userId, string[] args);
		/// <summary>
		/// Adds property of Current AssemblingStage to AccountAssemblingModel
		/// </summary>
		/// <param name="property">value of property to add to assembling</param>
		/// <returns>Next AssemblingStage</returns>
		AccountAssemblingStage Assemble(int userId, string property);
		/// <summary>
		/// Skips AccountAssemblingStage
		/// </summary>
		/// <returns>Next AssemblingStage</returns>
		AccountAssemblingStage SkipStage(int userId, AccountAssemblingStageSkip accountAssemblingStageSkip);
		string GetAccountName(int userId);

	}
}
