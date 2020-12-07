using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;
using PasswordManager.Bot.Abstractions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot {

	/// <summary>
	/// Telegram Bot Service
	/// </summary>
	public class BotService : IBotService {

		public TelegramBotClient Client { get; private set; }

		public int[] Admins { get; private set; }

		private readonly string token;

		public BotService(IOptions<BotSettings> botSettingsConfig) {
			BotSettings botSettings = botSettingsConfig.Value;

			token = botSettings.Token;
			try {
				Client = new TelegramBotClient(botSettings.Token);
			} catch (ArgumentException exception) {
				//TODO
				//Log exception
				throw;
			}
			if (botSettings.AdminIds.Length == 0) {
				//App is not closed here because it can run without telegram bot
				//using only site and API
				ArgumentException exception = new ArgumentException(
					"Bot cannot have zero admins",
					nameof(botSettings.AdminIds));
				//TODO
				//Log exception
				throw exception;
			}
			Admins = botSettings.AdminIds;
			SetWebhook(botSettings.Domain);
			ReportStart();
		}

		private void SetWebhook(string domain) {
			try {
				Client.SetWebhookAsync(
						$"https://{domain}/api/bots/{token}",
						allowedUpdates: new UpdateType[] {
					UpdateType.Message,
					UpdateType.CallbackQuery}).Wait();
			} catch(Exception exception) {
				SendMessageToAllAdmins("EXception was thrown while setting webhook.\nBot has bot started.\nSee logs for more info.");
				//TODO
				//Log exception
				throw;
			}
		}

		/// <summary>
		/// Bot sends message to all admins indicating that he is running
		/// </summary>
		private void ReportStart() {
			string message = "🔴";
			SendMessageToAllAdmins(message);
		}

		private bool SendMessageToAdmin(int adminId, string message) {
			try {
				Client.SendTextMessageAsync(adminId, message, ParseMode.Markdown).Wait();
			} catch {
				//TODO
				//Log exception and admin Id and bot Id
				return false;
			}
			return true;
		}

		public bool SendMessageToAllAdmins(string message) {
			if (Admins.Length == 1)
				return SendMessageToAdmin(Admins[0], message);

			for (int i = 0; i < Admins.Length; i++) {
				if (!SendMessageToAdmin(Admins[i], message))
					return false;
				//To prevent bot from being banned for spamming
				Thread.Sleep(500);
			}
			return true;
		}

		public bool IsTokenCorrect(string token) {
			return token != null && token == this.token;
		}

		public async Task TryDeleteMessageAsync(ChatId chatId, int messageId) {
			try {
				await Client.DeleteMessageAsync(chatId, messageId);
			} catch {
				try {
					await Client.EditMessageTextAsync(chatId, messageId, "🗑");
				} catch { }
			}
		}

	}
}
