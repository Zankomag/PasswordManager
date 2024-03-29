﻿using System;
using PasswordManager.Bot.Commands.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands; 

public class ShowAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
	private readonly IAccountService accountService;
	private readonly IBotUi botUi;

	public ShowAccountCommand(IBot bot, IAccountService accountService, IBotUi botUi) : base(bot) {
		this.accountService = accountService;
		this.botUi = botUi;
	}

	//todo rename all BotUser params to botUser as here
	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
		string accountIdString = callbackQuery.Data[1..];
		//TODO create custom Exception instead of default one. Also check other cases
		if(!Int64.TryParse(accountIdString, out long accountId))
			throw new Exception($"Unable to parse accountId to Int64: {accountIdString}");

		var account = await accountService.GetAccountAsync(botUser.Id, accountId);
		//todo add backButtonCommandCode
		await botUi.ShowAccountAsync(botUser, account, callbackQuery.Message.MessageId);
	}

}