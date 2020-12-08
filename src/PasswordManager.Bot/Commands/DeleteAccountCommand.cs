using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class DeleteAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;

		public DeleteAccountCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			int accountId;
			Int32.TryParse(callbackQuery.Data.Substring(2), out accountId);
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
				if (accountId != 0) {
					using (IDbConnection conn = new SQLiteConnection(botService.connString)) {
						conn.Execute("delete from Accounts where Id = @Id and UserId = @UserId",
							new {
								Id = accountId,
								UserId = user.Id});
					}
					await botService.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						"✅ " + Localization.GetMessage("AccountDeleted", user.Lang));
				}
			}
		}
	}
}
