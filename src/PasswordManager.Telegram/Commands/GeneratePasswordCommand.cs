﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using PasswordManager.Application;
using PasswordManager.Telegram.Extensions;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Telegram.Services;
using PasswordManager.Core.Entities;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Commands.Enums;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using Telegram.Bot;

namespace PasswordManager.Telegram.Commands; 

public class GeneratePasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand {
	private readonly IAccountService accountService;
	private readonly IUserService userService;
	private readonly ITelegramBotUi telegramBotUi;

	public GeneratePasswordCommand(IBot bot, IAccountService accountService, IUserService userService, ITelegramBotUi telegramBotUi) : base(bot) {

		this.accountService = accountService;
		this.userService = userService;
		this.telegramBotUi = telegramBotUi;
	}
	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		string password;
		string passwordGeneratorPattern = await userService.GetPasswordGeneratorPattern(botUser.Id);

		//todo move this to botUserService or somewhere else
		try {
			password = passwordGeneratorPattern.GeneratePasswordByPattern();
		} catch (ArgumentException ex) {
			// Default password generator pattern will be set to user and a password will be generated by it
			await SetPasswordPatternToDefault(botUser);
			password = Password.DefaultPasswordGeneratorPattern.GeneratePasswordByPattern();
		}

		//todo move this to botUserService or somewhere else
		if (password.Length > Account.MaxPasswordLength) {
			// Default password generator pattern will be set to user and a password will be generated by it
			await SetPasswordPatternToDefault(botUser);
			password = Password.DefaultPasswordGeneratorPattern.GeneratePasswordByPattern();
		}

		password = password.Trim();

		//todo move to ui service
		var inlineKeyBoard = new InlineKeyboardMarkup(
			new[] {
				new[] {
					InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", botUser.Lang),
						callbackQuery.Data),
					InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", botUser.Lang),
						callbackQuery.Data[1] switch {
							(char)GeneratePasswordCommandCode.Assembling
								=> AddAccountCommandCode.AcceptPassword.ToStringCode(),
							(char)GeneratePasswordCommandCode.Updating
								=> UpdateAccountCommandCode.AcceptPassword.ToStringCode(callbackQuery.Data[2..]),
							_ => throw new InvalidEnumArgumentException($"Unknown password accepting command: {callbackQuery.Data[1]}")
						})
				}
			});

		string passwordMessage = telegramBotUi.GetPasswordMessage(password);
			
		await Bot.Client.EditMessageTextAsync(
			callbackQuery.From.Id,
			callbackQuery.Message.MessageId,
			passwordMessage,
			replyMarkup: inlineKeyBoard,
			parseMode: ParseMode.MarkdownV2);
			
	}

	//todo move this to botUserService or somewhere else
	private async Task SetPasswordPatternToDefault(BotUser botUser) {
		await userService.UpdatePasswordGeneratorPattern(botUser.Id, Password.DefaultPasswordGeneratorPattern);
		await Bot.Client.SendTextMessageAsync(botUser.Id, Localization.GetMessage("PasswordGeneratorPatternToDefault", botUser.Lang));
	}

		
}