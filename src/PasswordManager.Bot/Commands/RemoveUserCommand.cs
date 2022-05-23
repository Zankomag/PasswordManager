using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using Telegram.Bot;

namespace PasswordManager.Bot.Commands; 

public class RemoveUserCommand : Abstractions.BotCommand, IMessageCommand {
	private readonly IUserService userService;

	public RemoveUserCommand(IBot bot, IUserService userService) : base(bot) {
		this.userService = userService;
	}

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		if(Bot.IsAdmin(botUser)) {
			int spaceIndex;
			if ((spaceIndex = message.Text.IndexOf(' ')) != -1) {
				string userIdStr = message.Text[(spaceIndex+1)..];
				if(Int32.TryParse(userIdStr, out int userId)){
					if (Bot.IsAdmin(botUser)) {
						await Bot.Client.SendTextMessageAsync(botUser.Id,
							"You are trying to remove admin. I won't let you do this.");
						return;
					}

					if (await userService.DeleteUser(userId)) {
						await Bot.Client.SendTextMessageAsync(botUser.Id,
							"User and their data have been removed.");
					} else {
						await Bot.Client.SendTextMessageAsync(botUser.Id, "Invalid user id");
					}
				} else {
					await Bot.Client.SendTextMessageAsync(botUser.Id, "Invalid user id");
				}
			} else {
				await Bot.Client.SendTextMessageAsync(botUser.Id, "Use /adduser < user id >");
			}
		}
	}
}