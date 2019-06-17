using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace UPwdBot.Commands {
	public class SelectLanguageCommand : IBaseCommand {
		private InlineKeyboardMarkup inlineKeyboard;

		public SelectLanguageCommand() {
			//Set up Choosing Language Keyboard
			IList<string> icons = Localization.GetIcons();
			IList<string> langCodes = Localization.GetLangCodes();
			int langNumber = Localization.LanguageNumber;
			int colNumber = 5;
			int rowNumber= (langNumber % colNumber) == 0 ? langNumber / colNumber : langNumber / colNumber + 1;

			InlineKeyboardButton[][] buttons = new InlineKeyboardButton[rowNumber][];
			int currentLang = 0;
			for (int i = 0; i < rowNumber; i++) {
				buttons[i] = new InlineKeyboardButton[colNumber - (((i + 1) * colNumber) - langNumber)];

				for(int j = 0; j < buttons[i].Length; j++) {
					buttons[i][j] = InlineKeyboardButton
					.WithCallbackData(icons[currentLang], "L" + langCodes[currentLang]);
					currentLang++;
				}	
			}
			inlineKeyboard = new InlineKeyboardMarkup(buttons);
		}

		public async Task ExecuteAsync(Message message) {
			await BotHandler.Bot.SendTextMessageAsync(message.Chat.Id, "Choose your language.",
				replyMarkup: inlineKeyboard);
		}
	}
}
