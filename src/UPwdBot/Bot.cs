using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace UPwdBot {
	public class Bot {

		public static Bot Instance { get; private set; } = new Bot();

		public TelegramBotClient Client { get; private set; }
		public ChatId AdminId { get; private set; }
		public MySqlConnection Connection { get; private set; } = null;
		private string connString;

		private Bot() {
			Instance = this;
			BotSettings botSettings;
			using (StreamReader file = System.IO.File.OpenText(@"Properties/botSettings.json")) {
				JsonSerializer serializer = new JsonSerializer();
				botSettings = (BotSettings)serializer.Deserialize(file, typeof(BotSettings));
			}
			Client = new TelegramBotClient(botSettings.BotToken);
			AdminId = botSettings.AdminId;
			connString = botSettings.GetDBConnectionString();
		}
		
		public bool ConnectToDB(out string message) {
			bool connected = false;
			try {
				Connection = new MySqlConnection(connString);
				Connection.Open();
				connected = true;
				message = "Connected to DB successfully";
			}
			catch (ArgumentException a_ex) {
				message = a_ex.ToString() + "\nShutting down";
			}
			catch (MySqlException ex) {
				message = ex.Message + "\nExeption Number: " + ex.Number + "\nShutting down";
				connected = false;
			}
			return connected;
		}

		public async Task Start() {
			string message;
			bool connected = ConnectToDB(out message);
			//Report start message
			await Client.SendTextMessageAsync(AdminId, "🔴\n" + message);

			if (!connected)
				Environment.Exit(0);
		}

	}
}
