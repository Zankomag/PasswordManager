using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands {
	public class DeleteAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public DeleteAccountCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		//TODO: refacor
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			int accountId;
			Int32.TryParse(callbackQuery.Data[2..], out accountId);
			if (callbackQuery.Data[1] == '0') {
				var keyboardMarkup = new InlineKeyboardMarkup(
					new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								Localization.GetMessage("ImSure", user.Lang),
							 CallbackQueryCommandCode.DeleteAccount.ToStringCode() + '1' + accountId)},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"❌ " + Localization.GetMessage("No!", user.Lang),
								CallbackQueryCommandCode.ShowAccount.ToStringCode() + accountId) },
					});

				await botService.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						Localization.GetMessage("SureDeleteAcc", user.Lang) + "\n\n" + callbackQuery.Message.Text,
						replyMarkup: keyboardMarkup,
						disableWebPagePreview: true);
			} else {
				if(accountId != 0 && await accountService.DeleteAccountAsync(user.Id, accountId)) {
					await botService.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						"✅ " + Localization.GetMessage("AccountDeleted", user.Lang));
				}
			}
		}
	}
}
