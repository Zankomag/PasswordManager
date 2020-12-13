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
		//Refactor, get rid of using hardcoded emoji and callback codes like '0'
		public async Task ShowAccount(BotUser user, Account account, int? messageToEditId = null, string extraMessage = null) {
			if (account != null) {
				//TODO: Show PasswordUpdatedDate (Password updated : 75 days ago) ( if 0 days = today)
				//TODO: Show password outdated time like (outdated in (PasswordUpdatedDate + UpdatedDate - DateTime.UTC.Now)
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
							CallbackQueryCommandCode.UpdateAccount.ToStringCode() + '0' + account.Id) },
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData(
							"🗑 " + Localization.GetMessage("DeleteAcc", user.Lang),
							CallbackQueryCommandCode.DeleteAccount.ToStringCode() + '0' + account.Id) },
					new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
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
	}
}
