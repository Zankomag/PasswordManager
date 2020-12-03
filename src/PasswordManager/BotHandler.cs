using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using PasswordManager.Commands;
using PasswordManager.Types.Enums;
using Uten.Localization.MultiUser;
using User = PasswordManager.Types.User;

namespace PasswordManager {
	public class BotHandler {
		public static BotHandler Instance { get; private set; }
		public static TelegramBotClient Bot { get => global::PasswordManager.Bot.Instance.Client; }
		private static Dictionary<string, IMessageCommand> messageCommands = new Dictionary<string, IMessageCommand>();
		private static Dictionary<CallbackCommandCode, ICallBackQueryCommand> callBackCommands = new Dictionary<CallbackCommandCode, ICallBackQueryCommand>();

		private static readonly Dictionary<UserAction, IMessageCommand> actionCommands = new Dictionary<UserAction, IMessageCommand>();


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

			actionCommands.Add(UserAction.Assemble, addAccountCommand);
			actionCommands.Add(UserAction.Search, searchCommand);
			actionCommands.Add(UserAction.Update, updateAccountCommand);
			actionCommands.Add(UserAction.UpdatePasswordLength, setUpPasswordGeneratorCommand);

			messageCommands.Add("/help", helpCommand);
			messageCommands.Add("/start", helpCommand);
			messageCommands.Add("/language", selectLanguageCommand);
			messageCommands.Add("/all", new ShowAllCommand());
			messageCommands.Add("/add", addAccountCommand);
			messageCommands.Add("/cancel", new CancelCommand());
			messageCommands.Add("/generator", setUpPasswordGeneratorCommand);
			messageCommands.Add("/adduser", new AddUserCommand());
			messageCommands.Add("/removeuser", new RemoveUserCommand());
			messageCommands.Add("/userlist", new UserListCommand());
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
			try {
				User user;
				using (IDbConnection conn = new SQLiteConnection(global::PasswordManager.Bot.Instance.connString)) {
					//User must choose language before be added to db and using any command
					user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id", new { message.From.Id });
					if (user == null) {
						//^^^NEW USERS NOW MUST BE ADDED MANUALLY THIS WILL WORK FOR ADMIN ONLY NOW^^^^
						if (message.From.Id == global::PasswordManager.Bot.Instance.AdminId.Identifier)
						{
							if (Localization.ContainsLanguage(message.From.LanguageCode))
							{
								user = PasswordManager.AddUser(message.From.Id, message.From.LanguageCode);
							}
							else
							{
								await selectLanguageCommand.ExecuteAsync(message, new User { Lang = Localization.defaultLanguage });
								return;
							}
						}
						else
						{
							return;
						}
					}
				}

				string commandText = null;

				//Command that starts with '/' may contain args and must be separated
				if (message.Text.StartsWith('/')) {
					string commandString = message.Text.ToLower();
					int cIndex = commandString.IndexOfAny(new char[] { ' ', '\n' });
					commandText = cIndex != -1 ? commandString.Substring(0, cIndex) : commandString;
				}

				if (commandText != null && messageCommands.TryGetValue(commandText, out IMessageCommand command)) {
					await command.ExecuteAsync(message, user);
				}
				else {
					try {
						await actionCommands[user.ActionType].ExecuteAsync(message, user);
					}
					catch (KeyNotFoundException) {
						PasswordManager.SetUserAction(user, UserAction.Search);
						await actionCommands[UserAction.Search].ExecuteAsync(message, user);
					}
				}
			}
			catch (Exception ex) {
				await Bot.SendTextMessageAsync(global::PasswordManager.Bot.Instance.AdminId, ex.ToString() + "\n\n" + ex.Message);
				Environment.Exit(47);
			}
		}


		private async void HandleCallbackQuery(CallbackQuery callbackQuery) {
			User logUser = null;
			try {
				User user;
				using (IDbConnection conn = new SQLiteConnection(global::PasswordManager.Bot.Instance.connString)) {
					//Check if user exists in User table and add this user if not
					user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id",
						new { callbackQuery.From.Id });
					if (user == null) {
						//Add new user to db when he selected language for the first time
						//^^^NEW USERS NOW MUST BE ADDED MANUALLY THIS WILL WORK FOR ADMIN ONLY NOW^^^^
						if (callbackQuery.From.Id == global::PasswordManager.Bot.Instance.AdminId.Identifier)
						{
							if (callbackQuery.Data[0] == 'L')
							{
								await selectLanguageCommand.ExecuteAsync(callbackQuery, user);
								return;
							}
							else
							{
								if (Localization.ContainsLanguage(callbackQuery.From.LanguageCode))
								{
									user = PasswordManager.AddUser(callbackQuery.From.Id, callbackQuery.From.LanguageCode);
								}
								else
								{
									await selectLanguageCommand.ExecuteAsync(
										new Message()
										{
											From = new Telegram.Bot.Types.User()
											{
												Id = callbackQuery.From.Id
											}
										},
										new User { Lang = Localization.defaultLanguage });
									return;
								}
							}
						} else
						{
							return;
						}
					}
					logUser = user;
				}

					ICallBackQueryCommand command;
					if (callBackCommands.TryGetValue((CallbackCommandCode)callbackQuery.Data[0], out command)) {
						await command.ExecuteAsync(callbackQuery, user);
					}
					else {
						await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
					}
				
			}
			catch (Telegram.Bot.Exceptions.InvalidParameterException) { }
			catch (Exception ex) {
				await Bot.SendTextMessageAsync(global::PasswordManager.Bot.Instance.AdminId, "EXCEPTION: " + ex.ToString() + "\n\nUSER: " + (logUser == null ? "null" : (logUser.Id.ToString() + "\n\n  [user](tg://user?id=" + logUser.Id + ")")), ParseMode.Markdown);
				Environment.Exit(47);
			}

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
				catch (Exception ex) {
					await Bot.SendTextMessageAsync(global::PasswordManager.Bot.Instance.AdminId, ex.ToString() + "\n\n" + ex.Message);
				}
			}
		}

	}
}
