using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Telegram.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Telegram.Services;

/// <summary>
/// This service should be used to run Telegram Bot on a local machine as it uses Telegram Bot API polling
/// </summary>
public class TelegramBotLocalRunner : IHostedService {

	private readonly IBotService botService;
	private readonly ILogger<TelegramBotLocalRunner> logger;
	private readonly ITelegramBotClient client;

	private CancellationTokenSource? cancellationTokenSource;
	private Task pollingTask;

	public TelegramBotLocalRunner(IOptions<BotSettings> botSettings, IBotService botService, ILogger<TelegramBotLocalRunner> logger) {
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