using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UPwdBot.Commands;
using UPwdBot.Types.Enums;
using Uten.Localization.MultiUser;
using User = UPwdBot.Types.User;

namespace UPwdBot {
	public class BotHandler {
		public static BotHandler Instance { get; private set; }
		public static TelegramBotClient Bot { get => UPwdBot.Bot.Instance.Client; }
		private static Dictionary<string, IMessageCommand> messageCommands = new Dictionary<string, IMessageCommand>();
		private static Dictionary<CallbackCommandCode, ICallBackQueryCommand> callBackCommands = new Dictionary<CallbackCommandCode, ICallBackQueryCommand>();

		private static readonly Dictionary<UserAction, IMessageCommand> ActionCommands = new Dictionary<UserAction, IMessageCommand>();


		private static readonly SelectLanguageCommand selectLanguageCommand = new SelectLanguageCommand();

		private BotHandler() { }

		static BotHandler() {
			if (Instance == null) {
				Instance = new BotHandler();
				InitCommands();
			}
		}

		private static void InitCommands() {
			IMessageCommand helpCommand = new HelpCommand();
			SearchCommand searchCommand = new SearchCommand();
			IMessageCommand addAccountCommand = new AddAccountCommand();
			UpdateAccountCommand updateAccountCommand = new UpdateAccountCommand();
			SetUpPasswordGeneratorCommand setUpPasswordGeneratorCommand = new SetUpPasswordGeneratorCommand();

			ActionCommands.Add(UserAction.Assemble, addAccountCommand);
			ActionCommands.Add(UserAction.Search, searchCommand);
			ActionCommands.Add(UserAction.Update, updateAccountCommand);
			ActionCommands.Add(UserAction.UpdatePasswordLength, setUpPasswordGeneratorCommand);

			messageCommands.Add("/help", helpCommand);
			messageCommands.Add("/start", helpCommand);
			messageCommands.Add("/language", selectLanguageCommand);
			messageCommands.Add("/all", new ShowAllCommand());
			messageCommands.Add("/add", addAccountCommand);
			messageCommands.Add("/cancel", new CancelCommand());
			messageCommands.Add("/generator", setUpPasswordGeneratorCommand);
			//messageCommands.Add("/delete", new DeleteAllMessagesCommand()); //EXPERIMENTAL

			callBackCommands.Add(CallbackCommandCode.SelectLanguage, selectLanguageCommand);
			callBackCommands.Add(CallbackCommandCode.SkipLink, new SkipLinkCommand());
			callBackCommands.Add(CallbackCommandCode.AutoLink, new AutoLinkCommand());
			callBackCommands.Add(CallbackCommandCode.GeneratePassword, new GeneratePasswordCommand());
			callBackCommands.Add(CallbackCommandCode.AcceptPassword, new AcceptPasswordCommand());
			callBackCommands.Add(CallbackCommandCode.Search, searchCommand);
			callBackCommands.Add(CallbackCommandCode.ShowPassword, new ShowPasswordCommand());
			callBackCommands.Add(CallbackCommandCode.ShowAccount, new ShowAccountCommand());
			callBackCommands.Add(CallbackCommandCode.DeleteMessage, new DeleteMessageCommand());
			callBackCommands.Add(CallbackCommandCode.UpdateAccount, updateAccountCommand);
			callBackCommands.Add(CallbackCommandCode.DeleteAccount, new DeleteAccountCommand());
			callBackCommands.Add(CallbackCommandCode.SetUpPasswordGenerator, setUpPasswordGeneratorCommand);
		}

		public void HandleUpdate(Update update) {
			switch (update.Type) {
				case UpdateType.Message:
				if (update.Message.Type == MessageType.Text)
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
				//User must choose language before be added to db and using any command
				user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id", new { message.From.Id });
				if (user == null) {
					if (Localization.ContainsLanguage(message.From.LanguageCode)) {
						user = PasswordManager.AddUser(message.From.Id, message.From.LanguageCode);
					}
					else {
						await selectLanguageCommand.ExecuteAsync(message, new User { Lang = Localization.defaultLanguage });
						return;
					}
				}
			}

			string commandText = null;

			//Command that starts with '/' may contain args and must be separated
			if (message.Text.StartsWith('/')) {
				string commandString = message.Text.ToLower();
				int cIndex = commandString.IndexOfAny(new char[] { ' ', '\n' });
				if (cIndex != -1) {
					commandText = commandString.Substring(0, cIndex);
				}
				else {
					commandText = commandString;
				}
			}

			if (commandText != null && messageCommands.TryGetValue(commandText, out IMessageCommand command)) {
				await command.ExecuteAsync(message, user);
			}
			else {
				await ActionCommands[user.ActionType].ExecuteAsync(message, user);
			}
		}


		private async void HandleCallbackQuery(CallbackQuery callbackQuery) {
			User user;
			using (IDbConnection conn = new SQLiteConnection(UPwdBot.Bot.Instance.connString)) {
				//Check if user exists in User table and add this user if not
				user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id",
					new { callbackQuery.From.Id });
				if (user == null) {
					//Add new user to db when he selected language for the first time
					if (callbackQuery.Data[0] == 'L') {
						await selectLanguageCommand.ExecuteAsync(callbackQuery, user);
						return;
					}
					else {
						if (Localization.ContainsLanguage(callbackQuery.From.LanguageCode)) {
							user = PasswordManager.AddUser(callbackQuery.From.Id, callbackQuery.From.LanguageCode);
						}
						else {
							await selectLanguageCommand.ExecuteAsync(
								new Message() {
									From = new Telegram.Bot.Types.User() {
										Id = callbackQuery.From.Id
									}
								},
								new User { Lang = Localization.defaultLanguage });
							return;
						}
					}
				}
			}

			try {
				ICallBackQueryCommand command;
				if (callBackCommands.TryGetValue((CallbackCommandCode)callbackQuery.Data[0], out command)) {
					await command.ExecuteAsync(callbackQuery, user);
				}
				else {
					await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
				}
			}
			catch (Telegram.Bot.Exceptions.InvalidParameterException) { }

		}

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public static async Task TryDeleteMessageAsync(ChatId chatId, int messageId) {
			try {
				await Bot.DeleteMessageAsync(chatId, messageId);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException) {
				try {
					await Bot.EditMessageTextAsync(chatId, messageId, "🗑️");
				}
				catch (Exception) { }
			}
		}

	}
}
