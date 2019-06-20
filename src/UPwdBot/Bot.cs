using System;
using System.IO;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Data.SQLite;
using Dapper;
using System.Linq;

namespace UPwdBot {
	public class Bot {

		public static Bot Instance { get; private set; } = new Bot();

		public TelegramBotClient Client { get; private set; }
		public ChatId AdminId { get; private set; }
		public string connString;

		private Bot() {
			Instance = this;
			BotSettings botSettings;
			using (StreamReader file = System.IO.File.OpenText(@"Properties/botSettings.json")) {
				JsonSerializer serializer = new JsonSerializer();
				botSettings = (BotSettings)serializer.Deserialize(file, typeof(BotSettings));
			}
			Client = new TelegramBotClient(botSettings.BotToken);
			AdminId = botSettings.AdminId;
			connString = botSettings.ConnectionString;
		}

		public async Task ReportStart() {
			await Client.SendTextMessageAsync(AdminId, "🔴\n");
		}

	}
}
