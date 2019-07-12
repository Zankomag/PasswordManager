using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Passwords;
using Uten.Localization.MultiUser;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class GeneratePasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			if (BotHandler.AssemblingAccounts.ContainsKey(user.Id)) {
				string password;
				try {
					password = user.GenPattern.GeneratePasswordByPattern();
				} catch (ArgumentException ex) {
					PasswordManager.SetPasswordPattern(user);
					await Bot.Instance.Client.SendTextMessageAsync(
						callbackQuery.From.Id,
						ex.Message + "\n" + Localization.GetMessage("DefaultPattern", user.Lang));
					password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
				}

				if (password.Length > Account.maxPasswordLength) {
					string genPattern = user.GenPattern.Remove(6) + Account.maxPasswordLength;
					password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
					PasswordManager.SetPasswordPattern(user, genPattern);
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
