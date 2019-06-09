using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Telegram.Bot;

namespace UPwdBot {
	public class Bot {

		public static Bot Instance { get; private set; } = new Bot();

		public TelegramBotClient Client { get; private set; }

		private Bot() {
			Instance = this;
			BotSettings botSettings;
			using (StreamReader file = File.OpenText(@"Properties/botSettings.json")) {
				JsonSerializer serializer = new JsonSerializer();
				botSettings = (BotSettings)serializer.Deserialize(file, typeof(BotSettings));
			}
				Client = new TelegramBotClient(botSettings.BotToken);
		}
	}
}
