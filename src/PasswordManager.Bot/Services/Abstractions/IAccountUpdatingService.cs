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
		void StartUpdatingRequest(int userId, Account account, AccountUpdatingStage accountUpdatingType);
		/// <summary>
		/// Releases held data
		/// </summary>
		void FinishUpdatingRequest(int userId);
		/// <summary></summary>
		/// <returns>Next updating stage or <see cref="AccountUpdatingStage.None"/>
		/// if there is no updating request
		/// or accountId doesn't match requested
		/// or updating account stage doesn't match expected</returns>
		/// <exception cref="ValidationException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		AccountUpdatingStage GetNextUpdatingStage(int userId, string property,
			AccountUpdatingStage expectedAccountUpdatingStage = AccountUpdatingStage.None,
			long? accountId = null);
		AccountUpdatingStage SkipNextUpdatingStage(int userId, long accountId,
			AccountUpdatingStage accountUpdatingStageToSkip);
		/// <summary></summary>
		/// <returns>Account with new property or <see langword="null"/> if there is no updating request
		/// or it's not to release ready yet</returns>
		Account GetAccount(int userId);
	}
}
