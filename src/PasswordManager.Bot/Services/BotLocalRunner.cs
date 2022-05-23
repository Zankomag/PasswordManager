﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services;

/// <summary>
/// This service should be used to run Telegram Bot on a local machine as it uses Telegram Bot API polling
/// </summary>
public class BotLocalRunner : IHostedService {

	private readonly IBotService botService;
	private readonly ILogger<BotLocalRunner> logger;
	private readonly ITelegramBotClient client;

	private CancellationTokenSource? cancellationTokenSource;
	private Task pollingTask;

	public BotLocalRunner(IOptions<BotSettings> botSettings, IBotService botService, ILogger<BotLocalRunner> logger) {
		this.botService = botService;
		this.logger = logger;
		BotSettings settings = botSettings.Value;
		client = new TelegramBotClient(settings.Token);
	}

	/// <inheritdoc />
	public Task StartAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Starting telegram polling...");
		cancellationTokenSource = new CancellationTokenSource();
		
		//todo move allowed updates to appsettings when moving telegram bot to library
		pollingTask = Task.Run(() => client.ReceiveAsync(botService, new ReceiverOptions() {
			AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
		}, cancellationTokenSource.Token), cancellationToken);

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public async Task StopAsync(CancellationToken cancellationToken) {
		logger.LogInformation("Stopping telegram polling...");
		cancellationTokenSource?.Cancel();
		await pollingTask!;
	}

}