using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UPwdBot.Commands;
using UPwdBot.Types;
using Uten.Localization.MultiUser;
using User = UPwdBot.Types.User;

namespace UPwdBot {
	public class BotHandler {
		public static BotHandler Instance { get; private set; }
		public static TelegramBotClient Bot { get => UPwdBot.Bot.Instance.Client; }
		public static Dictionary<string, IMessageCommand> MessageCommands { get; private set; } = new Dictionary<string, IMessageCommand>();
		public static Dictionary<char, ICallBackQueryCommand> CallBackCommands { get; private set; } = new Dictionary<char, ICallBackQueryCommand>();
		public static Dictionary<int, Account> AssemblingAccounts { get; set; } = new Dictionary<int, Account>();

		private static readonly SearchCommand searchCommand = new SearchCommand();

		private BotHandler() {}

		static BotHandler() {
			if (Instance == null) {
				Instance = new BotHandler();
				InitCommands();
			}
		}

		private static  void InitCommands() {
			SelectLanguageCommand selectLangCommand = new SelectLanguageCommand();
			IMessageCommand addAccountCommand = new AddAccountCommand();
			IMessageCommand helpCommand = new HelpCommand();

			MessageCommands.Add("/help", helpCommand);
			MessageCommands.Add("/start",helpCommand);
			MessageCommands.Add("/language", selectLangCommand);
			MessageCommands.Add("/all", new ShowAllCommand());
			MessageCommands.Add("/add", addAccountCommand);
			MessageCommands.Add("/cancel", new CancelCommand());

			CallBackCommands.Add('L', selectLangCommand);
			CallBackCommands.Add('S', new SkipLinkCommand());
			CallBackCommands.Add('A', new AutoLinkCommand());
			CallBackCommands.Add('G', new GeneratePasswordCommand());
			CallBackCommands.Add('Z', new AcceptPasswordCommand());
			CallBackCommands.Add('Q', searchCommand);
			CallBackCommands.Add('P', new ShowPasswordCommand());
			CallBackCommands.Add('O', new ShowAccountCommand());
			CallBackCommands.Add('D', new DeleteMessageCommand());
			CallBackCommands.Add('U', new UpdateAccountCommand());
			CallBackCommands.Add('X', new DeleteAccountCommand());
		}

		public void HandleUpdate(Update update) {
			switch (update.Type) {
				case UpdateType.Message:
					if(update.Message.Text != null)
						HandleMessage(update.Message);
				return;
				case UpdateType.CallbackQuery:
					HandleCallbackQuery(update.CallbackQuery);
				return;
			}
		}

		private async void HandleMessage(Message message) {
			User user;
			using (IDbConnection conn = new SQLiteConnection(UPwdBot.Bot.Instance.connString)) {
				//User must choose language before using any command
				user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id", new { message.From.Id });
				if (user == null) {
					await MessageCommands["/language"].ExecuteAsync(message, Localization.defaultLanguage);
					return;
				}
			}

			string commandText = message.Text.ToLower();

			//Command that starts with '/' can contain args
			if (message.Text.StartsWith('/')){
				int cIndex = commandText.IndexOfAny(new char[] { ' ', '\n' });
				if(cIndex != -1)
					commandText = commandText.Substring(0, cIndex);
			}

			string langCode = Localization.ContainsLanguage(user.Lang) ? user.Lang : Localization.defaultLanguage;
			IMessageCommand command;
			if(MessageCommands.TryGetValue(commandText, out command)) {
					await command.ExecuteAsync(message, langCode);
			} else {
				//if user doesn't setting up account then search
				if (!AssemblingAccounts.ContainsKey(message.From.Id)) {
					await searchCommand.ExecuteAsync(message, langCode);
				} else {
					await MessageCommands["/add"].ExecuteAsync(message, langCode);
				}
			}
		}

		private async void HandleCallbackQuery(CallbackQuery callbackQuery) {
			User user;
			using (IDbConnection conn = new SQLiteConnection(UPwdBot.Bot.Instance.connString)) {
				//Check if user exists in User table and add this user if not
				user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id",
					new { callbackQuery.From.Id });
				if (user == null) {
					//Add new user to db with selected language
					await CallBackCommands['L'].ExecuteAsync(callbackQuery, user);
					return;
				}
			}

			ICallBackQueryCommand command;
			if (CallBackCommands.TryGetValue(callbackQuery.Data[0], out command)) {
				await command.ExecuteAsync(callbackQuery, user);
			} else {
				await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
			}
				
		} 

	}
}
