using PasswordManager.Core.Entities;
using PasswordManager.Telegram.Services.Enums;

namespace PasswordManager.Telegram.Models; 

public class AccountUpdatingModel {
	public AccountUpdatingStage AccountUpdatingStage { get; set; }
	public Account Account { get; set; }

	public AccountUpdatingModel(AccountUpdatingStage accountUpdatingStage, Account account) {
		AccountUpdatingStage = accountUpdatingStage;
		Account = account;
	}
}