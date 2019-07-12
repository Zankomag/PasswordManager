using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Uten.Encryption;
using UPwdBot.Types;
using Uten.Localization.MultiUser;

namespace UPwdBot.Commands {
	public class AddAccountCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, string langCode) {
			if (message.Text.StartsWith("/add") && message.Text.Length > 5) {
				if (BotHandler.AssemblingAccounts.ContainsKey(message.From.Id))
					BotHandler.AssemblingAccounts.Remove(message.From.Id);
				await AssembleAccountAsync(message, langCode);

			}
			else if (message.Text == "/add") {
				BotHandler.AssemblingAccounts[message.From.Id] = new Account() { UserId = message.From.Id };
				await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
					"📝 " + Localization.GetMessage("AddAccount", langCode));
			}
			//then user is already assembling account, so CONTINUE assembling it
			else {
				Account account = BotHandler.AssemblingAccounts[message.From.Id];
				string data = !message.Text.Contains('\n') ? message.Text :
					message.Text.Substring(0, message.Text.IndexOf('\n'));
				if (account.AccountName == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxAccountNameLength,"AccName",
							message.From.Id, langCode)) {
						return;
					}

					account.AccountName = data;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await AddLinkPrompt(message.Chat.Id, account.AccountName, langCode);

				} else if(!account.SkipLink && account.Link == null){
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxLinkLength, "Link",
							message.From.Id, langCode)) {
						return;
					}

					account.Link = data.BuildLink();
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"📇 " + Localization.GetMessage("AddLogin", langCode));

				} else if (account.Login == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxLoginLength, "Login",
							message.From.Id, langCode)) {
						return;
					}
					
					account.Login = data;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔐 " + String.Format(Localization.GetMessage("AddPassword", langCode), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", langCode),
							"G")));

				} else if (account.Password == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxPasswordLength, "Password",
							message.From.Id, langCode)) {
						return;
					}

					account.Password = Encryption.Encrypt(data);
					await SaveToDBAsync(account, langCode);
					BotHandler.AssemblingAccounts.Remove(message.From.Id);
				}
			}
		}

		private async Task AssembleAccountAsync(Message message, string langCode) {
			string[] accountData = message.Text.Remove(0, 5).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (await IsLengthExceeded(accountData[0].Length,
							Account.maxAccountNameLength, "AccName",
							message.From.Id, langCode)) {
				return;
			}
			Account account = new Account {
				AccountName = accountData[0],
				UserId = message.From.Id
			};

			if (accountData.Length > 3) {
				if (await IsLengthExceeded(accountData[1].Length, Account.maxLinkLength, "Link", message.From.Id, langCode) ||
					await IsLengthExceeded(accountData[2].Length, Account.maxLoginLength, "Login", message.From.Id, langCode) ||
					await IsLengthExceeded(accountData[3].Length, Account.maxPasswordLength, "Password", message.From.Id, langCode)) {

					return;
				}
				account.Link = accountData[1].BuildLink();
				account.Login = accountData[2].Trim();
				account.Password = Encryption.Encrypt(accountData[3].Trim());
				await SaveToDBAsync(account, langCode);
			} else if(accountData.Length == 3) {
				if (accountData[1].Length > Account.maxLoginLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Login", langCode), Account.maxLoginLength);
					return;
				}
				else if (accountData[2].Length > Account.maxPasswordLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Password", langCode), Account.maxPasswordLength);
					return;
				}
				account.Login = accountData[1];
				account.Password = Encryption.Encrypt(accountData[2]);
				await SaveToDBAsync(account, langCode);
			} else if(accountData.Length == 2) {
				if (accountData[1].Length > Account.maxLoginLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Login", langCode), Account.maxLoginLength);
					return;
				}
				account.Login = accountData[1].Trim();
				BotHandler.AssemblingAccounts[message.From.Id] = account;
				await AddLinkPrompt(message.Chat.Id, account.AccountName, langCode);
			} else {
				BotHandler.AssemblingAccounts[message.From.Id] = account;
				await AddLinkPrompt(message.Chat.Id, account.AccountName, langCode);
			}

		}

		private async Task<bool> IsLengthExceeded(int paramLength, int maxParamLength, string paramName, int userId, string langCode) {
			if (paramLength > maxParamLength) {
				await ReportExceededLength(userId, langCode,
					Localization.GetMessage("Login", langCode), Account.maxLoginLength);
				return true;
			}
			return false;
		}

		private static async Task SaveToDBAsync(Account account, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account.Id = (long)conn.ExecuteScalar("Insert into Account (UserId, AccountName, Link, Login, Password) " +
					"values (@UserId, @AccountName, @Link, @Login, @Password);" +
					"select last_insert_rowid()",
					account);
			}

			await PasswordManager.ShowAccount(account.UserId, account, langCode, 
				extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", langCode), account.AccountName));
		}

		private static async Task AddLinkPrompt(ChatId chatId, string accountName, string langCode) {
			var inlineKeyBoard = new InlineKeyboardMarkup(
				new InlineKeyboardButton[][] {
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("🔗 " + accountName.AutoLink(), "A")
							},
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData(
									"⏩ " + Localization.GetMessage("Skip",langCode), "S")
							}
				}
			);
			await BotHandler.Bot.SendTextMessageAsync(chatId,
				"🔗 " + String.Format(Localization.GetMessage("AddLink", langCode), "/help"),
				replyMarkup: inlineKeyBoard);
		}

		public static async Task UpdateCallBackMessageAsync(ChatId chatId, int MessageId, Account account, string langCode) {

			if (account.AccountName == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, MessageId,
					"📝 " + Localization.GetMessage("AddAccount", langCode));
			}
			else if (!account.SkipLink && account.Link == null) {
				await AddLinkPrompt(chatId, account.AccountName, langCode);
			}
			else if (account.Login == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, MessageId,
					"📇 " + Localization.GetMessage("AddLogin", langCode));
			}
			else if (account.Password == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, MessageId,
					"🔐 " + String.Format(Localization.GetMessage("AddPassword", langCode), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", langCode),
							"G")));
			}
			else {
				BotHandler.AssemblingAccounts.Remove(account.UserId);
				await SaveToDBAsync(account, langCode);
			}
		}

		public static async Task ReportExceededLength(ChatId chatid, string langCode, string paramName, int maxLength) {
			await Bot.Instance.Client.SendTextMessageAsync(chatid,
				String.Format(Localization.GetMessage("MaxLength", langCode), paramName, maxLength));
		}

	}
}
