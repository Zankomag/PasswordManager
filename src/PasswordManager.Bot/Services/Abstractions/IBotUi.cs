using System.Collections.Generic;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace PasswordManager.Bot.Services.Abstractions; 

public interface IBotUi {
	/// <summary>
	/// Shows full account data with buttons
	/// </summary>
	/// <param name="account"></param>
	/// <param name="messageToEditId">If specified, message will be edited instead of sending new</param>
	/// <param name="extraMessage">An Extra message to show with account data</param>
	/// <param name="backButtonCommandCode">If specified, the Back button with this command code
	/// will be attached to the end of button list</param>
	/// <param name="botUser"></param>
	/// <returns></returns>
	Task ShowAccountAsync(BotUser botUser,
		Account account,
		int? messageToEditId = null,
		string extraMessage = null,
		string backButtonCommandCode = null);

	/// <summary>
	/// Shows page with accounts and buttons to select them
	/// If count of accounts is greater than pageSize -
	/// also shows pageIndex/totalPages
	/// </summary>
	/// <returns></returns>
	/// <param name="botUser"></param>
	/// <param name="accounts"></param>
	/// <param name="totalAccountCount">number of accounts returned by search</param>
	/// <param name="pageIndex"></param>
	/// <param name="searchQuery">query of account search (basically name of account we need to find) used to place it in button for future searches if we have more than 1 page</param>
	/// <param name="messageToEditId">Used to edit same message when navigating through search result pages</param>
	Task ShowAccountsPageAsync(BotUser botUser, IReadOnlyList<Account> accounts, int totalAccountCount, int pageIndex, string searchQuery, int messageToEditId = 0);

	/// <summary>
	/// Serializes given account to MarkdownV2 string
	/// </summary>
	Task<string> SerializeAccountAsync(BotUser botUser, Account account,
		bool includeOutdatedTime, string extraMessage = null);
		
	InlineKeyboardButton[] GeneratePasswordKeyboard(BotUser botUser,
		GeneratePasswordCommandCode generatePasswordCommandCode,
		SetUpPasswordGeneratorCommandCode setUpPasswordGeneratorCommandCode,
		long? accountId = null);

	/// <summary>
	/// Shows account data with buttons of account updating menu
	/// </summary>
	/// <param name="botUser"></param>
	/// <param name="account"></param>
	/// <param name="messageToEditId"></param>
	/// <param name="extraMessage">An Extra message to show with account data</param>
	/// <returns></returns>
	Task ShowAccountUpdatingMenuAsync(BotUser botUser, Account account,
		int messageToEditId, string extraMessage);
		
	Task SendValidationErrorAsync(BotUser botUser, ValidationException validationException);
		
	string GetPasswordMessage(string password);
		
	InlineKeyboardMarkup GetPasswordGeneratorSettingsKeyboard(BotUser botUser, string passwordGeneratorPattern);

}