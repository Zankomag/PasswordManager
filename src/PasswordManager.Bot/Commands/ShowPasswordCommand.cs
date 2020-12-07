using System;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using PasswordManager.Bot.Types;
using MultiUserLocalization;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Core.Entities;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Application.Encryption;

namespace PasswordManager.Bot.Commands {
	public class ShowPasswordCommand : ICallbackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, BotUser user) {
			await BotService.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			int accountId = Convert.ToInt32(callbackQuery.Data.Substring(1));
			Account account;
			using (IDbConnection conn = new SQLiteConnection(BotService.Instance.connString)) {
				account = conn.QueryFirstOrDefault<Account>(
					"select Password, Encrypted from Accounts where Id = @Id and UserId = @UserId",
					new {
						UserId = callbackQuery.From.Id,
						Id = accountId});
			}
			if(account!= null) {
				//TODO
				//If user does not have hint - don't show "show hint" button, but remember that user failed his key
				//after he enters right key - send invintation to create hint
				//

				//TODO
				//ADD **GOOD CODED** DECRYPTION BY KEY (this is temporary messed working code)
				if(!account.Encrypted)
					await BotService.Instance.Client.SendTextMessageAsync(callbackQuery.From.Id,
						"`" + account.Password + "`",
						replyMarkup: new InlineKeyboardMarkup(
							InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
								CallbackCommandCode.DeleteMessage.ToStringCode())),
						parseMode: ParseMode.Markdown);
				else {
					try {
						string decryptedPassword = account.Password.Decrypt("supa dupa secret ke");
					} catch {
						await BotService.Instance.Client.SendTextMessageAsync(callbackQuery.From.Id,
						"`" + account.Password + "`",
						replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[][] {
							new InlineKeyboardButton[]{
								InlineKeyboardButton.WithCallbackData("🔁 " + Localization.GetMessage("TryAgain", user.Lang),
									CallbackCommandCode.EnterEncryptionyonKey.ToStringCode()) },
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("💡 " + Localization.GetMessage("ShowHint", user.Lang),
										CallbackCommandCode.ShowEncryptionKeyHint.ToStringCode()) }
						}),
						parseMode: ParseMode.Markdown);
					}
				}
			}
		}



	}
}
