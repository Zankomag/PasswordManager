using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using Passwords;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Commands.Abstractions;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Core.Entities;

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
							callbackQuery.Data),
						InlineKeyboardButton.WithCallbackData("✅ " + Localization.GetMessage("Accept", user.Lang),
							callbackQuery.Data[1] switch {
								(char)GeneratePasswordCommandCode.Assembling
									=> AddAccountCommandCode.AcceptPassword.ToStringCode(),
								(char)GeneratePasswordCommandCode.Updating
									=> UpdateAccountCommandCode.AcceptPassword.ToStringCode() + callbackQuery.Data[2..],
								_ => throw new InvalidOperationException("Unknown password accepting command")
							})
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
