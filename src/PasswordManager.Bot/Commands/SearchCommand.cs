using PasswordManager.Bot.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services;
using PasswordManager.Bot.Settings;
using PasswordManager.Common.Extensions;
using PasswordManager.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Commands; 

public class SearchCommand : Abstractions.BotCommand, IActionCommand, ICallbackQueryCommand {

	private readonly IAccountService accountService;
	private readonly IBotUi botUi;
	private readonly BotUiSettings uiSettings;

	public SearchCommand(IBot bot, IAccountService accountService, IBotUi botUi, IOptions<BotUiSettings> uiSettings) : base(bot) {
		this.accountService = accountService;
		this.botUi = botUi;
		this.uiSettings = uiSettings?.Value ?? throw new ArgumentNullException(nameof(uiSettings), $"{nameof(BotUiSettings)} value is null");
	}

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		string accountName = message.Text;
		int accountCount = await accountService.GetAccountsCountByNameAsync(botUser.Id, accountName);

		if(accountCount == 0) {
			await Bot.Client.SendTextMessageAsync(botUser.Id,
				String.Format(Localization.GetMessage(accountName != null ? "NotFound" : "NoAccounts", botUser.Lang), "/add"));
		} else if(accountCount == 1) {
			var account = await accountService.GetSingleAccountByNameAsync(botUser.Id, accountName);
			await botUi.ShowAccountAsync(botUser, account);
		} else if(accountCount <= uiSettings.PageSize) {
			var accounts = await accountService.GetAccountsByNameAsync(botUser.Id, 0, uiSettings.PageSize, accountName);
			await botUi.ShowAccountsPageAsync()
			await ShowSinglePage(botUser.Id, accountName, langCode);
		} else {
			await ShowPage(botUser.Id, accountName, 0, accountCount.PageCount(uiSettings.PageSize), langCode);
		}
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		int page = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
		string accountName = callbackQuery.Data.Length != (callbackQuery.Data.IndexOf('.') + 1) ?
			callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1) : null;
		int accountCount = .GetAccountCount(callbackQuery.From.Id, accountName);
		if(accountCount != 0) {
			await .ShowPage(callbackQuery.From.Id, accountName, page,
				.GetPageCount(accountCount),
			botUser.Lang, callbackQuery.Message.MessageId);
			await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
		} else {
			await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
				Localization.GetMessage("SearchAgain", botUser.Lang), showAlert: true);
			await BotHandler.TryDeleteMessageAsync(
				callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}


}