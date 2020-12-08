using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using MultiUserLocalization;
using System;
using System.Data;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class RemoveUserCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;

		public RemoveUserCommand(IBotService botService, IUserService userService) : base(botService) {
			this.userService = userService;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			if(botService.IsAdmin(user)) {
				try {
					string userIdStr = message.Text.Split(' ')[1];
					int userId = Convert.ToInt32(userIdStr);
					if(botService.IsAdmin(user)) {
						await botService.SendMessageToAllAdmins("You are trying to remove admin. I won't let you do this.");
						return;
					}

					using (IDbConnection conn = new SQLiteConnection(botService.connString))
					{
						conn.Execute("delete from Users where Id = @userId",
							new { userId});
					}
					await botService.SendMessageToAllAdmins("User and their data have been removed.");
				}
				catch(Exception ex)
				{
					await botService.SendMessageToAllAdmins("Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
