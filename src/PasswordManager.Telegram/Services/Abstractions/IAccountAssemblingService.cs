using PasswordManager.Core.Entities;
using PasswordManager.Telegram.Services.Enums;

namespace PasswordManager.Telegram.Services.Abstractions; 

/// <summary>
/// This service is responsible for assembling account 
/// by Account parts in sequential order
/// </summary>
public interface IAccountAssemblingService {
	/// <summary>
	/// Deletes assembling account of user
	/// </summary>
	void Cancel(long userId);
	/// <returns>Assembled Account or null if there is no corresponding assembling account or it's not completely assembled</returns>
	Account Release(long userId);

	/// <summary>
	/// Creates new AssemblingAccoun even if there already is one
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="args">inline Arguments of account to assemble</param>
	/// <returns>Next AssemblingState</returns>
	/// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
	AccountAssemblingStage Create(long userId, string[] args = null);

	/// <summary>
	/// Adds property of Current AssemblingStage to AccountAssemblingModel
	/// if current Assemling Stage equals to expected and accepts property.
	/// Otherwise throws exception
	/// </summary>
	/// <param name="userId"></param>
	/// <param name="property">value of property to add to assembling</param>
	/// <param name="expectedAccountAssemblingStage">excpected assembling stage. 
	/// If expceted stage is unknown use <see cref="AccountAssemblingStage.None"/></param>
	/// <returns>Next AssemblingStage</returns>
	/// <exception cref="System.InvalidOperationException"></exception>
	/// <exception cref="System.ArgumentNullException"></exception>
	/// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"></exception>
	AccountAssemblingStage Assemble(long userId, string property,
		AccountAssemblingStage expectedAccountAssemblingStage = AccountAssemblingStage.None);
	/// <summary>
	/// Skips AccountAssemblingStage
	/// </summary>
	/// <returns>Next AssemblingStage</returns>
	AccountAssemblingStage SkipStage(long userId, AccountAssemblingStageSkip accountAssemblingStageSkip);
	string GetAccountName(long userId);

}