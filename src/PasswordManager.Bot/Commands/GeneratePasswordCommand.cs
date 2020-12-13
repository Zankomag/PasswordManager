using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using Passwords;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;

namespace PasswordManager.Bot.Commands {
	public class GeneratePasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand {
		private readonly IAccountService accountService;
		private readonly IUserService userService;

		public GeneratePasswordCommand(IBot bot,
			IAccountService accountService, 
			IUserService userService)
			: base(bot) {

			this.accountService = accountService;
			this.userService = userService;
		}
		async Task ICallbackQueryCommand.ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			string password;
			try {
				password = user.GenPattern.GeneratePasswordByPattern();
			} catch (ArgumentException ex) {
				.SetUserPasswordPattern(user);
				await bot.Client.SendTextMessageAsync(
					callbackQuery.From.Id,
					ex.Message + "\n" + Localization.GetMessage("DefaultPattern", user.Lang));
				password = Password.GeneratePasswordByPattern(Password.DefaultPasswordGeneratorPattern);
			}

			if (password.Length > (int)MaxAccountDataLength.Password) {
				string genPattern = user.GenPattern.Remove(6) + ((int)MaxAccountDataLength.Password).ToString();
				password = Password.GeneratePasswordByPattern(genPattern);
				.SetUserPasswordPattern(user, genPattern);
			}

			password = password.Trim();

			var inlineKeyBoard = new InlineKeyboardMarkup(
				new InlineKeyboardButton[][] {
					new InlineKeyboardButton[] {
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("TryAgain", user.Lang),
							CallbackQueryCommandCode.GeneratePassword.ToStringCode()),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							AddAccountCommandCode.AcceptPassword.ToStringCode())
					}
				});

			await BotHandler.Bot.EditMessageTextAsync(
				callbackQuery.From.Id,
				callbackQuery.Message.MessageId,
				"`" + password + "`",
				replyMarkup: inlineKeyBoard,
				parseMode: ParseMode.Markdown);
			
		}

		//Moved from 
		public static InlineKeyboardMarkup GeneratePasswordButtonMarkup(string langCode) {
			return new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", langCode),
							CallbackQueryCommandCode.GeneratePassword.ToStringCode()));
		}
	}
}
