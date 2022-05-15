using System.Threading.Tasks;
using Telegram.Bot.Types;
using System;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;

namespace PasswordManager.Bot.Commands; 

public class AddUserCommand : Abstractions.BotCommand, IMessageCommand {
	private readonly IUserService userService;

	public AddUserCommand(IBot bot, IUserService userService) : base(bot) {
		this.userService = userService;
	}

	//This command allows admin manually add users to bot
	//Bot don't need this command if it has free registration
	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		if (Bot.IsAdmin(botUser)) {
			int spaceIndex;
			if ((spaceIndex = message.Text.IndexOf(' ')) != -1) {
				try {
					string newUserIdStr = message.Text[(spaceIndex+1)..];
					int newUserId = Convert.ToInt32(newUserIdStr);
					await userService.AddUserAsync(newUserId, Localization.DefaultLanguageCode);
				}
				catch {
					await Bot.Client.SendTextMessageAsync(botUser.Id, "Invalid user id");
					return;
				}
				await Bot.Client.SendTextMessageAsync(botUser.Id, "New user has been added successfully\n/userlist");
				return;
			}
			await Bot.Client.SendTextMessageAsync(botUser.Id, "Use /adduser <user id>");
		}
	}
}