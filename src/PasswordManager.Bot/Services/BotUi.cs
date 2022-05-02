﻿using MultiUserLocalization;
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

namespace PasswordManager.Bot.Services; 

public class BotUi : IBotUi {
	private readonly IBot bot;
	private readonly IUserService userService;

	public BotUi(IBot bot, IUserService userService) {
		this.bot = bot;
		this.userService = userService;
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

	public async Task ShowAccountsPageAsync(BotUser botUser, IList<Account> accounts, int totalAccountCount, int page, int pageSize) {
		string message = Localization.GetMessage("Page", langCode) + " " + (page + 1) + "/" + pageCount + "\n";
		message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, false, langCode, message);

		//This is first page
		if(page == 0) {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(true, page, accountName, langCode)
			};
		}
		//This is last page
		else if(page == pageCount - 1) {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(false, page, accountName, langCode)
			};
		}
		//This is middle page
		else {
			keyboard[keyboard.Length - 1] = new InlineKeyboardButton[] {
				GetPageButton(false, page, accountName, langCode),
				GetPageButton(true, page, accountName, langCode)
			};
		}

		if(messageToEditId == 0) {
			await Bot.Client.SendTextMessageAsync(userId, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		} else {
			await Bot.Client.EditMessageTextAsync(userId, messageToEditId, message,
				replyMarkup: new InlineKeyboardMarkup(keyboard),
				disableWebPagePreview: true);
		}
	}
	
	//todo utilize this in method to show pages above to that that method will work 
	//both for single and multiple pages
	private static async Task ShowSinglePage(long userId, string accountName, string langCode) {

		string message = GetPageMessage(accounts, out InlineKeyboardButton[][] keyboard, true, langCode);

		await Bot.Client.SendTextMessageAsync(userId, message,
			replyMarkup: new InlineKeyboardMarkup(keyboard),
			disableWebPagePreview: true);
	}

	//todo utilize this in method to show pages above to that that method will work 
	//both for single and multiple pages
	private static string GetPageMessage(List<Account> accounts,
		out InlineKeyboardButton[][] keyboard,
		bool singlePage, string langCode, string message = null) {

		if(message == null)
			message = "";

		keyboard = new InlineKeyboardButton[singlePage ? accounts.Count : accounts.Count + 1][];

		for(int i = 0; i < accounts.Count; i++) {
			if(i != 0)
				message += AccountSeparator;
			message += "\n" + accounts[i].AccountName;
			if(accounts[i].Link != null)
				message += "\n" + accounts[i].Link;
			message += "\n" + Localization.GetMessage("Login", langCode) + ": " + accounts[i].Login;
			keyboard[i] = new InlineKeyboardButton[] {
				InlineKeyboardButton.WithCallbackData((i + 1) + "⃣ " + accounts[i].AccountName,
					CallbackCommandCode.ShowAccount.ToStringCode() + accounts[i].Id.ToString())
			};
		}
		return message;
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