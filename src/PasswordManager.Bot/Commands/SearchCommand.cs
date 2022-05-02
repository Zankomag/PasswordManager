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
using PasswordManager.Core.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Commands; 

public class SearchCommand : Abstractions.BotCommand, IActionCommand, ICallbackQueryCommand {
	//todo move to botUi and later to appsettings
	public const string AccountSeparator = "\n──────────────────";
	//todo move to botUi and later to appsettings
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
		int accountCount = await accountService.GetAccountCountByNameAsync(botUser.Id, accountName);

		if(accountCount == 1) {
			await ShowAccountByName(chatId, accountName, langCode);
		} else if(accountCount == 0) {
			await Bot.Client.SendTextMessageAsync(chatId,
				String.Format(Localization.GetMessage(accountName != null ? "NotFound" : "NoAccounts", langCode), "/add"));
		} else if(accountCount <= pageSize) {
			await ShowSinglePage(chatId, accountName, langCode);
		} else {
			await ShowPage(chatId, accountName, 0, GetPageCount(accountCount), langCode);
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

	//todo: delete this comment below
	//Moved from password manager
	public static async Task ShowPage(int userId,
		string accountName, int page, int pageCount,
		string langCode, int messageToEditId = 0) {

		List<Account> accounts;
		if (accountName != null) {
			using (IDbConnection conn = new SQLiteConnection(Bot.connString)) {
				accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where UserId = @userId and AccountName like @AccountName " +
						"limit @pageSize offset @Offset",
						new {
							userId,
							AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%",
							maxAccsByPage = pageSize,
							Offset = page * pageSize
						})
					.ToList();
			}
		} else {
			using (IDbConnection conn = new SQLiteConnection(Bot.connString)) {
				accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where userId = @UserId " +
						"limit @pageSize offset @Offset",
						new {
							userId,
							maxAccsByPage = pageSize,
							Offset = page * pageSize
						})
					.ToList();
			}
		}

		string message = Localization.GetMessage("Page", langCode) + " " + (page + 1) + "/" + pageCount + "\n";
		message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, false, langCode, message);

		//This is first page
		if (page == 0) {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(true, page, accountName, langCode)};
		}
		//This is last page
		else if (page == pageCount - 1) {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(false, page, accountName, langCode)};
		}
		//This is middle page
		else {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(false, page, accountName, langCode),
				GetPageButton(true, page, accountName, langCode)};
		}

		if (messageToEditId == 0) {
			await Bot.Client.SendTextMessageAsync(userId, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		} else {
			await Bot.Client.EditMessageTextAsync(userId, messageToEditId, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		}
	}

	//todo: delete this comment below
	//Moved from password manager
	private static async Task ShowSinglePage(int userId, string accountName, string langCode) {
		List<Account> accounts;
		if (accountName != null) {
			using (IDbConnection conn = new SQLiteConnection(Bot.connString)) {
				accounts = conn.Query<Account>(
					"select Id, AccountName, Link, Login from Accounts where UserId = @userId and AccountName like @AccountName",
					new {
						userId,
						AccountName = "%" + accountName.Replace("[", "[[]").Replace("%", "[%]") + "%"
					}).ToList();
			}
		} else {
			using (IDbConnection conn = new SQLiteConnection(Bot.connString)) {
				accounts = conn.Query<Account>(
						"select Id, AccountName, Link, Login from Accounts where UserId = @userId",
						new { userId })
					.ToList();
			}
		}

		string message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, true, langCode);

		await Bot.Client.SendTextMessageAsync(userId, message,
			replyMarkup: new InlineKeyboardMarkup(keyboard),
			disableWebPagePreview: true);
	}

	//todo: delete this comment below
	//Moved from password manager
	private static string GetPageMessage(List<Account> accounts,
		out InlineKeyboardButton[][] keyboard,
		bool singlePage, string langCode, string message = null) {

		if (message == null)
			message = "";

		keyboard = new InlineKeyboardButton[singlePage ? accounts.Count : accounts.Count + 1][];

		for (int i = 0; i < accounts.Count; i++) {
			if (i != 0)
				message += AccountSeparator;
			message += "\n" + accounts[i].AccountName;
			if (accounts[i].Link != null)
				message += "\n" + accounts[i].Link;
			message += "\n" + Localization.GetMessage("Login", langCode) + ": " + accounts[i].Login;
			keyboard[i] = new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData(
				(i + 1) + "⃣ " + accounts[i].AccountName,
				CallbackCommandCode.ShowAccount.ToStringCode() + accounts[i].Id.ToString()) };
		}
		return message;
	}

		
	//todo: delete this comment below
	//Moved from password manager
	public static int GetPageCount(int accountCount) {
		return (int)Math.Ceiling(accountCount / (double)pageSize);
	}

}