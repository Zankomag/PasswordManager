using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Uten.Passwords;
using Uten.Localization.MultiUser;
using UPwdBot.Types.Enums;
using UPwdBot.Extensions;

namespace UPwdBot.Commands {
	public class GeneratePasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
			string password;
			try {
				password = user.GenPattern.GeneratePasswordByPattern();
			} catch (ArgumentException ex) {
				PasswordManager.SetUserPasswordPattern(user);
				await Bot.Instance.Client.SendTextMessageAsync(
					callbackQuery.From.Id,
					ex.Message + "\n" + Localization.GetMessage("DefaultPattern", user.Lang));
				password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
			}

			if (password.Length > (int)MaxAccountDataLength.Password) {
				string genPattern = user.GenPattern.Remove(6) + ((int)MaxAccountDataLength.Password).ToString();
				password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
				PasswordManager.SetUserPasswordPattern(user, genPattern);
			}

			password = password.Trim();

			var inlineKeyBoard = new InlineKeyboardMarkup(
				new InlineKeyboardButton[][] {
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							CallbackCommandCode.GeneratePassword.ToStringCode()),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							CallbackCommandCode.AcceptPassword.ToStringCode())
					},
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
							CallbackCommandCode.DeleteMessage.ToStringCode()),
					}
				});

			await BotHandler.Bot.EditMessageTextAsync(
				callbackQuery.From.Id,
				callbackQuery.Message.MessageId,
				"`" + password + "`",
				replyMarkup: inlineKeyBoard,
				parseMode: ParseMode.Markdown);
			
		}
	}
}
