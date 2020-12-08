using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using MultiUserLocalization;
using System;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class AddUserCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;

		public AddUserCommand(IBotService botService, IUserService userService) : base(botService) {
			this.userService = userService;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {

			if (botService.IsAdmin(user)) {
				if (message.Text.Contains(' ')) {
					try {
						string newUserIdStr = message.Text.Split(' ')[1];
						int newUserId = Convert.ToInt32(newUserIdStr);
						.AddUser(newUserId, Localization.DefaultLanguageCode);
						await botService.SendMessageToAllAdmins("Added new user.\n/userlist");
					}
					catch {
						await botService.SendMessageToAllAdmins("Invalid User Id");
					}
					
				}
			}
		}
	}
}
