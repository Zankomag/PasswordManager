using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace UPwdBot {
	public class Bot {

		public static Bot Instance { get; private set; } = new Bot();

		public TelegramBotClient Client { get; private set; }
		public ChatId AdminId { get; private set; }
		public string connString;
		private string domain;

		private string token;

		private Bot() {
			Instance = this;
			BotSettings botSettings;
			using (StreamReader file = System.IO.File.OpenText(@"Properties/botSettings.json")) {
				JsonSerializer serializer = new JsonSerializer();
				botSettings = (BotSettings)serializer.Deserialize(file, typeof(BotSettings));
			}
			token = botSettings.BotToken;
			Client = new TelegramBotClient(botSettings.BotToken);
			AdminId = botSettings.AdminId;
			connString = botSettings.ConnectionString;
			domain = botSettings.Domain;
		}

		public async Task ReportStart() {
			await Client.SetWebhookAsync(
				$"https://{domain}/api/bots/{token}",
				allowedUpdates:  new UpdateType[] {
					UpdateType.Message,
					UpdateType.CallbackQuery});
			await Client.SendTextMessageAsync(AdminId, "🔴");
		}

		public bool IsTokenCorrect(string token) {
			return token != null ? token == this.token : false;
		}

	}
}
