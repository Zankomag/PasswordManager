using PasswordManager.Bot.Commands.Abstractions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services;
using PasswordManager.Common.Extensions;
using PasswordManager.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Commands; 

public class SearchCommand : Abstractions.BotCommand, IActionCommand, ICallbackQueryCommand {
	//todo move to appsettings, and settings must be related to botUiSettings
	public const string AccountSeparator = "\n──────────────────";
	//todo move to appsettings, and settings must be related to botUiSettings
	/// <summary>
	/// How many accounts can be on a page
	/// </summary>
	private const int pageSize = 3;
		
	private readonly IAccountService accountService;
	private readonly IBotUi botUi;

	public SearchCommand(IBot bot, IAccountService accountService, IBotUi botUi) : base(bot) {
		this.accountService = accountService;
		this.botUi = botUi;
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
		} else if(accountCount <= pageSize) {
			var accounts = await accountService.GetAccountsByNameAsync(botUser.Id, 0, pageSize, accountName);
			await botUi.ShowAccountsPageAsync()
			await ShowSinglePage(botUser.Id, accountName, langCode);
		} else {
			await ShowPage(botUser.Id, accountName, 0, accountCount.PageCount(pageSize), langCode);
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

	//todo: delete this comment below
	//Moved from password manager
	private InlineKeyboardButton GetPageButton(bool next, int page, string accountName, string langCode) {
		if (accountName != null) {
			//Telegram inline button accepts only 64 bytes of data. UTF-16 string has 2 bytes per char.
			//So, search string can be no more than 64 - (4(>(2 bytes) + . (2 bytes)) + page.ToString().Length*2) chars
			int maxLength = (64 - (4/*(> + .)*/ + page.ToString().Length * 2));
			accountName = accountName.Length <= maxLength ?
				accountName : accountName.Substring(0, maxLength);
		}
		return InlineKeyboardButton.WithCallbackData(
			next ? "▶️ " + Localization.GetMessage("Next", langCode) :
				"◀️ " + Localization.GetMessage("Prev", langCode),
			CallbackCommandCode.Search.ToStringCode() + (next ? (page + 1).ToString() : (page - 1).ToString()) +
			"." + accountName);
	}
	

}