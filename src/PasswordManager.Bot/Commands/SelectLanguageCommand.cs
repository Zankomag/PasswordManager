using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands {
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

		async Task IMessageCommand.ExecuteAsync(Message message, BotUser user) {
			await Bot.Client.SendTextMessageAsync(user.Id,
				Localization.GetMessage("ChooseLang", user.Lang),
				replyMarkup: GetLanguagesKeyboard());
		}

		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			//If user pressed on "Change lang" button in settings, bot shows invitation to select language
			if (callbackQuery.Data[1] == (char)SelectLanguageCommandCode.SelectLanguage){
				await Bot.Client.EditMessageTextAsync(user.Id,
					callbackQuery.Message.MessageId,
					Localization.GetMessage("ChooseLang", user.Lang),
					replyMarkup: GetLanguagesKeyboard());
				return;
			}

			string langCode = callbackQuery.Data[1..];
			if (!Localization.ContainsLanguage(langCode))
				langCode = Localization.DefaultLanguageCode;
			user.Lang = langCode;
			await userService.UpdateLanguage(user.Id, langCode);

			//TODO:
			//Create public static method in HelpCommand that returns help message
			await Bot.Client.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
				Localization.GetMessage("LangIsSet", user.Lang) + "\n\n" +
				String.Format(Localization.GetMessage("Help", user.Lang),
				"/add", "/all", "/generator", "/language", "/cancel", "/help"), Telegram.Bot.Types.Enums.ParseMode.Markdown);
		}
	}
}
