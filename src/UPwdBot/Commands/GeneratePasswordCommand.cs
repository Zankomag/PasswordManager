using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Passwords;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class GeneratePasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (BotHandler.AssemblingAccounts.ContainsKey(user.Id)) {
				string password;
				try {
					password = user.GenPattern.GeneratePasswordByPattern();
				} catch (ArgumentException) {
					PasswordManager.SetPasswordPattern(user);
					await Bot.Instance.Client.SendTextMessageAsync(
						callbackQuery.From.Id,
						"PASSWORD PATTERN ERROR.\nPattern has been set to default.");
					password = Password.Generate();
				}

			var inlineKeyBoard = new InlineKeyboardMarkup(
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							"G"),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							"Z")});

				await BotHandler.Bot.EditMessageTextAsync(
					callbackQuery.From.Id,
					callbackQuery.Message.MessageId,
					"`" + password + "`",
					replyMarkup: inlineKeyBoard,
					parseMode: ParseMode.Markdown);


			} else {
				await BotHandler.Bot.AnswerCallbackQueryAsync(callbackQuery.Id,
					text: Localization.GetMessage("CantWithoutNewAcc", user.Lang), showAlert: true);
			}
		}
	}
}
