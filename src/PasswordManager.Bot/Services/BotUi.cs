using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Text;
using PasswordManager.Application.Services.Abstractions;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using PasswordManager.Bot.Settings;
using PasswordManager.Common.Extensions;

namespace PasswordManager.Bot.Services; 

public class BotUi : IBotUi {
	private readonly IBot bot;
	private readonly IUserService userService;
	private readonly BotUiSettings botUiSettings;

	public BotUi(IBot bot, IUserService userService, IOptions<BotUiSettings> uiSettings) {
		this.bot = bot;
		this.userService = userService;
		this.botUiSettings = uiSettings?.Value ?? throw new ArgumentNullException(nameof(uiSettings), $"{nameof(BotUiSettings)} value is null");
	}

	//TODO Create data type for CommandCode as class tht consists from MainCommandCode
	// and Optional AdditionalCommandCode and ToString() method so that it will be validated on usage instead
	// of just using strings as 'backButtonCommandCode' below
		
	//TODO: 
	//get rid of using hardcoded emoji
	//
	//TODO: Show Expiration settings button here
	public async Task ShowAccountAsync(BotUser botUser, Account account, int? messageToEditId = null,
		string extraMessage = null, string backButtonCommandCode = null) {

		if (account != null) {
			string message = await SerializeAccountAsync(botUser, account, true, extraMessage);

			//TODO: use methods from new localization system and emoji
			var keyboard = new List<List<InlineKeyboardButton>> {
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData(
						"🔑 " + Localization.GetMessage("Password", botUser.Lang),
						CallbackQueryCommandCode.ShowPassword.ToStringCode() + account.Id)},
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData(
						"🛡 " + Localization.GetMessage("UpdatePassword", botUser.Lang),
						UpdateAccountCommandCode.Password.ToStringCode(account.Id))},
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData(
						"✏️ " + Localization.GetMessage("UpdateAcc", botUser.Lang),
						UpdateAccountCommandCode.SelectUpdateType.ToStringCode(account.Id)) },
				new List<InlineKeyboardButton> {
					InlineKeyboardButton.WithCallbackData(
						"🗑 " + Localization.GetMessage("DeleteAcc", botUser.Lang),
						DeleteAccountCommandCode.AskForDeletion.ToStringCode(account.Id)) },
			};

			var deleteMessageButton = InlineKeyboardButton.WithCallbackData(
				"🗑 " + Localization.GetMessage("DeleteMsg", botUser.Lang),
				CallbackQueryCommandCode.DeleteMessage.ToStringCode());

			var lastButtonRow = new List<InlineKeyboardButton>();

			if(backButtonCommandCode != null) {
				lastButtonRow.Add(InlineKeyboardButton.WithCallbackData(
					"⬅️ " + Localization.GetMessage("Back", botUser.Lang),
					backButtonCommandCode));
			}
			lastButtonRow.Add(deleteMessageButton);

			keyboard.Add(lastButtonRow);

			var keyboardMarkup = new InlineKeyboardMarkup(keyboard);

			if (messageToEditId == null) {
				await bot.Client.SendTextMessageAsync(botUser.Id, message,
					replyMarkup: keyboardMarkup, disableWebPagePreview: true,
					parseMode: ParseMode.Markdown);
			} else {
				await bot.Client.EditMessageTextAsync(botUser.Id, messageToEditId.Value, message,
					replyMarkup: keyboardMarkup, disableWebPagePreview: true,
					parseMode: ParseMode.Markdown);
			}
		}
	}

	public async Task ShowAccountsPageAsync(BotUser botUser, IReadOnlyList<Account> accounts, int totalAccountCount, int pageIndex, string searchQuery, int messageToEditId = 0) {
		int pageCount = totalAccountCount.PageCount(botUiSettings.PageSize);
		bool isSinglePage = pageCount == 1;
		
		//todo fucking refactor this to Format string!
		string message = Localization.GetMessage("Page", botUser.Lang) + " " + (pageIndex + 1)
			+ "/" + pageCount + "\n";
		
		message += GetAccountsPageMessage(accounts, out InlineKeyboardButton[][] keyboard, isSinglePage, botUser.Lang);

		//todo refactor and extract to method
		//todo add regions for different processes for example region for showing pages of account and related methods
		if(!isSinglePage) {
			//First page
			if(pageIndex == 0) {
				keyboard[^1] = new[] {
					GetPageNavigationButton(true, pageIndex, searchQuery, botUser.Lang)
				};
			}

			//Last page
			else if(pageIndex == pageCount - 1) {
				keyboard[^1] = new[] {
					GetPageNavigationButton(false, pageIndex, searchQuery, botUser.Lang)
				};
			}

			//Middle page
			else {
				keyboard[^1] = new[] {
					GetPageNavigationButton(false, pageIndex, searchQuery, botUser.Lang),
					GetPageNavigationButton(true, pageIndex, searchQuery, botUser.Lang)
				};
			}
		}

		if(messageToEditId == 0) {
			await bot.Client.SendTextMessageAsync(botUser.Id, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		} else {
			await bot.Client.EditMessageTextAsync(botUser.Id, messageToEditId, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		}
	}
	
	private string GetAccountsPageMessage(IReadOnlyList<Account> accounts,
		out InlineKeyboardButton[][] keyboard,
		bool singlePage, string langCode) {

		StringBuilder messageSb = new StringBuilder();
		
		//If page is single we need only buttons for selecting accounts, 
		//otherwise we also need one more row for navigation buttons
		keyboard = new InlineKeyboardButton[singlePage ? accounts.Count : accounts.Count + 1][];

		for(int i = 0; i < accounts.Count; i++) {
			if(i != 0)
				messageSb.Append(botUiSettings.AccountSeparator);
			messageSb.AppendLine().Append(accounts[i].AccountName);
			if(accounts[i].Link != null) 
				messageSb.AppendLine().Append(accounts[i].Link);
			messageSb.AppendLine();
			messageSb.Append(Localization.GetMessage("Login", langCode))
				.Append(": ").Append(accounts[i].Login);
			keyboard[i] = new[] {
				InlineKeyboardButton.WithCallbackData((i + 1) + "⃣ " + accounts[i].AccountName, 
					CallbackQueryCommandCode.ShowAccount.ToStringCode() + accounts[i].Id)
			};
		}
		return messageSb.ToString();
	}
	
	/// <summary>
	/// Creates a button to navigate through accounts pages (Next or Previous page)
	/// </summary>
	/// <param name="next"></param>
	/// <param name="page"></param>
	/// <param name="searchQuery">Needs for placing it in navigation button</param>
	/// <param name="langCode"></param>
	/// <returns></returns>
	//todo change bool next to enum for clearness
	private InlineKeyboardButton GetPageNavigationButton(bool next, int page, string searchQuery, string langCode) {
		if(searchQuery != null) {
			//todo may be if we have limits on search in telegram, we can save search in cache instead of button
			//and save only search id in button
			
			//todo verify if statement below is true
			//Telegram inline button accepts only 64 bytes of data. UTF-16 string has 2 bytes per char.
			//So, search string can be no more than 64 - (4(>(2 bytes) + . (2 bytes)) + page.ToString().Length*2) chars
			int maxLength = (64 - (4 /*(> + .)*/ + page.ToString().Length * 2));
			searchQuery = searchQuery.Length <= maxLength ? searchQuery : searchQuery[..maxLength];
		}
		//todo refactor command code building
		return InlineKeyboardButton.WithCallbackData(
			next ? "▶️ " + Localization.GetMessage("Next", langCode) : "◀️ " + Localization.GetMessage("Prev", langCode),
			CallbackQueryCommandCode.Search.ToStringCode() + (next ? (page + 1).ToString() : (page - 1).ToString()) +
			"." + searchQuery);
	}

	/// <summary>
	/// Serializes given account to MarkdownV2 string
	/// </summary>
	public async Task<string> SerializeAccountAsync(BotUser botUser, Account account,
		bool includeOutdatedTime, string extraMessage = null) {
		StringBuilder messageBuilder = extraMessage != null 
			? new StringBuilder(extraMessage).Append("\n\n")
			: new StringBuilder();

		if (account.Link != null) {
			messageBuilder.Append('[')
				.Append(account.AccountName.EscapeMarkdownV2Chars()).Append("](")
				.Append(account.Link.EscapeInlineLinkMarkdownV2Chars()).Append(')');
		} else {
			messageBuilder.Append(account.AccountName.EscapeMarkdownV2Chars());
		}

		if (account.Note != null)
			messageBuilder.Append("\n\n_").Append(account.Note.EscapeMarkdownV2Chars()).AppendLine("_\n");

		messageBuilder.Append(Localization.GetMessage("Login", botUser.Lang)).Append(": `")
			.Append(account.Login.EscapeCodeBlockMarkdownV2Chars()).Append('`');

		if (includeOutdatedTime) {
			TimeSpan? outdatedTime = null;
			if (account.OutdatedTime == null) {
				User user = await userService.GetUserOutdatedTimeAsync(botUser.Id);
				if (user.OutdatedTime != null) {
					outdatedTime = user.OutdatedTime.Value;
				}
			} else {
				outdatedTime = account.OutdatedTime.Value;
			}
			if(outdatedTime != null) {
				int changePasswordInDays = (account.PasswordUpdatedDate + outdatedTime.Value - DateTime.UtcNow.Date).Days;
				//TODO: use methods from new localization system and emoji
				string changePasswordInDaysString = String.Format(Localization.GetMessage("WhenChangePassword", botUser.Lang),
					changePasswordInDays > 0 
						? String.Format(Localization.GetMessage("WhenDays", botUser.Lang), outdatedTime)
						: String.Concat("⚠️ ",
							changePasswordInDays == 0 
								? Localization.GetMessage("WhenToday", botUser.Lang)
								: String.Concat('*', Localization.GetMessage("WhenNow", botUser.Lang), '*'),
							"⚠️ "));

				messageBuilder.Append("\n\n").Append(changePasswordInDaysString);
			}
		}

		return messageBuilder.ToString();	
	}

	public InlineKeyboardButton[] GeneratePasswordKeyboard(BotUser botUser,
		GeneratePasswordCommandCode generatePasswordCommandCode,
		SetUpPasswordGeneratorCommandCode setUpPasswordGeneratorCommandCode,
		long? accountId = null) 
		=> new InlineKeyboardButton[] {
			InlineKeyboardButton.WithCallbackData(
				"🌋 " + Localization.GetMessage("Generate", botUser.Lang),
				generatePasswordCommandCode.ToStringCode()),
			InlineKeyboardButton.WithCallbackData(
				"🛠 " + Localization.GetMessage("AdjustGenerator", botUser.Lang),
				setUpPasswordGeneratorCommandCode.ToStringCode() + accountId)};


	public async Task ShowAccountUpdatingMenuAsync(BotUser botUser, Account account,
		int messageToEditId, string extraMessage) {
		var keyboardMarkup = new InlineKeyboardMarkup(
			new InlineKeyboardButton[][] {
				new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData(
						"📝 " + Localization.GetMessage("AccountName", botUser.Lang),
						UpdateAccountCommandCode.AccountName.ToStringCode(account.Id))},
				new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData(
						"🔗 " + Localization.GetMessage("Link", botUser.Lang),
						UpdateAccountCommandCode.Link.ToStringCode(account.Id)),
					InlineKeyboardButton.WithCallbackData(
						"🗒 " + Localization.GetMessage("Note", botUser.Lang),
						UpdateAccountCommandCode.Note.ToStringCode(account.Id))},
				new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData(
						"📇 " + Localization.GetMessage("Login", botUser.Lang),
						UpdateAccountCommandCode.Login.ToStringCode(account.Id)) },
				new InlineKeyboardButton[] {
					InlineKeyboardButton.WithCallbackData(
						"⬅️ " + Localization.GetMessage("Back", botUser.Lang),
						CallbackQueryCommandCode.ShowAccount.ToStringCode() + account.Id)}
			});

		extraMessage = await SerializeAccountAsync(botUser, account, false, extraMessage);

		await bot.Client.EditMessageTextAsync(botUser.Id, messageToEditId,
			extraMessage,
			replyMarkup: keyboardMarkup,
			disableWebPagePreview: true);
	}

	public async Task SendValidationErrorAsync(BotUser botUser, ValidationException validationException) {
		//TODO:
		//Change to good translated message
		await bot.Client.SendTextMessageAsync(botUser.Id, validationException.Message);
	}

	public string GetPasswordMessage(string password)
		=> new StringBuilder("`")
			.Append(password.EscapeCodeBlockMarkdownV2Chars())
			.Append('`')
			.ToString();
		
	public InlineKeyboardMarkup GetPasswordGeneratorSettingsKeyboard(BotUser botUser, string passwordGeneratorPattern) {
		//todo get all this data from PasswordGenerator library in future :)
		if (passwordGeneratorPattern == null || passwordGeneratorPattern.Length < 7)
			throw new ArgumentException("Generator pattern must contain all  6 params and length");
		bool containsLowerChars = passwordGeneratorPattern[0] != '0',
			containsUpperChars = passwordGeneratorPattern[1] != '0',
			containsDigits = passwordGeneratorPattern[2] != '0',
			containsSpecialChars = passwordGeneratorPattern[3] != '0',
			firstCharIsLetter = passwordGeneratorPattern[4] != '0',
			containsSpace = passwordGeneratorPattern[5] != '0';


		return new InlineKeyboardMarkup(
			new[] {
				new[] {
					InlineKeyboardButton.WithCallbackData(
						containsLowerChars.ToEmojiString(true) +
						Localization.GetMessage("LowerChars", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.ContainsLowerChars.ToStringCode() +
						containsLowerChars.ToReverseZeroOneString()
					)
				},
				new[] {
					InlineKeyboardButton.WithCallbackData(
						containsUpperChars.ToEmojiString(true) +
						Localization.GetMessage("UpperChars", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.ContainsUpperChars.ToStringCode() +
						containsUpperChars.ToReverseZeroOneString()
					)
				},
				new[] {
					InlineKeyboardButton.WithCallbackData(
						containsDigits.ToEmojiString(true) +
						Localization.GetMessage("Digits", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.ContainsDigits.ToStringCode() +
						containsDigits.ToReverseZeroOneString()
					),
					InlineKeyboardButton.WithCallbackData(
						containsSpace.ToEmojiString(true) +
						Localization.GetMessage("Spaces", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.ContainsSpace.ToStringCode() +
						containsSpace.ToReverseZeroOneString()
					)
				},
				new[] {
					InlineKeyboardButton.WithCallbackData(
						firstCharIsLetter.ToEmojiString(true) +
						Localization.GetMessage("FirstChar", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.FirstCharIsLetter.ToStringCode() +
						firstCharIsLetter.ToReverseZeroOneString()
					)
				},
				new[] {
					InlineKeyboardButton.WithCallbackData(
						containsSpecialChars.ToEmojiString(true) +
						Localization.GetMessage("SpecialChars", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.ContainsSpecialChars.ToStringCode() +
						containsSpecialChars.ToReverseZeroOneString()
					),
					InlineKeyboardButton.WithCallbackData(
						"⛓️ " +
						Localization.GetMessage("Length", botUser.Lang) + " " + passwordGeneratorPattern[6..],
						SetUpPasswordGeneratorCommandCode.Length.ToStringCode()
					)
				},
				new[] {
					InlineKeyboardButton.WithCallbackData(
						"🌋 " + Localization.GetMessage("Generate", botUser.Lang),
						SetUpPasswordGeneratorCommandCode.Generate.ToStringCode()
					)
				}
			}
		);
	}

	//new InlineKeyboardButton[] {
	//	InlineKeyboardButton.WithCallbackData(
	//		"🗑 " + Localization.GetMessage("DeleteLink", botUser.Lang),
	//		UpdateAccountCommandCode.DeleteLink.ToStringCode(accountId)) },
	//new InlineKeyboardButton[] {
	//	InlineKeyboardButton.WithCallbackData(
	//		"🗑 " + Localization.GetMessage("DeleteNote", botUser.Lang),
	//		UpdateAccountCommandCode.DeleteNote.ToStringCode(accountId)) },




	//TODO: Expiration settings 
	//Show PasswordUpdatedDate (Password updated : 75 days ago/today. (17.03.2019))
	//Show OutdatedTime (It is considered outdated 365 days after it's been updated (in 40 days/today/*NOW*)
	//and should be changed after this period)
	//Explain when it takes outdated time 
	//([This password uses global validity period from your /settings.\n]
	//If you want to change validity period for this password, click the button below.)
	//*button* (Change validity period)

}