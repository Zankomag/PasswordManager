﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Telegram.Settings;

namespace PasswordManager.Telegram.Commands; 

public class ShowAllAccountsCommand : Abstractions.BotCommand, IMessageCommand {
	private readonly IAccountService accountService;
	private readonly ITelegramBotUi telegramBotUi;
	private readonly BotUiSettings botUiSettings;

	public ShowAllAccountsCommand(IBot bot, IAccountService accountService, ITelegramBotUi telegramBotUi, IOptions<BotUiSettings> uiSettings) : base(bot) {
		this.accountService = accountService;
		this.telegramBotUi = telegramBotUi;
		this.botUiSettings = uiSettings?.Value ?? throw new ArgumentNullException(nameof(uiSettings), $"{nameof(BotUiSettings)} value is null");
	}

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		//todo if we have this logic both here and in SeachCommand - we need to extract in may be to extension method
		int accountCount = await accountService.GetAccountCountByNameAsync(botUser.Id);
		var firstPageAccounts = (await accountService.GetAccountsByNameAsync(botUser.Id, 0, botUiSettings.PageSize)).ToList();
		await telegramBotUi.ShowAccountsPageAsync(botUser, firstPageAccounts, accountCount, 0, null);
	}

}