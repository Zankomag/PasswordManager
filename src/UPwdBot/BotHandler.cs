using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace UPwdBot {
	public class BotHandler {
		public static BotHandler Instance { get; private set; } = new BotHandler();
		private TelegramBotClient bot;
		private BotHandler() {
			Instance = this;
			bot = Bot.Instance.Client;
		}

		public async void HandleUpdate(Update update) {
			switch (update.Type) {
				case UpdateType.Message:
				await HandleMessage(update.Message);
				break;
			}
		}

		public async Task HandleMessage(Message message) {

			if(message.Text[0] == '/' && message.Text.Length > 1) {
				string decString = Uten.Encryption.Encryption.Decrypt(message.Text.Substring(1));
				await bot.SendTextMessageAsync(message.From.Id, decString);
				return;
			}

			string encryptedMessage = Uten.Encryption.Encryption.Encrypt(message.Text);
			await bot.SendTextMessageAsync(message.From.Id, encryptedMessage);


			//string accountName = message.Text.Length <= 50 ? message.Text : message.Text.Remove(49);
			//using (var cmd = new MySqlCommand(
			//	"SELECT accountName, link, login " +
			//	" FROM passwordManager.accounts " +
			//	"where userId = " + message.From.Id +
			//	" and accountName like '%" + accountName + "%'",
			//	Bot.Instance.Connection)) {

			//	using (var reader = cmd.ExecuteReader()) {
			//		while (reader.Read()) {
			//			await bot.SendTextMessageAsync(message.From.Id, reader.GetString(0) + Environment.NewLine
			//				+ reader.GetString(1)
			//				+ Environment.NewLine + reader.GetString(2), disableWebPagePreview: true);
			//		}
			//	}
			//}
		}
	}
}
