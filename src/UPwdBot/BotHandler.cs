using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
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

		private static Dictionary<Actions, IMessageCommand> ActionCommands = new Dictionary<Actions, IMessageCommand>();


		private static readonly SelectLanguageCommand selectLangCommand = new SelectLanguageCommand();

		private BotHandler() {}

		static BotHandler() {
			if (Instance == null) {
				Instance = new BotHandler();
				InitCommands();
			}
		}

		private static  void InitCommands() {
			
			IMessageCommand helpCommand = new HelpCommand();
			SearchCommand searchCommand = new SearchCommand();
			IMessageCommand addAccountCommand = new AddAccountCommand();

			ActionCommands.Add(Actions.Assemble, addAccountCommand);
			ActionCommands.Add(Actions.Search, searchCommand);
			ActionCommands.Add(Actions.Update, new UpdateAccountCommand());

			MessageCommands.Add("/help", helpCommand);
			MessageCommands.Add("/start", helpCommand);
			MessageCommands.Add("/language", selectLangCommand);
			MessageCommands.Add("/all", new ShowAllCommand());
			MessageCommands.Add("/add", addAccountCommand);
			MessageCommands.Add("/cancel", new CancelCommand());
			MessageCommands.Add("/delete", new DeleteAllMessagesCommand());

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
					if(update.Message.Type == MessageType.Text)
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
					await selectLangCommand.ExecuteAsync(message, new Types.User { Lang = Localization.defaultLanguage });
					return;
				}
			}

			string commandText = message.Text.ToLower();

			//Command that starts with '/' may contain args and must be separated
			if (message.Text.StartsWith('/')){
				int cIndex = commandText.IndexOfAny(new char[] { ' ', '\n' });
				if(cIndex != -1)
					commandText = commandText.Substring(0, cIndex);
			}

			if (MessageCommands.TryGetValue(commandText, out IMessageCommand command)) {
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
					await selectLangCommand.ExecuteAsync(callbackQuery, user);
					return;
				}
			}

			try {
				ICallBackQueryCommand command;
				if (CallBackCommands.TryGetValue(callbackQuery.Data[0], out command)) {
					await command.ExecuteAsync(callbackQuery, user);
				}
				else {
					await Bot.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
				}
			} catch (Telegram.Bot.Exceptions.InvalidParameterException) { }
				
		}

		/// <summary>
		/// Tries to delete message. If unsuccessfully - edit it.
		/// </summary>
		public static async Task DeleteMessageAsync(ChatId chatId, int messageId) {
			try {
				await Bot.DeleteMessageAsync(chatId, messageId);
			}
			catch (Telegram.Bot.Exceptions.ApiRequestException) {
				await Bot.EditMessageTextAsync(chatId, messageId, "🗑️");
			}
		}

	}
}
