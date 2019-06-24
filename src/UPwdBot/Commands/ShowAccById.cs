using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class ShowAccById : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			int accountId = Convert.ToInt32(callbackQuery.Data.Substring(1));
			Account account;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account = conn.QueryFirstOrDefault<Account>(
					"select Id, AccountName, Link, Login from Account where Id = @Id and UserId = @UserId",
					new {
						Id = accountId,
						UserId = callbackQuery.From.Id
					});
			}
			if (account != null) {
				string message =  account.Link!=null ? account.AccountName + "\n" + account.Link + "\n" +
					Localization.GetMessage("Login", user.Lang) + account.Login :
					account.AccountName + "\n" + Localization.GetMessage("Login", user.Lang) + account.Login;
				await Bot.Instance.Client.SendTextMessageAsync(callbackQuery.From.Id, message,
					replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
								"D") },
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData("🔑 " + Localization.GetMessage("Password", user.Lang),
								"P" + account.Id.ToString())} }), disableWebPagePreview: true);
			}
		}
	}
}
