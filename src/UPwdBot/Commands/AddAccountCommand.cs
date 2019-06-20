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
						"You have been adding another account.\n" +
						"Use /cancel to revoke it and then create a new one."); //ADD MESSAGE WHAT TO DO NEXT (ADD LINK OR ACCOUNT NAME...)
					return;
				}
				if(account.AccountName == null) {	
					account.AccountName = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					string autoLink = "🔗 ";
					autoLink += account.AccountName.Contains(' ') ?
										account.AccountName.Substring(0,
											account.AccountName.IndexOf(' ')).ToLower() :
										account.AccountName.ToLower();
					autoLink += ".com";
					var inlineKeyBoard = new InlineKeyboardMarkup(
						new InlineKeyboardButton[][] {
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData(autoLink,
									"A" + message.From.Id.ToString()
								)
							},
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("⏩ Skip", 
									"S" + message.From.Id.ToString())
							}
						}
					);
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔗 Good. Now you can add an optional link for your account or use suggested.\n" +
						"Use /help to get more info about links.", 
						replyMarkup: inlineKeyBoard);
				} else if(!account.SkipLink && account.Link == null){
					account.Link = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"📇 Nice. Choose a login for your account.\n");
				} else if (account.Login == null) {
					account.Login = message.Text;
					BotHandler.AssemblingAccounts[message.From.Id] = account;
					var inlineKeyBoard = new InlineKeyboardMarkup(
						InlineKeyboardButton.WithCallbackData("🌋 Generate",
							"G" + message.From.Id.ToString())
					);
					await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
						"🔐 Now you should send or generate a password for your account.\n" +
						"Create strong password with upper and lower case, numbers and special characters, you do NOT need to remember this password.\n" +
						"Use /gen to set up password generator.",
						replyMarkup: inlineKeyBoard);
				} else if (account.Password == null) {
					account.Password = Encryption.Encrypt(message.Text);
					await SaveToDBAsync(account);
					BotHandler.AssemblingAccounts.Remove(message.From.Id);
				}
			} else if (message.Text.StartsWith("/add") && message.Text.Length > 5) {
				await AssembleAccountAsync(message);
			} else {
				BotHandler.AssemblingAccounts.Add(message.From.Id, new Account() { UserId = message.From.Id});
				await BotHandler.Bot.SendTextMessageAsync(message.From.Id,
					"📝 Okay, a new account. Choose a name for it.\n" +
					"For example: name of service this account belongs to.\n" +
					"That's not login! You will recognize account by its name.\n" +
					"Use several words (like tags) to help yourself find it in the future.");
			}
		}

		private async Task AssembleAccountAsync(Message message) {
			string[] accountData = message.Text.Remove(0, 5).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			Account account = new Account {
				AccountName = accountData[0],
				UserId = message.From.Id
			};

			if (accountData.Length > 3) {
				if (accountData[1].StartsWith("https://") || accountData[1].StartsWith("http://"))
					account.Link = accountData[1].Trim();
				else
					account.Link = "http://" + accountData[1].Trim();

				account.Login = accountData[2].Trim();
				account.Password = Encryption.Encrypt(accountData[3].Trim());
				await SaveToDBAsync(account);
			} else if(accountData.Length == 3) {
				account.Login = accountData[1];
				account.Password = Encryption.Encrypt(accountData[2]);
				await SaveToDBAsync(account);
			} else if(accountData.Length == 2) {
				account.Login = accountData[1].Trim();
				BotHandler.AssemblingAccounts[message.From.Id] = account;
				//
				//TODO:
				//SEND MESSAGE FOR ADDING LINK PASSWORD
				//
			} else {
				BotHandler.AssemblingAccounts[message.From.Id] = account;
				//
				//TODO:
				//SEND MESSAGE FOR ADDING LINK, LOGIN, PASSWORD
				//
			}

		}

		private async Task SaveToDBAsync(Account account) {
			using (IDbConnection conn = new SQLiteConnection(Bot.Instance.connString)) {
				conn.Execute("Insert into Account (UserId, AccountName, Link, Login, Password) " +
					"values (@UserId, @AccountName, @Link, @Login, @Password)",
					account);
			}

			await BotHandler.Bot.SendTextMessageAsync(account.UserId,
				"✅ " + account.AccountName + " account has been added successfully!");
		}

	}
}
