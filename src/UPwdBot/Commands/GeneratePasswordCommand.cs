using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using UPwdBot.Types;
using Uten.Passwords;

namespace UPwdBot.Commands {
	public class GeneratePasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (BotHandler.AssemblingAccounts.TryGetValue(user.Id, out Account account)) {
				string password = user.GenPattern.GeneratePasswordByPattern();

				var inlineKeyBoard = new InlineKeyboardMarkup(
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							"G"),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							"Z")});

				await BotHandler.Bot.EditMessageTextAsync(
					callbackQuery.Message.Chat.Id,
					callbackQuery.Message.MessageId,
					password, replyMarkup: inlineKeyBoard);


			} else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
