using PasswordManager.Bot.Services.Enums;
using PasswordManager.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System;

namespace PasswordManager.Bot.Services.Abstractions {

	/// <summary>
	/// Saves updating type and account id of user to return it later
	/// </summary>
	public interface IAccountUpdatingService {
		/// <summary>
		/// Saves updating type and account of user to return it later.
		/// If updating request exists, it overrides with new
		/// </summary>
		void StartUpdatingRequest(long userId, Account account, AccountUpdatingStage accountUpdatingType);
		/// <summary>
		/// Releases held data
		/// </summary>
		void FinishUpdatingRequest(long userId);
		/// <summary></summary>
		/// <returns>Next updating stage or <see cref="AccountUpdatingStage.None"/>
		/// if there is no updating request
		/// or accountId doesn't match requested
		/// or updating account stage doesn't match expected</returns>
		/// <exception cref="ValidationException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		AccountUpdatingStage GetNextUpdatingStage(long userId, string property,
			AccountUpdatingStage expectedAccountUpdatingStage = AccountUpdatingStage.None,
			long? accountId = null);
		/// <summary></summary>
		/// <returns>Id of updating account and next updating stage or null and <see cref="AccountUpdatingStage.None"/>
		/// if there is no updating request</returns>
		/// <exception cref="ValidationException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		(long? accountId, AccountUpdatingStage) GetNextUpdatingStageAndAccountId(long userId, string property,
			AccountUpdatingStage expectedAccountUpdatingStage = AccountUpdatingStage.None);
		AccountUpdatingStage SkipNextUpdatingStage(long userId, long accountId,
			AccountUpdatingStage accountUpdatingStageToSkip);
		/// <summary></summary>
		/// <returns>Account with new property and finished updating request or <see langword="null"/> if there is no updating request
		/// or it's not to release ready yet</returns>
		Account ReleaseAccount(long userId);
	}
}
