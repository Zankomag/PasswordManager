using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UPwdBot.Commands;

namespace UPwdBot {
	public class BotHandler {
		public static BotHandler Instance { get; private set; }
		public static TelegramBotClient Bot { get => UPwdBot.Bot.Instance.Client; }
		public static Dictionary<string, ICommand> Commands { get; private set; } = new Dictionary<string, ICommand>();
		public static Dictionary<int, Account> AssemblingAccounts { get; set; } = new Dictionary<int, Account>();
		
		private BotHandler() {}

		static BotHandler() {
			if(Instance == null)
				Instance = new BotHandler();
			InitCommands();
		}

		public static  void InitCommands() {
			ICommand selectLanguageCommand = new SelectLanguageCommand();
			ICommand addAccountCommand = new AddAccountCommand();
			ICommand helpCommand = new HelpCommand();
			Commands.Add("/help", helpCommand);
			Commands.Add("/start",helpCommand);
			Commands.Add("/language", selectLanguageCommand);
			Commands.Add("/all", new ShowAllCommand());
			Commands.Add("/add", addAccountCommand);
			Commands.Add("add account", addAccountCommand);
		}

		public async void HandleUpdate(Update update) {
			switch (update.Type) {
				case UpdateType.Message:
					if(update.Message.Text != null)
						await HandleMessage(update.Message);
				break;
				case UpdateType.CallbackQuery:

				break;
			}
		}

		public async Task HandleMessage(Message message) {
			User user;
			using (IDbConnection conn = new SQLiteConnection(UPwdBot.Bot.Instance.connString)) {
				//Check if user exists in User table and add this user if not
				user = conn.QuerySingleOrDefault<User>("SELECT * FROM User WHERE Id = @Id", new { Id = message.From.Id });
				if (user == null) {
					conn.Execute("Insert into User (Id) values (@Id)", //DELETE, COOHING LANGUAGE HANDLER MUST INSERT NEW USER
						new { Id = message.From.Id });
					user = new User() {Id = message.From.Id};
					await (Commands["/language"] as IBaseCommand).ExecuteAsync(message);
				} else if(user.Lang == null) { //DELETE AFTER MAKE LOCALICATION SETTINGS
					await (Commands["/language"] as IBaseCommand).ExecuteAsync(message);
				}
			}
			string commandText = message.Text.ToLower();
			if (message.Text.StartsWith('/')){
				int cIndex = commandText.IndexOfAny(new char[] { ' ', '\n' });
				if(cIndex != -1)
					commandText = commandText.Substring(0, cIndex);
			}
			ICommand command;
			if(Commands.TryGetValue(commandText, out command)) {
				if(command is ILocalizedCommand)
					await (command as ILocalizedCommand).ExecuteAsync(message, user.Lang);
				else
					await (command as IBaseCommand).ExecuteAsync(message);
			} else {
				if (AssemblingAccounts.ContainsKey(message.From.Id)) {
					await (Commands["/add"] as ILocalizedCommand).ExecuteAsync(message, user.Lang);
				} //ELSE IF - USER IS SEARCHING ACCOUNT AND PUT SERSHING IF AS FIRST CHECK
				else {
					await Bot.SendTextMessageAsync(message.From.Id, "Don't get command\n" + commandText + "\n" + commandText.Length);
				}
			}
		}
	}
}
