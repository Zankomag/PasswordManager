using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using PasswordManager.Types;
using Uten.Localization.MultiUser;
using PasswordManager.Types.Enums;
using PasswordManager.Extensions;

namespace PasswordManager.Commands {
	public class ShowPasswordCommand : ICallBackQueryCommand {
		public async Task ExecuteAsync(CallbackQuery callbackQuery, Types.User user) {
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
				await Bot.Instance.Client.SendTextMessageAsync(callbackQuery.From.Id,
					"`" + Uten.Encryption.Encryption.Decrypt(account.Password) + "`",
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🗑 " + Localization.GetMessage("DeleteMsg", user.Lang),
							CallbackCommandCode.DeleteMessage.ToStringCode())),
					parseMode: ParseMode.Markdown);
			}
		}



	}
}
