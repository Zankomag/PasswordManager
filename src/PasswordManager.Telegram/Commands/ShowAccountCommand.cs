using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Telegram.Commands.Abstractions;
using PasswordManager.Telegram.Models;
using PasswordManager.Telegram.Services.Abstractions;
using Telegram.Bot;

namespace PasswordManager.Telegram.Commands; 

public class ShowAccountCommand : Abstractions.BotCommand, ICallbackQueryCommand {
	private readonly IAccountService accountService;
	private readonly ITelegramBotUi telegramBotUi;

	public ShowAccountCommand(IBot bot, IAccountService accountService, ITelegramBotUi telegramBotUi) : base(bot) {
		this.accountService = accountService;
		this.telegramBotUi = telegramBotUi;
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
		await telegramBotUi.ShowAccountAsync(botUser, account, callbackQuery.Message.MessageId);
	}

}