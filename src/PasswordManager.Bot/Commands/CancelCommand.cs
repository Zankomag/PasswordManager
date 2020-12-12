﻿using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Commands {
	public class CancelCommand : Abstractions.BotCommand, IMessageCommand {
		private readonly IUserService userService;
		private readonly IAccountAssemblingService accountAssemblingService;

		public CancelCommand(IBot bot,
			IUserService userService,
			IAccountAssemblingService accountAssemblingService)
			: base(bot) {

			this.userService = userService;
			this.accountAssemblingService = accountAssemblingService;
		}

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			if (user.Action != UserAction.Search) {
				if(user.Action == UserAction.AssembleAccount)
					accountAssemblingService.Cancel(user.Id);
				await userService.UpdateActionAsync(user.Id, UserAction.Search);
				await bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("Cancel", user.Lang));
			}
			else {
				await bot.Client.SendTextMessageAsync(message.From.Id,
					Localization.GetMessage("NoCancel", user.Lang));
			}
		}
	}
}
