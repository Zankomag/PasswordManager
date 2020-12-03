using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Uten.Localization.MultiUser;
using PasswordManager.Extensions;
using PasswordManager.Types.Enums;

namespace PasswordManager.Commands {
	public class DeleteAccountCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
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

				await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						Localization.GetMessage("SureDeleteAcc", user.Lang) + "\n\n" + callbackQuery.Message.Text,
						replyMarkup: keyboardMarkup,
						disableWebPagePreview: true);
			} else {
				if (accountId != 0) {
					using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
						conn.Execute("delete from Account where Id = @Id and UserId = @UserId",
							new {
								Id = accountId,
								UserId = user.Id});
					}
					await Bot.Instance.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id,
						callbackQuery.Message.MessageId,
						"✅ " + Localization.GetMessage("AccountDeleted", user.Lang));
				}
			}
		}
	}
}
