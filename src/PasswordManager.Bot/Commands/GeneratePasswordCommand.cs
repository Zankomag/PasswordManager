using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using Passwords;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot.Commands {
	public class GeneratePasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			string password;
			try {
				password = user.GenPattern.GeneratePasswordByPattern();
			} catch (ArgumentException ex) {
				PasswordManagerHandler.SetUserPasswordPattern(user);
				await Bot.Instance.Client.SendTextMessageAsync(
					callbackQuery.From.Id,
					ex.Message + "\n" + Localization.GetMessage("DefaultPattern", user.Lang));
				password = Password.GeneratePasswordByPattern(Password.defaultPasswordGeneratorPattern);
			}

			if (password.Length > (int)MaxAccountDataLength.Password) {
				string genPattern = user.GenPattern.Remove(6) + ((int)MaxAccountDataLength.Password).ToString();
				password = Password.GeneratePasswordByPattern(genPattern);
				PasswordManagerHandler.SetUserPasswordPattern(user, genPattern);
			}

			password = password.Trim();

			var inlineKeyBoard = new InlineKeyboardMarkup(
				new InlineKeyboardButton[][] {
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							CallbackCommandCode.GeneratePassword.ToStringCode()),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							CallbackCommandCode.AcceptPassword.ToStringCode())
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
