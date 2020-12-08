using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using MultiUserLocalization;
using PasswordManager.Bot.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Encryption;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowPasswordCommand : Abstractions.BotCommand, ICallbackQueryCommand, IActionCommand {
		private readonly IAccountService accountService;

		public ShowPasswordCommand(IBotService botService, IAccountService accountService) : base(botService) {
			this.accountService = accountService;
		}

		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			int accountId;
			try {
				accountId = Convert.ToInt32(callbackQuery.Data[1..]);
			} catch(Exception exception) {
				//TODO: Log exception
				throw;
			}
			Account account = await accountService.GetPasswordAsync(user.Id, accountId);
			if(account!= null) {

				
				if(!account.Encrypted)
					await botService.Client.SendTextMessageAsync(callbackQuery.From.Id,
						"`" + account.Password + "`",
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
								CallbackQueryCommandCode.DeleteMessage.ToStringCode())),
						parseMode: ParseMode.Markdown);
				else {
					try {
						//TODO
						//ADD **GOOD CODED** DECRYPTION BY KEY (this is temporary messed working code)
						string decryptedPassword = account.Password.Decrypt("supa dupa secret ke");
					} catch {
						//TODO
						//If user does not have hint - don't show "show hint" button, but remember that user failed his key
						//after he enters right key - send invintation to create hint
						await botService.Client.SendTextMessageAsync(callbackQuery.From.Id,
						"`" + account.Password + "`",
						replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
							new InlineKeyboardButton[]{
								InlineKeyboardButton.WithCallbackData("🔁 " + Localization.GetMessage("TryAgain", user.Lang),
									CallbackQueryCommandCode.EnterEncryptionyonKey.ToStringCode()) },
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("💡 " + Localization.GetMessage("ShowHint", user.Lang),
										CallbackQueryCommandCode.ShowEncryptionKeyHint.ToStringCode()) }
						}),
						parseMode: ParseMode.Markdown);
					}
				}
			}
		}

		async Task IActionCommand.ExecuteAsync(Message message, BotUser user) => _NONIMPLEMENTED_ throw new NotImplementedException();
	}
}
