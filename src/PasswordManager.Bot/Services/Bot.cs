using Microsoft.Extensions.Options;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using PasswordManager.Bot.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services; 

/// <summary>
/// Telegram Bot Service
/// </summary>
public class Bot : IBot {

	public TelegramBotClient Client { get; }

	public bool IsPublic { get; }

	//TODO:
	//Add feature to edit admins at runtime
	//todo get rid of such implementation, use from other projects
	private long[] admins;

	private readonly string token;

	public Bot(IOptions<BotSettings> botSettingsConfig) {
		BotSettings botSettings = botSettingsConfig.Value;

		token = botSettings.Token;
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
		try {
			Client = new TelegramBotClient(botSettings.Token);
		} catch (ArgumentException exception) {
			//TODO
			//Log exception
			throw;
		}
		IsPublic = botSettings.IsPublic;
		admins = botSettings.AdminIds;
		//TODO:
		//Write allowedUpdates in botsettings config
		UpdateType[] allowedUpdates = new UpdateType[] {
			UpdateType.Message,
			UpdateType.CallbackQuery
		};
		//TODO: find another solution to set webhook, for example flag in appsettings
		//SetWebhook(botSettings.Domain, allowedUpdates).GetAwaiter().GetResult();
		ReportStart().GetAwaiter().GetResult();
	}

	private async Task SetWebhook(string domain, UpdateType[] allowedUpdates) {
		try {
			await Client.SetWebhookAsync(
				$"https://{domain}/api/bots/{token}",
				allowedUpdates: allowedUpdates);
		} catch (Exception exception) {
			await SendMessageToAllAdmins("Exception was thrown while setting webhook.\nBot has not started.\nSee logs for more info.");
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

	private async Task<bool> SendMessageToAdmin(long adminId, string message, ParseMode parseMode = default) {
		try {
			await Client.SendTextMessageAsync(adminId, message, parseMode);
		} catch {
			//TODO
			//Log exception and admin Id and bot Id
			return false;
		}
		return true;
	}

	public async Task<bool> SendMessageToAllAdmins(string message, ParseMode parseMode = default) {
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
			} catch {
				// ignored
			}
		}
	}

	public bool IsAdmin(long botUserId) => admins.Contains(botUserId);

	public bool IsAdmin(BotUser botUser) => IsAdmin(botUser.Id);

}