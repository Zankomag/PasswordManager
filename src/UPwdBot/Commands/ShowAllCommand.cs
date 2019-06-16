using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace UPwdBot.Commands {
	public class ShowAllCommand : ICommand {

		public async Task ExecuteAsync(Message message) {
			await BotHandler.Instance.Bot.SendTextMessageAsync(message.From.Id,
				GetAllAccounts(message.From.Id),
				disableWebPagePreview: true);
		}

		private string GetAllAccounts(int userID) {
			IEnumerable<Account> accounts;

			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				accounts = conn.Query<Account>("select * from Account where UserId = @UserId", new {UserID = userID});
			}

			if (!accounts.Any()) {
				return "You have no accounts.\nAdd one: /add";
			}

			string message = "Your accounts:";
			foreach (Account account in accounts) {
				message += "\n==========================";
				message += "\n" + account.AccountName;
				if(account.Link != null)
					message += "\n" + account.Link;
				message += "\nLogin: " + account.Login;
			}

			return message;
		}
	}
}
