using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using System;
using System.Data;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class RemoveUserCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, BotUser user) {
			if(user.Id == BotService.Instance.AdminId.Identifier)
			{
				try
				{
					string userIdStr = message.Text.Split(' ')[1];
					int userId = Convert.ToInt32(userIdStr);
					if(userId == BotService.Instance.AdminId.Identifier)
					{
						await BotService.Instance.Client.SendTextMessageAsync(BotService.Instance.AdminId, "You are trying to remove yourself. I won't let you do this.");
						return;
					}

					using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString))
					{
						conn.Execute("delete from Users where Id = @userId",
							new { userId});
					}
					await BotService.Instance.Client.SendTextMessageAsync(BotService.Instance.AdminId, "User and their data have been removed.");
				}
				catch(Exception ex)
				{
					await BotService.Instance.Client.SendTextMessageAsync(BotService.Instance.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
