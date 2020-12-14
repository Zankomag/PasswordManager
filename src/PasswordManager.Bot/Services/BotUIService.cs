using MultiUserLocalization;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Services {
	public class BotUIService : IBotUIService {
		private readonly IBot bot;

		public BotUIService(IBot bot) {
			this.bot = bot;
		}

		//TODO: 
		//Refactor, get rid of using hardcoded emoji
		public async Task ShowAccount(BotUser user, Account account, int? messageToEditId = null,
			string extraMessage = null, InlineKeyboardButton backButton = null) {

			if (account != null) {
				//TODO: Show Expiration settings button here
				//TODO: Show password outdated time like
				//(You should change the password in [PasswordUpdatedDate + UpdatedDate - DateTime.UTC.Now.Date])
				//TODO: Show login as `login` (monospace that can be copied)
				//TODO: RECODE THIS SHIT
				string message = account.AccountName 
					+ (account.Link == null ? "\n" + account.Link : null)
					+ (account.Note == null ? "\n" + account.Note : null)
					+ "\n" + Localization.GetMessage("Login", user.Lang) + ": " + account.Login;
				//TODO: Change Update account to edit account and delete Update password button from update account page
				//and add this button here instead
				var keyboardMarkup = new InlineKeyboardMarkup( new InlineKeyboardButton[][] {
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🔑 " + Localization.GetMessage("Password", user.Lang),
							CallbackQueryCommandCode.ShowPassword.ToStringCode() + account.Id)},
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"✏️ " + Localization.GetMessage("UpdateAcc", user.Lang),
							UpdateAccountCommandCode.SelectUpdateType.ToStringCode() + account.Id) },
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🗑 " + Localization.GetMessage("DeleteAcc", user.Lang),
							DeleteAccountCommandCode.AskForDeletion.ToStringCode() + account.Id) },
					new InlineKeyboardButton[] {
						//TODO:
						//Make sure this works
						backButton,
						InlineKeyboardButton.WithCallbackData(
							"🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
							CallbackQueryCommandCode.DeleteMessage.ToStringCode())
					}
					});
				if (extraMessage != null) {
					message = extraMessage + "\n\n" + message;
				}

				if (messageToEditId != null) {
					await bot.Client.SendTextMessageAsync(user.Id, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				} else {
					await bot.Client.EditMessageTextAsync(user.Id, messageToEditId.Value, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
			}
		}

		public InlineKeyboardMarkup GeneratePasswordKeyboardMarkup(BotUser user,
			GeneratePasswordCommandCode generatePasswordCommandCode) 
			=> new InlineKeyboardMarkup(
				InlineKeyboardButton.WithCallbackData(
					"🌋 " + Localization.GetMessage("Generate", user.Lang),
					generatePasswordCommandCode.ToStringCode()));
		

		//TODO: Expiration settings 
		//Show PasswordUpdatedDate (Password updated : 75 days ago/today. (17.03.2019))
		//Show OutdatedTime (It is considered outdated 365 days after it's been updated (in 40 days/today/*NOW*)
		//and should be changed after this period)
		//Explain when it takes outdated time 
		//([This password uses global validity period from your /settings.\n]
		//If you want to change validity period for this password, click the button below.)
		//*button* (Change validity period)

	}
}
