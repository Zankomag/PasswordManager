
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UPwdBot.Commands;

namespace UPwdBot {
	public class BotHandler {
		public static BotHandler Instance { get; private set; } = new BotHandler();
		public TelegramBotClient Bot { get; private set; }
		public Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();
		public Dictionary<int, Account> AssemblingAccounts = new Dictionary<int, Account>();
		private BotHandler() {
			Instance = this;
			Bot = UPwdBot.Bot.Instance.Client;
			InitCommands();
		}

		private void InitCommands() {
			ICommand addAccountCommand = new AddAccountCommand();

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
			}
		}

		public async Task HandleMessage(Message message) {
			//string commandText = message.Text.ToLower().Split(new char[] { ' ', '\n'})[0];
			string commandText = message.Text.ToLower();
			if (message.Text.StartsWith('/')){
				int cIndex = commandText.IndexOfAny(new char[] { ' ', '\n' });
				if(cIndex != -1)
					commandText = commandText.Substring(0, cIndex);
			}
			ICommand command;
			if(Commands.TryGetValue(commandText, out command)) {
				await command.ExecuteAsync(message);
			} else {
				if (BotHandler.Instance.AssemblingAccounts.ContainsKey(message.From.Id)) {
					await Commands["/add"].ExecuteAsync(message);
				} //ELSE IF SEARCHING ACCOUNT AND PUT SERSHING IF AS FIRST CHECK
				else {
					await Bot.SendTextMessageAsync(message.From.Id, "Don't get command\n" + commandText + "\n" + commandText.Length);
				}
			}

		}
	}
}
