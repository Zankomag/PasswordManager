using Dapper;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Uten.Encryption;
using UPwdBot.Types;

namespace UPwdBot.Commands {
	public class AddAccountCommand : IMessageCommand {
		public async Task ExecuteAsync(Message message, string langCode) {
			Account account = new Account();
			//if user is already assembling account - CONTINUE assembling it instead of creating new
			if (BotHandler.AssemblingAccounts.TryGetValue(message.From.Id, out account)) {
				if (message.Text.StartsWith("/add")) {
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						String.Format(Localization.GetMessage("AlreadyAdding", langCode), "/cancel")); 
					//TODO:
					//ADD MESSAGE WHAT TO DO NEXT (ADD LINK OR ACCOUNT NAME...)
					return;
				}
				if(account.AccountName == null) {
					if (message.Text.Length > Account.maxAccountNameLength) {
						await ReportExceededLength(message.From.Id, langCode,
							Localization.GetMessage("AccName", langCode), Account.maxAccountNameLength);
						return;
					}

					account.AccountName = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await AddLinkPrompt(message.Chat.Id, account.AccountName, langCode);

				} else if(!account.SkipLink && account.Link == null){
					if (message.Text.Length > Account.maxLinkLength) {
						await ReportExceededLength(message.From.Id, langCode,
							Localization.GetMessage("Link", langCode), Account.maxLinkLength);
						return;	
					}

					account.Link = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"📇 " + Localization.GetMessage("AddLogin", langCode));

				} else if (account.Login == null) {
					if (message.Text.Length > Account.maxLoginLength) {
						await ReportExceededLength(message.From.Id, langCode,
							Localization.GetMessage("Login", langCode), Account.maxLoginLength);
						return;
					}

					account.Login = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔐 " + String.Format(Localization.GetMessage("AddPassword", langCode), "/gen"),
					replyMarkup: new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 " + Localization.GetMessage("Generate", langCode),
							"G")));

				} else if (account.Password == null) {
					if (message.Text.Length > Account.maxPasswordLength) {
						await ReportExceededLength(message.From.Id, langCode,
							Localization.GetMessage("Password", langCode), Account.maxPasswordLength);
						return;
					}

					account.Password = Encryption.Encrypt(message.Text);
					await SaveToDBAsync(account, langCode);
					BotHandler.AssemblingAccounts.Remove(message.From.Id);
				}

			} else if (message.Text.StartsWith("/add") && message.Text.Length > 5) {
				await AssembleAccountAsync(message, langCode);

			} else {
				BotHandler.AssemblingAccounts.Add(message.From.Id, new Account() { UserId = message.From.Id});
				await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
					"📝 " + Localization.GetMessage("AddAccount", langCode));
			}
		}

		private async Task AssembleAccountAsync(Message message, string langCode) {
			string[] accountData = message.Text.Remove(0, 5).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			if (accountData[0].Length > Account.maxAccountNameLength) {
				await ReportExceededLength(message.From.Id, langCode,
					Localization.GetMessage("AccName", langCode), Account.maxAccountNameLength);
				return;
			}
			Account account = new Account {
				AccountName = accountData[0],
				UserId = message.From.Id
			};

			if (accountData.Length > 3) {
				if (accountData[1].Length > Account.maxLinkLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Link", langCode), Account.maxLinkLength);
					return;
				} else if (accountData[2].Length > Account.maxLoginLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Login", langCode), Account.maxLoginLength);
					return;
				} else if (accountData[3].Length > Account.maxPasswordLength) {
					await ReportExceededLength(message.From.Id, langCode,
						Localization.GetMessage("Password", langCode), Account.maxPasswordLength);
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

		private static async Task SaveToDBAsync(Account account, string langCode) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				conn.Execute("Insert into Account (UserId, AccountName, Link, Login, Password) " +
					"values (@UserId, @AccountName, @Link, @Login, @Password)",
					account);
			}

			await BotHandler.Bot.SendTextMessageAsync(account.UserId,
				"✅ " + String.Format(Localization.GetMessage("AccountAdded", langCode), account.AccountName));
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
