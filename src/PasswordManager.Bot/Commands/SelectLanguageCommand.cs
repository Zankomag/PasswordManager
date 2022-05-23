using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using Telegram.Bot;

namespace PasswordManager.Bot.Commands; 

public class SelectLanguageCommand : Abstractions.BotCommand, IMessageCommand, ICallbackQueryCommand {
	private readonly IUserService userService;

	public SelectLanguageCommand(IBot bot, IUserService userService) : base(bot) {
		this.userService = userService;
			
	}

	private InlineKeyboardMarkup GetLanguagesKeyboard() {
		IList<string> icons = Localization.GetIcons();
		IList<string> langCodes = Localization.GetLangCodes();
		int langNumber = Localization.LanguageNumber;
		int colNumber = 5;
		int rowNumber = (langNumber % colNumber) == 0 ? langNumber / colNumber : langNumber / colNumber + 1;

		InlineKeyboardButton[][] buttons = new InlineKeyboardButton[rowNumber][];
		int currentLang = 0;
		string selectLangCommandCode = CallbackQueryCommandCode.SelectLanguage.ToStringCode();
		for (int i = 0; i < rowNumber; i++) {
			buttons[i] = new InlineKeyboardButton[(langNumber - ((i + 1) * colNumber) >= 0) ? colNumber : (langNumber - (i * colNumber))];

			for (int j = 0; j < buttons[i].Length; j++) {
				buttons[i][j] = InlineKeyboardButton
					.WithCallbackData(icons[currentLang], selectLangCommandCode + langCodes[currentLang]);
				currentLang++;
			}
		}
		return new InlineKeyboardMarkup(buttons);
	}

	async Task IMessageCommand.ExecuteAsync(Message message, BotUser botUser) {
		await Bot.Client.SendTextMessageAsync(botUser.Id,
			Localization.GetMessage("ChooseLang", botUser.Lang),
			replyMarkup: GetLanguagesKeyboard());
	}

	async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser botUser) {
		//If botUser pressed on "Change lang" button in settings, bot shows invitation to select language
		if (callbackQuery.Data[1] == (char)SelectLanguageCommandCode.SelectLanguage){
			await Bot.Client.EditMessageTextAsync(botUser.Id,
				callbackQuery.Message.MessageId,
				Localization.GetMessage("ChooseLang", botUser.Lang),
				replyMarkup: GetLanguagesKeyboard());
			return;
		}

		string langCode = callbackQuery.Data[1..];
		if (!Localization.ContainsLanguage(langCode))
			langCode = Localization.DefaultLanguageCode;
		botUser.Lang = langCode;
		await userService.UpdateLanguage(botUser.Id, langCode);

		//TODO:
		//Create public static method in HelpCommand that returns help message
		await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
			Localization.GetMessage("LangIsSet", botUser.Lang) + "\n\n" +
			String.Format(Localization.GetMessage("Help", botUser.Lang),
				"/add", "/all", "/generator", "/language", "/cancel", "/help"), Telegram.Bot.Types.Enums.ParseMode.Markdown);
	}
}