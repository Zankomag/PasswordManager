using Telegram.Bot.Types;
using Newtonsoft.Json;
using System;

namespace UPwdBot {
	public struct DataBaseConnection {
		public string Server;
		public string UserId;
		public string Password;
		public string DataBase;
	}

	public class BotSettings {
		public string BotToken;
		public ChatId AdminId;
		public DataBaseConnection DBConnection;
		public string GetDBConnectionString() {
			if (DBConnection.Server != null)
				return $"Server={DBConnection.Server};User ID={DBConnection.UserId};Password={DBConnection.Password};Database={DBConnection.DataBase}";
			else
				return String.Empty;
		}

	}
}
