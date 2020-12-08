using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using MultiUserLocalization;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Application.Services.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class SelectLanguageCommand : Abstractions.BotCommand, IMessageCommand, ICallbackQueryCommand {
		private readonly InlineKeyboardMarkup inlineKeyboard;
		private readonly IUserService userService;

		public SelectLanguageCommand(IBotService botService, IUserService userService) : base(botService) {
			this.userService = userService;
		}

		public SelectLanguageCommand() {
			//Set up Choosing Language Keyboard
			IList<string> icons = Localization.GetIcons();
			IList<string> langCodes = Localization.GetLangCodes();
			int langNumber = Localization.LanguageNumber;
			int colNumber = 5;
			int rowNumber= (langNumber % colNumber) == 0 ? langNumber / colNumber : langNumber / colNumber + 1;

			InlineKeyboardButton[][] buttons = new InlineKeyboardButton[rowNumber][];
			int currentLang = 0;
			string commandCode = CallbackCommandCode.SelectLanguage.ToStringCode();
			for (int i = 0; i < rowNumber; i++) {
				buttons[i] = new InlineKeyboardButton[(langNumber - ((i + 1) * colNumber) >= 0) ? colNumber : (langNumber - (i * colNumber))];

				for(int j = 0; j < buttons[i].Length; j++) {
					buttons[i][j] = InlineKeyboardButton
					.WithCallbackData(icons[currentLang], commandCode + langCodes[currentLang]);
					currentLang++;
				}	
			}
			inlineKeyboard = new InlineKeyboardMarkup(buttons);
		}

		public async Task ExecuteAsync(Message message, BotUser user) {
			await BotHandler.Bot.SendTextMessageAsync(user.Id, Localization.GetMessage("ChooseLang", user.Lang),
				replyMarkup: inlineKeyboard);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			string langCode = callbackQuery.Data.Substring(1);
			if (!Localization.ContainsLanguage(langCode))
				langCode = Localization.DefaultLanguageCode;
			if (user == null) {
				PasswordManagerService.AddUser(callbackQuery.From.Id, langCode);
			} else {
				PasswordManagerService.SetUserLanguage(user, langCode);
			}

			await BotHandler.Bot.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
				Localization.GetMessage("LangIsSet", langCode) + "\n\n" +
				string.Format(Localization.GetMessage("Help", langCode),
				"/add", "/all", "/generator", "/language", "/cancel", "/help"), Telegram.Bot.Types.Enums.ParseMode.Markdown);
		}
	}
}
