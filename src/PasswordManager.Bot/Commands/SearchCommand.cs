using PasswordManager.Bot.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
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
	private readonly BotUiSettings botUiSettings;

	public SearchCommand(IBot bot, IAccountService accountService, IBotUi botUi, IOptions<BotUiSettings> uiSettings) : base(bot) {
		this.accountService = accountService;
		this.botUi = botUi;
		this.botUiSettings = uiSettings?.Value ?? throw new ArgumentNullException(nameof(uiSettings), $"{nameof(BotUiSettings)} value is null");
	}

	async Task IActionCommand.ExecuteAsync(Message message, BotUser botUser) {
		string accountName = message.Text;
		int accountCount = await accountService.GetAccountsCountByNameAsync(botUser.Id, accountName);

		switch(accountCount) {
			case 0:
				await Bot.Client.SendTextMessageAsync(botUser.Id,
					String.Format(Localization.GetMessage(accountName != null ? "NotFound" : "NoAccounts", botUser.Lang), "/add"));
				break;
			case 1: {
				var account = await accountService.GetSingleAccountByNameAsync(botUser.Id, accountName);
				await botUi.ShowAccountAsync(botUser, account);
				break;
			}
			default: {
				var firstPageAccounts = (await accountService.GetAccountsByNameAsync(botUser.Id, 0, botUiSettings.PageSize, accountName)).ToList();
				await botUi.ShowAccountsPageAsync(botUser, firstPageAccounts, accountCount, 0, accountName);
				break;
			}
		}
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		//todo refactor this whole method
		int pageIndex = Convert.ToInt32(callbackQuery.Data.Substring(1, callbackQuery.Data.IndexOf('.')-1));
		
		string accountName = callbackQuery.Data.Length != (callbackQuery.Data.IndexOf('.') + 1) ?
			callbackQuery.Data.Substring(callbackQuery.Data.IndexOf('.') + 1) : null;
		//todo I guess for all callback queries we need to check (or double check) if callbackQuery.From.Id equals to chat id (and chat must be private)
		int accountCount = await accountService.GetAccountsCountByNameAsync(callbackQuery.From.Id, accountName);
		if(accountCount != 0) {
			var pageAccounts = (await accountService.GetAccountsByNameAsync(botUser.Id, pageIndex, botUiSettings.PageSize, accountName)).ToList();
			await botUi.ShowAccountsPageAsync(botUser, pageAccounts, accountCount, pageIndex, accountName);

			//todo do we need to answer???
			//await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
		} else {
			//todo do we need to delete/answer or what the fuck is this case even? We need to test it
			await Bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
				Localization.GetMessage("SearchAgain", botUser.Lang), showAlert: true);
			await Bot.TryDeleteMessageAsync(
				callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
		}
	}


}