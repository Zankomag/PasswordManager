using System.Threading.Tasks;
using Telegram.Bot.Types;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using System;
using System.Data;
using System.Data.SQLite;
using Dapper;

namespace UPwdBot.Commands {
	public class RemoveUserCommand: IMessageCommand {
		public async Task ExecuteAsync(Message message, Types.User user) {
			if(user.Id == Bot.Instance.AdminId.Identifier)
			{
				try
				{
					string userIdStr = message.Text.Split(' ')[1];
					int userId = Convert.ToInt32(userIdStr);
					if(userId == Bot.Instance.AdminId.Identifier)
					{
						await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "You are trying to remove yourself. I won't let you do this.");
						return;
					}

					using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString))
					{
						conn.Execute("delete from User where Id = @userId",
							new { userId});
						conn.Execute("delete from Account where UserId = @userId",
							new { userId });
					}
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "User and their data have been removed.");
				}
				catch(Exception ex)
				{
					await Bot.Instance.Client.SendTextMessageAsync(Bot.Instance.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
