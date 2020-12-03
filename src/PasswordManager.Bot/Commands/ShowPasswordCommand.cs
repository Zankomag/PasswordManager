using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
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
using PasswordManager.Bot.Commands.Abstractions;

namespace PasswordManager.Bot.Commands {
	public class ShowPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, User user) {
			await Bot.Instance.Client.AnswerCallbackQueryAsync(callbackQuery.Id);
			int accountId = Convert.ToInt32(callbackQuery.Data.Substring(1));
			Account account;
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account = conn.QueryFirstOrDefault<Account>(
					"select Password from Account where Id = @Id and UserId = @UserId",
					new {
						UserId = callbackQuery.From.Id,
						Id = accountId});
			}
			if(account!= null) {
				//TODO
				//ADD DECRYPTION BY KEY
				await Bot.Instance.Client.SendTextMessageAsync(callbackQuery.From.Id,
					"`" + account.Password + "`",
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
							CallbackCommandCode.DeleteMessage.ToStringCode())),
					parseMode: ParseMode.Markdown);
			}
		}



	}
}
