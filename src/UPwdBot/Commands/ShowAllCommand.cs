using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class ShowAllCommand : IMessageCommand {

		public async Task ExecuteAsync(Message message, string langCode) {
			await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
				GetAllAccounts(message.From.Id, langCode),
				disableWebPagePreview: true);
		}

		private string GetAllAccounts(int userID, string langCode) {
			IEnumerable<Account> accounts;

			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				accounts = conn.Query<Account>("select * from Account where UserId = @UserId", new {UserID = userID});
			}

			if (!accounts.Any()) {
				return String.Format(Localization.GetMessage("NoAccounts", langCode),  "/add");
			}

			string message = Localization.GetMessage("AccountList", langCode);
			foreach (Account account in accounts) {
				message += "\n==========================";
				message += "\n" + account.AccountName;
				if(account.Link != null)
					message += "\n" + account.Link;
				message += "\n" + Localization.GetMessage("Login", langCode) + ": " + account.Login;
			}

			return message;
		}
	}
}
