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
		public async Task ExecuteAsync(Message message, BotUser user) {
			if(user.Id == botService.AdminId.Identifier)
			{
				try
				{
					string userIdStr = message.Text.Split(' ')[1];
					int userId = Convert.ToInt32(userIdStr);
					if(userId == botService.AdminId.Identifier)
					{
						await botService.Client.SendTextMessageAsync(botService.AdminId, "You are trying to remove yourself. I won't let you do this.");
						return;
					}

					using (IDbConnection conn = new SQLiteConnection(botService.connString))
					{
						conn.Execute("delete from Users where Id = @userId",
							new { userId});
					}
					await botService.Client.SendTextMessageAsync(botService.AdminId, "User and their data have been removed.");
				}
				catch(Exception ex)
				{
					await botService.Client.SendTextMessageAsync(botService.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
