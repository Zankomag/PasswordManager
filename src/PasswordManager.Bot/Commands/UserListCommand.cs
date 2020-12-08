using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using MultiUserLocalization;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class UserListCommand: Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;

		public UserListCommand(IBotService botService, IUserService userService) : base(botService) {
			this.userService = userService;
		}

		public async Task ExecuteAsync(Message message, BotUser user) {
			if(user.Id == botService.AdminId.Identifier)
			{
				try
				{
					List<User> users = null;
					using (IDbConnection conn = new SQLiteConnection(botService.connString))
					{
						users = conn.Query<User>("select Id from Users").ToList();
					}
					string response = string.Empty;
					for(int i = 0; i < users.Count; i++)
					{
						response += (i + 1) + ". [" + users[i].Id + "](tg://user?id=" + users[i].Id + ") \n\n";
					}
					//TODO:
					//fix @UPwdBot, get bot nickname from bot and save in in bot class
					await botService.Client.SendTextMessageAsync(botService.AdminId, "All @UPwdBot users:\n\n" + response, Telegram.Bot.Types.Enums.ParseMode.Markdown);
				}
				catch(Exception ex)
				{
					await botService.Client.SendTextMessageAsync(botService.AdminId, "Error occured:\n\n" + ex.ToString());
				}
			}
		}
	}
}
