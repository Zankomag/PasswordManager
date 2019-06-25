using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class ShowAccountCommand : ICallBackQueryCommand {
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
			await PasswordManager.ShowAccount(callbackQuery.From.Id, account, user.Lang, callbackQuery.Message.MessageId);
		}

	}
}
