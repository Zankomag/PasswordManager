using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace UPwdBot.Commands {
	public class SelectLanguageCommand : IMessageCommand, ICallBackQueryCommand {
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

		public async Task ExecuteAsync(Message message, string langCode) {
			await BotHandler.Bot.SendTextMessageAsync(message.Chat.Id, Localization.GetMessage("ChooseLang", langCode),
				replyMarkup: inlineKeyboard);
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			string langCode = callbackQuery.Data.Substring(1);
			if (!Localization.HasLanguage(langCode))
				langCode = Localization.defaultLanguage;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				if (user == null) {
					//New user
					conn.Execute("Insert into User (Id, Lang) values (@Id, @Lang)",
						new { callbackQuery.From.Id, Lang = langCode });
				} else {
					conn.Execute("update User set Lang = @Lang where Id = @Id",
						new { Lang = langCode, callbackQuery.From.Id });
				}
			}
			await BotHandler.Bot.EditMessageTextAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId,
				Localization.GetMessage("LangIsSet", langCode) + "\n\n" +
				Localization.GetMessage("Help", langCode));
		}
	}
}
