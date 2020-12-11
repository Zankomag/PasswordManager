using MultiUserLocalization;
using PasswordManager.Bot.Abstractions;
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
		private readonly IBotService botService;

		public BotUIService(IBotService botService) {
			this.botService = botService;
		}

		//TODO: 
		//Refactor, get rid of using hardcoded emoji and callback codes like '0'
		public async Task ShowAccount(BotUser user, Account account, int? messageToEditId = null, string extraMessage = null) {
			if (account != null) {
				string message = account.Link != null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", user.Lang) + ": " + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", user.Lang) + ": " + account.Login;
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
					await botService.Client.SendTextMessageAsync(user.Id, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				} else {
					await botService.Client.EditMessageTextAsync(user.Id, messageToEditId.Value, message,
						replyMarkup: keyboardMarkup, disableWebPagePreview: true);
				}
			}
		}
	}
}
