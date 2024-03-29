﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot;
using System;
using System.Data;
using System.Collections.Generic;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands; 

public class UserListCommand: Abstractions.BotCommand, IMessageCommand {
	private readonly IUserService userService;

	public UserListCommand(IBot bot, IUserService userService) : base(bot) {
		this.userService = userService;
	}

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		if(Bot.IsAdmin(botUser)) {
			try {
				IList<User> users = await userService.GetAllBasicInfoAsync();
				string response = String.Empty;
				//TODO: use string builder
				for(int i = 0; i < users.Count; i++) {
					response += $"{(i + 1)}. [{ users[i].Id}](tg://user?id={users[i].Id}): {users[i].Accounts.Count}\n\n";
				}
				//TODO:
				//fix @UPwdBot, get bot nickname from bot and save in in bot class
				await Bot.Client.SendTextMessageAsync(botUser.Id, "All @UPwdBot users:\nUser: Number of accounts\n" + response,
					Telegram.Bot.Types.Enums.ParseMode.Markdown);
			}
			catch(Exception ex) {
				await Bot.Client.SendTextMessageAsync(botUser.Id, "Error occured:\n\n" + ex.ToString());
				//TODO: Log Exception
			}
		}
	}
}