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
		public async Task ExecuteAsync(Message message, Types.User user) {
			if (message.Text.StartsWith("/add") && message.Text.Length > 5) {
				PasswordManager.AssemblingAccounts.Remove(message.From.Id);
				await AssembleAccountAsync(message, user);
			}
			else if (message.Text == "/add") {
				PasswordManager.AssemblingAccounts[message.From.Id] = new Account() { UserId = message.From.Id };
				PasswordManager.SetUserAction(user, Actions.Assemble);
				await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
					"📝 " + Localization.GetMessage("AddAccount", user.Lang));
			}
			//then user is already assembling account, so CONTINUE assembling it
			else {
				Account account = PasswordManager.AssemblingAccounts[message.From.Id];
				string data = !message.Text.Contains('\n') ? message.Text :
					message.Text.Substring(0, message.Text.IndexOf('\n'));
				if (account.AccountName == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxAccountNameLength,"AccountName",
							message.From.Id, user.Lang)) {
						return;
					}

					account.AccountName = data;
					PasswordManager.AssemblingAccounts[message.From.Id] = account;
					PasswordManager.SetUserAction(user, Actions.Assemble);
					await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);

				} else if(!account.SkipLink && account.Link == null){
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxLinkLength, "Link",
							message.From.Id, user.Lang)) {
						return;
					}

					account.Link = data.BuildLink();
					PasswordManager.AssemblingAccounts[message.From.Id] = account;
					PasswordManager.SetUserAction(user, Actions.Assemble);
					if (account.Login == null) {
						await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
							"📇 " + Localization.GetMessage("AddLogin", user.Lang));
					}
					else {
						await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
							"G")));
					}
				} else if (account.Login == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxLoginLength, "Login",
							message.From.Id, user.Lang)) {
						return;
					}
					
					account.Login = data;
					PasswordManager.AssemblingAccounts[message.From.Id] = account;
					PasswordManager.SetUserAction(user, Actions.Assemble);
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
							"G")));

				} else if (account.Password == null) {
					if (await IsLengthExceeded(message.Text.Length,
							Account.maxPasswordLength, "Password",
							message.From.Id, user.Lang)) {
						return;
					}

					account.Password = Encryption.Encrypt(data);
					await SaveToDBAsync(account, user);
					PasswordManager.AssemblingAccounts.Remove(message.From.Id);
				}
			}
		}

		private async Task AssembleAccountAsync(Message message, Types.User user) {
			string[] accountData = message.Text.Remove(0, 5).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (await IsLengthExceeded(accountData[0].Length,
							Account.maxAccountNameLength, "AccountName",
							message.From.Id, user.Lang)) {
				return;
			}
			Account account = new Account {
				AccountName = accountData[0],
				UserId = message.From.Id
			};

			if (accountData.Length > 3) {
				if (await IsLengthExceeded(accountData[1].Length, Account.maxLinkLength, "Link", message.From.Id, user.Lang) ||
					await IsLengthExceeded(accountData[2].Length, Account.maxLoginLength, "Login", message.From.Id, user.Lang) ||
					await IsLengthExceeded(accountData[3].Length, Account.maxPasswordLength, "Password", message.From.Id, user.Lang)) {

					return;
				}
				account.Link = accountData[1].BuildLink();
				account.Login = accountData[2].Trim();
				account.Password = Encryption.Encrypt(accountData[3].Trim());
				await SaveToDBAsync(account, user);
			} else if(accountData.Length == 3) {
				if (accountData[1].Length > Account.maxLoginLength) {
					await ReportExceededLength(message.From.Id, user.Lang,
						Localization.GetMessage("Login", user.Lang), Account.maxLoginLength);
					return;
				}
				else if (accountData[2].Length > Account.maxPasswordLength) {
					await ReportExceededLength(message.From.Id, user.Lang,
						Localization.GetMessage("Password", user.Lang), Account.maxPasswordLength);
					return;
				}
				account.Login = accountData[1];
				account.Password = Encryption.Encrypt(accountData[2]);
				await SaveToDBAsync(account, user);
			} else if(accountData.Length == 2) {
				if (accountData[1].Length > Account.maxLoginLength) {
					await ReportExceededLength(message.From.Id, user.Lang,
						Localization.GetMessage("Login", user.Lang), Account.maxLoginLength);
					return;
				}
				account.Login = accountData[1].Trim();
				PasswordManager.AssemblingAccounts[message.From.Id] = account;
				PasswordManager.SetUserAction(user, Actions.Assemble);
				await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);
			} else {
				PasswordManager.AssemblingAccounts[message.From.Id] = account;
				PasswordManager.SetUserAction(user, Actions.Assemble);
				await AddLinkPrompt(message.Chat.Id, account.AccountName, user.Lang);
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

		private static async Task SaveToDBAsync(Account account, Types.User user, int messageToEditId = 0) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				account.Id = (long)conn.ExecuteScalar("Insert into Account (UserId, AccountName, Link, Login, Password) " +
					"values (@UserId, @AccountName, @Link, @Login, @Password);" +
					"select last_insert_rowid()",
					account);
			}

			PasswordManager.SetUserAction(user, Actions.Search);

			await PasswordManager.ShowAccount(account.UserId, account, user.Lang, messageToEditId: messageToEditId,
				extraMessage: "✅ " + String.Format(Localization.GetMessage("AccountAdded", user.Lang), account.AccountName));
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

		public static async Task UpdateCallBackMessageAsync(ChatId chatId, int messageId, Account account, Types.User user) {

			if (account.AccountName == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"📝 " + Localization.GetMessage("AddAccount", user.Lang));
			}
			else if (!account.SkipLink && account.Link == null) {
				await AddLinkPrompt(chatId, account.AccountName, user.Lang);
			}
			else if (account.Login == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"📇 " + Localization.GetMessage("AddLogin", user.Lang));
			}
			else if (account.Password == null) {
				await BotHandler.Bot.EditMessageTextAsync(chatId, messageId,
					"🔐 " + String.Format(Localization.GetMessage("AddPassword", user.Lang), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", user.Lang),
							"G")));
			}
			else {
				PasswordManager.AssemblingAccounts.Remove(account.UserId);
				await SaveToDBAsync(account, user, messageId);
			}
		}

		public static async Task ReportExceededLength(ChatId chatid, string langCode, string paramName, int maxLength) {
			await Bot.Instance.Client.SendTextMessageAsync(chatid,
				String.Format(Localization.GetMessage("MaxLength", langCode), paramName, maxLength));
		}

	}
}
