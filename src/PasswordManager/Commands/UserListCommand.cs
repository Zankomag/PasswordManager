using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Types;
using Uten.Localization.MultiUser;
using System;
using System.Data;
using System.Data.SQLite;
using Dapper;
using System.Collections.Generic;
using System.Linq;

namespace PasswordManager.Commands {
	public class UserListCommand: IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			if(user.Id == Bot.Instance.AdminId.Identifier)
			{
				try
				{
					List<Types.User> users = null;
					using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString))
					{
						users = conn.Query<Types.User>("select Id from User").ToList();
					}
					string response = string.Empty;
					for(int i = 0; i < users.Count; i++)
					{
						response += (i + 1) + ". [" + users[i].Id + "](tg://user?id=" + users[i].Id + ") \n\n";
					}
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "All @UPwdBot users:\n\n" + response, Telegram.Bot.Types.Enums.ParseMode.Markdown);
				}
				catch(Exception ex)
				{
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
