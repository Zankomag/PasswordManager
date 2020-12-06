using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Types.Enums;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class DeleteAccountCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			int accountId;
			Int32.TryParse(callbackQuery.Data.Substring(2), out accountId);
			if (callbackQuery.Data[1] == '0') {
				var keyboardMarkup = new InlineKeyboardMarkup(
					new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								Localization.GetMessage("ImSure", user.Lang),
							 CallbackCommandCode.DeleteAccount.ToStringCode() + '1' + accountId)},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"❌ " + Localization.GetMessage("No!", user.Lang),
								CallbackCommandCode.ShowAccount.ToStringCode() + accountId) },
					});

				await BotService.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						Localization.GetMessage("SureDeleteAcc", user.Lang) + "\n\n" + callbackQuery.Message.Text,
						replyMarkup: keyboardMarkup,
						disableWebPagePreview: true);
			} else {
				if (accountId != 0) {
					using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
						conn.Execute("delete from Accounts where Id = @Id and UserId = @UserId",
							new {
								Id = accountId,
								UserId = user.Id});
					}
					await BotService.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						"✅ " + Localization.GetMessage("AccountDeleted", user.Lang));
				}
			}
		}
	}
}
