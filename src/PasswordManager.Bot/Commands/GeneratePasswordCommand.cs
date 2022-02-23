﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using Passwords;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Services;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Commands {
	public class GeneratePasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;
		private readonly IUserService userService;
		private readonly IBotUIService botUi;

		public GeneratePasswordCommand(IBot bot, IAccountService accountService, IUserService userService, IBotUIService botUi) : base(bot) {

			this.accountService = accountService;
			this.userService = userService;
			this.botUi = botUi;
		}
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			string password;
			string passwordGeneratorPattern = await userService.GetPasswordGeneratorPattern(user.Id);
			try {
				password = passwordGeneratorPattern.GeneratePasswordByPattern();
			} catch (ArgumentException ex) {
				// Default password generator pattern will be set to user and a password will be generated by it
				await SetPasswordPatternToDefault(user);
				password = Password.DefaultPasswordGeneratorPattern.GeneratePasswordByPattern();
			}

			if (password.Length > Account.MaxPasswordLength) {
				// Default password generator pattern will be set to user and a password will be generated by it
				await SetPasswordPatternToDefault(user);
				password = Password.DefaultPasswordGeneratorPattern.GeneratePasswordByPattern();
			}

			password = password.Trim();

			//todo move to ui service
			var inlineKeyBoard = new InlineKeyboardMarkup(
				new[] {
					new[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							callbackQuery.Data),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							callbackQuery.Data[1] switch {
								(char)GeneratePasswordCommandCode.Assembling
									=> AddAccountCommandCode.AcceptPassword.ToStringCode(),
								(char)GeneratePasswordCommandCode.Updating
									=> UpdateAccountCommandCode.AcceptPassword.ToStringCode(callbackQuery.Data[2..]),
								_ => throw new InvalidEnumArgumentException($"Unknown password accepting command: {callbackQuery.Data[1]}")
							})
					}
				});

			string passwordMessage = botUi.GetPasswordMessage(password);
			
			await Bot.Client.EditMessageTextAsync(
				callbackQuery.From.Id,
				callbackQuery.Message.MessageId,
				passwordMessage,
				replyMarkup: inlineKeyBoard,
				parseMode: ParseMode.MarkdownV2);
			
		}

		private async Task SetPasswordPatternToDefault(BotUser botUser) {
			await userService.UpdatePasswordGeneratorPattern(botUser.Id, Password.DefaultPasswordGeneratorPattern);
			await Bot.Client.SendTextMessageAsync(botUser.Id, Localization.GetMessage("PasswordGeneratorPatternToDefault", botUser.Lang));
		}

		
	}
}
