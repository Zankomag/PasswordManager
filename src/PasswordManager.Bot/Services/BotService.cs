using Microsoft.Extensions.Options;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services {

	/// <summary>
	/// Telegram Bot Service
	/// </summary>
	public class BotService : IBotService {

		public TelegramBotClient Client { get; private set; }

		//TODO:
		//Add feature to edit admins at runtime
		private int[] admins;

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
			if (botSettings.AdminIds == null || botSettings.AdminIds.Length == 0) {
				//App is not closed here because it can run without telegram bot
				//using only site and API
				ArgumentException exception = new ArgumentException(
					"Bot cannot have zero admins",
					nameof(botSettings));
				//TODO
				//Log exception
				throw exception;
			}
			admins = botSettings.AdminIds;
			SetWebhook(botSettings.Domain).Wait();
			ReportStart().Wait();
		}

		private async Task SetWebhook(string domain) {
			try {
				await Client.SetWebhookAsync(
						$"https://{domain}/api/bots/{token}",
						allowedUpdates: new UpdateType[] {
					UpdateType.Message,
					UpdateType.CallbackQuery});
			} catch (Exception exception) {
				await SendMessageToAllAdmins("EXception was thrown while setting webhook.\nBot has bot started.\nSee logs for more info.");
				//TODO
				//Log exception
				throw;
			}
		}

		/// <summary>
		/// Bot sends message to all admins indicating that he is running
		/// </summary>
		private async Task ReportStart() {
			string message = "🔴";
			await SendMessageToAllAdmins(message);
		}

		private async Task<bool> SendMessageToAdmin(int adminId, string message, ParseMode parseMode) {
			try {
				await Client.SendTextMessageAsync(adminId, message, parseMode);
			} catch {
				//TODO
				//Log exception and admin Id and bot Id
				return false;
			}
			return true;
		}

		public async Task<bool> SendMessageToAllAdmins(string message, ParseMode parseMode = ParseMode.Default) {
			if (admins.Length == 1)
				return await SendMessageToAdmin(admins[0], message, parseMode);

			for (int i = 0; i < admins.Length; i++) {
				if (!await SendMessageToAdmin(admins[i], message, parseMode))
					return false;
				//To prevent bot from being banned for spamming
				await Task.Delay(500);
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

		public bool IsAdmin(int botUserId) => admins.Contains(botUserId);

		public bool IsAdmin(BotUser botUser) => IsAdmin(botUser.Id);

	}
}
