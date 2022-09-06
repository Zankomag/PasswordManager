using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PasswordManager.Telegram.Services.Abstractions;
using PasswordManager.Telegram.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace PasswordManager.Telegram.Services; 

//TODO MAKE FUCKING TEMPORARY PROJECT Junetic.Telegram.Bot right here and move all library stuff there

//todo merge this with BotHandler and name final as TelegramBotHandler
public class BotService : IBotService {

	private readonly ITelegramBotClient client;
	private readonly ILogger<BotService> logger;
	private readonly ITelegramBotUi telegramBotUi;
	private readonly BotSettings settings;
		
	//todo validate if this works as expected
	/// <summary>
	/// Bot username with @ in front
	/// </summary>
	private readonly Lazy<Task<string>> botUsername;

	public BotService(IOptions<BotSettings> botSettings, ILogger<BotService> logger,  ITelegramBotUi telegramBotUi) {
		this.logger = logger;
		this.telegramBotUi = telegramBotUi;
		//todo check if it works when options are null
		settings = botSettings?.Value ?? throw new ArgumentNullException(nameof(botSettings), $"{nameof(botSettings)} value is null");
		client = new TelegramBotClient(settings.Token);
		botUsername = new Lazy<Task<string>>(async () => await InitializeBotUsername());
	}

	private async Task<string> InitializeBotUsername() {
		var botInfo = await client.GetMeAsync();
		return String.Concat('@', botInfo.Username);
	}

	/// <inheritdoc />
	Task IUpdateHandler.HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => this.HandleUpdateAsync(update);

	public async Task HandleUpdateAsync(Update update) {
		switch(update.Type) {
			case UpdateType.Message:
				if(update.Message!.Type == MessageType.Text) await HandleTextMessageAsync(update.Message);
				break;
			case UpdateType.InlineQuery:
				await HandleInlineQueryAsync(update.InlineQuery);
				break;
			default:
				logger.LogWarning("Update type {update.Type} is not supported", update.Type);
				break;
		}

	}

	/// <inheritdoc />
	Task IUpdateHandler.HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
		logger.LogError(exception, "Received an exception from Telegram Bot API");
		return Task.CompletedTask;
	}

	public bool IsTokenCorrect(string token) => token == settings.Token;
	

	private async Task HandleTextMessageAsync(Message message) {
		if(message is null) throw new ArgumentNullException(nameof(message));

		//If command contains bot username we need to exclude it from command (/btc@MyBtcBot should be /btc)
		string username = await botUsername.Value;
		int botMentionIndex = message.Text!.IndexOf(username, StringComparison.Ordinal);
		int spaceIndex = message.Text.IndexOf(' ');

		//There should not be spaces between @botUsername and /command (should be as /command@botUsername). Also space cannot be first char
		if((spaceIndex != -1 && botMentionIndex > spaceIndex) || spaceIndex == 0)
			return;

		//Bot should not respond to commands in group chats without direct mention
		if(message.From!.Id != message.Chat.Id && botMentionIndex == -1)
			return;

		(string command, string? groupName) = SplitMessagePayload(message, botMentionIndex, spaceIndex);
			
		
	}

	/// <summary>
	/// Splits message by command and single arg after command if exists
	/// </summary>
	/// <param name="message"></param>
	/// <param name="botMentionIndex">An index of @botUsername</param>
	/// <param name="spaceIndex">A first appearance of space in message</param>
	/// <returns></returns>
	private static (string command, string arg) SplitMessagePayload(Message message, int botMentionIndex, int spaceIndex) {
		if(message.Text is null) throw new ArgumentNullException(nameof(message), $"{nameof(message)}.{nameof(message.Text)} cannot be null");
		//This implementation calculates only single arg (all text after command). To calculate arg list changes needed
		(string command, string arg) = (botMentionIndex, spaceIndex) switch {
			(-1, -1) => (message.Text, null),
			(_, -1) => (message.Text[..botMentionIndex], null),
			(-1, _) => (message.Text[..spaceIndex], message.Text[spaceIndex..]),
			(_, _) => (message.Text[..botMentionIndex], message.Text[spaceIndex..])
		};
		return (command, arg);
	}

	private Task HandleInlineQueryAsync(InlineQuery inlineQuery) {
		if(inlineQuery is null) throw new ArgumentNullException(nameof(inlineQuery));
		return Task.CompletedTask;
	}

}