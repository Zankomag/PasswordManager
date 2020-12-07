using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using PasswordManager.Bot.Commands;
using PasswordManager.Bot.Types.Enums;
using MultiUserLocalization;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Core.Repositories;
using System.Linq;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot {
	public class BotHandlerService {
		private readonly IBotService botService;
		private readonly IUnitOfWork workUnit;
		private static Dictionary<string, IMessageCommand> messageCommands = new Dictionary<string, IMessageCommand>();
		private static Dictionary<CallbackCommandCode, ICallBackQueryCommand> callBackCommands = new Dictionary<CallbackCommandCode, ICallBackQueryCommand>();

		private static readonly Dictionary<UserAction, IMessageCommand> actionCommands = new Dictionary<UserAction, IMessageCommand>();


		private static readonly SelectLanguageCommand selectLanguageCommand = new SelectLanguageCommand();

		public BotHandlerService(IBotService botService, IUnitOfWork workUnit) {
			this.botService = botService;
			this.workUnit = workUnit;
			InitCommands();
		}

		private static void InitCommands() {
			IMessageCommand helpCommand = new HelpCommand();
			ICommand searchCommand = new SearchCommand();
			IMessageCommand addAccountCommand = new AddAccountCommand();
			ICommand updateAccountCommand = new UpdateAccountCommand();
			ICommand setUpPasswordGeneratorCommand = new SetUpPasswordGeneratorCommand();

			actionCommands.Add(UserAction.Assemble, addAccountCommand);
			actionCommands.Add(UserAction.Search, (IMessageCommand)searchCommand);
			actionCommands.Add(UserAction.Update, (IMessageCommand)updateAccountCommand);
			actionCommands.Add(UserAction.UpdatePasswordLength, (IMessageCommand)setUpPasswordGeneratorCommand);

			messageCommands.Add("/help", helpCommand);
			messageCommands.Add("/start", helpCommand);
			messageCommands.Add("/language", selectLanguageCommand);
			messageCommands.Add("/all", new ShowAllCommand());
			messageCommands.Add("/add", addAccountCommand);
			messageCommands.Add("/cancel", new CancelCommand());
			messageCommands.Add("/generator", (IMessageCommand)setUpPasswordGeneratorCommand);
			messageCommands.Add("/adduser", new AddUserCommand());
			messageCommands.Add("/removeuser", new RemoveUserCommand());
			messageCommands.Add("/userlist", new UserListCommand());

			callBackCommands.Add(CallbackCommandCode.SelectLanguage, selectLanguageCommand);
			callBackCommands.Add(CallbackCommandCode.SkipLink, new SkipLinkCommand());
			callBackCommands.Add(CallbackCommandCode.AutoLink, new AutoLinkCommand());
			callBackCommands.Add(CallbackCommandCode.GeneratePassword, new GeneratePasswordCommand());
			callBackCommands.Add(CallbackCommandCode.AcceptPassword, new AcceptPasswordCommand());
			callBackCommands.Add(CallbackCommandCode.Search, (ICallBackQueryCommand)searchCommand);
			callBackCommands.Add(CallbackCommandCode.ShowPassword, new ShowPasswordCommand());
			callBackCommands.Add(CallbackCommandCode.ShowAccount, new ShowAccountCommand());
			callBackCommands.Add(CallbackCommandCode.DeleteMessage, new DeleteMessageCommand());
			callBackCommands.Add(CallbackCommandCode.UpdateAccount, (ICallBackQueryCommand)updateAccountCommand);
			callBackCommands.Add(CallbackCommandCode.DeleteAccount, new DeleteAccountCommand());
			callBackCommands.Add(CallbackCommandCode.SetUpPasswordGenerator, (ICallBackQueryCommand)setUpPasswordGeneratorCommand);
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
				BotUser user;
				using (IDbConnection conn = new SQLiteConnection(PasswordManager.Bot.BotService.Instance.connString)) {
					//User must choose language before be added to db and using any command
					user = conn.QuerySingleOrDefault<User>("SELECT * FROM Users WHERE Id = @Id", new { message.From.Id });
					if (user == null) {
						//ONLY If unauthorized user is admin - bot treats them as new user
						//Not-admin users must be added to bot by Admin manually
						//If you want to allow free registration in your bot for any user - disable this admin check
						if (botService.Admins.Contains(message.From.Id)) {
							if (Localization.ContainsLanguage(message.From.LanguageCode)) {
								user = PasswordManagerService.AddUser(message.From.Id, message.From.LanguageCode);
							}
							else {
								await selectLanguageCommand.ExecuteAsync(message, new BotUser { Lang = Localization.defaultLanguage });
								return;
							}
						}
						return;
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
						await actionCommands[user.Action].ExecuteAsync(message, user);
					}
					catch (KeyNotFoundException) {
						PasswordManagerService.SetUserAction(user, UserAction.Search);
						await actionCommands[UserAction.Search].ExecuteAsync(message, user);
					}
				}
			}
			catch (Exception ex) {
				botService.SendMessageToAllAdmins(ex.ToString());
				//TODO: Log Exception
				throw;
			}
		}


		private async void HandleCallbackQuery(CallbackQuery callbackQuery) {
			BotUser logUser = null;
			try {
				BotUser user;
				using (IDbConnection conn = new SQLiteConnection(PasswordManager.Bot.BotService.Instance.connString)) {
					//Check if user exists in User table and add this user if not
					user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id",
						new { callbackQuery.From.Id });
					if (user == null) {
						//Add new user to db when he selected language for the first time
						//^^^NEW USERS NOW MUST BE ADDED MANUALLY THIS WILL WORK FOR ADMIN ONLY NOW^^^^
						if (callbackQuery.From.Id == PasswordManager.Bot.BotService.Instance.AdminId.Identifier)
						{
							if (callbackQuery.Data[0] == (char)CallbackCommandCode.SelectLanguage)
							{
								await selectLanguageCommand.ExecuteAsync(callbackQuery, user);
								return;
							}
							else
							{
								if (Localization.ContainsLanguage(callbackQuery.From.LanguageCode))
								{
									user = PasswordManagerService.AddUser(callbackQuery.From.Id, callbackQuery.From.LanguageCode);
								}
								else
								{
									await selectLanguageCommand.ExecuteAsync(
										new Message()
										{
											From = new Telegram.Bot.Types.User
											{
												Id = callbackQuery.From.Id
											}
										},
										new BotUser { Lang = Localization.defaultLanguage });
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
					try {
						await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
					} catch { }
				}
				
			}
			catch (Telegram.Bot.Exceptions.InvalidParameterException) { }
			catch (Exception ex) {
				string message = "EXCEPTION: " + ex.ToString();
				if(logUser != null)
					message += "\n\nUSER: [user](tg://user?id=" + logUser.Id.ToString() + ")";
				botService.SendMessageToAllAdmins(message);
				//TODO: Log exception
			}

		}

	}
}
