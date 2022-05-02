using System;
using System.Threading.Tasks;
using MultiUserLocalization;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services; 

/// <summary>
///     Handles Telegram Bot Updates
/// </summary>
public class BotHandler : IBotHandler {
	private readonly IBot bot;
	private readonly IUserService userService;
	private readonly ICommandFactory commandFactory;
	private readonly IBotUserService botUserService;

	//TODO:
	//Make BaseBotHandler for framework
	public BotHandler(IBot bot, IUserService userService, ICommandFactory commandFactory,
		IBotUserService botUserService) {
		this.bot = bot;
		this.userService = userService;
		this.commandFactory = commandFactory;
		this.botUserService = botUserService;
	}

	public virtual async Task HandleUpdateAsync(Update update) {
		Func<BotUser, Task> handleUpdateFunc = update.Type switch {
			UpdateType.Message => async botUser => {
				if(update.Message.Type == MessageType.Text)
					await HandleMessageAsync(update.Message, botUser);
			},
			UpdateType.CallbackQuery => async botUser
				=> await HandleCallbackQueryAsync(update.CallbackQuery, botUser),
			_ => null
		};

		//We do not call the database if there is no reason to retrieve the user
		if(handleUpdateFunc != null) {
			BotUser botUser = null;
			if((botUser = await GetUser(update)) != null)
				await handleUpdateFunc(botUser);
		}
	}

	protected virtual async Task<BotUser> GetUser(Update update)
		=> await botUserService.GetUser(update);


	//TODO for all message handlers: when logging exception include user id too in logger
	protected virtual async Task HandleMessageAsync(Message message, BotUser botUser) {
		if(message == null) {
			ArgumentNullException exception = new ArgumentNullException(nameof(message));

			//TODO:
			//Log exception
			throw exception;
		}
		if(botUser == null) {
			ArgumentNullException exception = new ArgumentNullException(nameof(botUser));

			//TODO:
			//Log exception
			throw exception;
		}

		string commandText = message.Text.GetTextCommand();

		if(commandText != null) {
			var messageCommand = commandFactory.GetMessageCommand(commandText);
			if(messageCommand != null) {
				await messageCommand.ExecuteAsync(message, botUser);
			} else {
				try {
					await HandleUnknownMessageCommandAsync(message, botUser);
				} catch { }
			}
		} else {
			if(message.ReplyToMessage != null) {
				var replyActionCommand = commandFactory.GetReplyActionCommand(botUser.Action);
				if(replyActionCommand != null) {
					await replyActionCommand.ExecuteAsync(message, botUser);
					return;
				}
			}
			var actionCommand = commandFactory.GetActionCommand(botUser.Action);
			if(actionCommand != null) {
				await actionCommand.ExecuteAsync(message, botUser);
			} else {
				//If ActionCommand is unknown - hanlde it as Search
				if(botUser.Action != default) {
					botUser.Action = default;
					await userService.UpdateActionAsync(botUser.Id, default);
				}
				await commandFactory.GetActionCommand(default)
					.ExecuteAsync(message, botUser);
			}
		}
	}


	protected virtual async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, BotUser botUser) {
		if(callbackQuery == null) {
			ArgumentNullException exception = new ArgumentNullException(nameof(callbackQuery));

			//TODO:
			//Log exception
			throw exception;
		}
		if(botUser == null) {
			ArgumentNullException exception = new ArgumentNullException(nameof(botUser));

			//TODO:
			//Log exception
			throw exception;
		}

		var callbackCommandCode = (CallbackQueryCommandCode)callbackQuery.Data[0];

		var callbackQueryCommand = commandFactory.GetCallBackQueryCommand(callbackCommandCode);
		if(callbackQueryCommand != null) {
			await callbackQueryCommand.ExecuteAsync(callbackQuery, botUser);
		} else {
			try {
				await HandleUnknownCallbackQueryCommandAsync(callbackQuery, botUser);
			} catch { }
		}
	}

	protected virtual async Task HandleUnknownMessageCommandAsync(Message message, BotUser botUser)
		=> await bot.Client.SendTextMessageAsync(message.From.Id,
			Localization.GetMessage("UnknownCommand", botUser.Lang));
		
	protected virtual async Task HandleUnknownCallbackQueryCommandAsync(CallbackQuery callbackQuery, BotUser botUser)
		=> await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
			Localization.GetMessage("UnknownCommand", botUser.Lang),
			true);
		
}