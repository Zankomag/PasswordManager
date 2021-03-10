using MultiUserLocalization;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Services.Abstractions;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Bot.Extensions;
using PasswordManager.Bot.Models;
using PasswordManager.Core.Entities;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PasswordManager.Bot.Services {
	/// <summary>
	/// Handles Telegram Bot Updates
	/// </summary>
	public class BotHandler : IBotHandler {
		private readonly IBot bot;
		private readonly IUserService userService;
		private readonly ICommandFactory commandFactory;
		private readonly IBotUserService botUserService;

		//TODO:
		//Make BaseBotHandler for framework
		public BotHandler(IBot bot, IUserService userService, ICommandFactory commandFactory, IBotUserService botUserService) {
			this.bot = bot;
			this.userService = userService;
			this.commandFactory = commandFactory;
			this.botUserService = botUserService;
		}

		public async virtual Task HandleUpdateAsync(Update update) { 
			Func<BotUser, Task> handleUpdateFunc = update.Type switch {
				UpdateType.Message => (Func<BotUser, Task>)(async (BotUser botUser) => {
					if (update.Message.Type == MessageType.Text)
						await HandleMessageAsync(update.Message, botUser);
				}),
				UpdateType.CallbackQuery => (Func<BotUser, Task>)(async (BotUser botUser)
					=> await HandleCallbackQueryAsync(update.CallbackQuery, botUser)),
				_ => null
			};
			//We do not call the database if there is no reason to retrieve the user
			if (handleUpdateFunc != null) {
				BotUser botUser = null;
				if ((botUser = await GetUser(update)) != null)
					await handleUpdateFunc(botUser);
			}
		}

		protected async virtual Task<BotUser> GetUser(Update update)
			=> await botUserService.GetUser(update);
		

		protected async virtual Task HandleMessageAsync(Message message, BotUser user) {
			if (message == null) {
				ArgumentNullException exception = new ArgumentNullException(nameof(message));
				//TODO:
				//Log exception
				throw exception;
			} 
			if (user == null) {
				ArgumentNullException exception = new ArgumentNullException(nameof(user));
				//TODO:
				//Log exception
				throw exception;
			}

			string commandText = message.Text.GetTextCommand();

			if (commandText != null) {
				var messageCommand = commandFactory.GetMessageCommand(commandText);
				if (messageCommand != null) {
					await messageCommand.ExecuteAsync(message, user);
				} else {
					try {
						await HandleUnknownMessageCommandAsync(message, user);
					} catch { }
				}
			} else {
				if(message.ReplyToMessage != null) {
					var replyActionCommand = commandFactory.GetReplyActionCommand(user.Action);
					if(replyActionCommand != null) {
						await replyActionCommand.ExecuteAsync(message, user);
						return;
					}
				}
				var actionCommand = commandFactory.GetActionCommand(user.Action);
				if (actionCommand != null) {
					await actionCommand.ExecuteAsync(message, user);
				} else {
					//If ActionCommand is unknown - hanlde it as Search
					if (user.Action != default) {
						user.Action = default;
						await userService.UpdateActionAsync(user.Id, default);
					}
					await commandFactory.GetActionCommand(default)
						.ExecuteAsync(message, user);
				}
			}
		}


		protected async virtual Task HandleCallbackQueryAsync(CallbackQuery callbackQuery, BotUser user) {
			if (callbackQuery == null) {
				ArgumentNullException exception = new ArgumentNullException(nameof(callbackQuery));
				//TODO:
				//Log exception
				throw exception;
			}
			if (user == null) {
				ArgumentNullException exception = new ArgumentNullException(nameof(user));
				//TODO:
				//Log exception
				throw exception;
			}

			CallbackQueryCommandCode callbackCommandCode;
			try {
				callbackCommandCode = (CallbackQueryCommandCode)callbackQuery.Data[0];
			} catch (Exception exception) {
				//TODO: Log Exception
				throw;
			}

			var callbackQueryCommand = commandFactory.GetCallBackQueryCommand(callbackCommandCode);
			if (callbackQueryCommand != null) {
				await callbackQueryCommand.ExecuteAsync(callbackQuery, user);
			} else {
				try {
					await HandleUnknownCallbackQueryCommandAsync(callbackQuery, user);
				} catch { }
			}
		}

		protected virtual async Task HandleUnknownMessageCommandAsync(Message message, BotUser user) {
			await bot.Client.SendTextMessageAsync(message.From.Id,
				text: Localization.GetMessage("UnknownCommand", user.Lang));
			//await Task.FromResult(0);
		}

		protected virtual async Task HandleUnknownCallbackQueryCommandAsync(CallbackQuery callbackQuery, BotUser user) {
			await bot.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
				text: Localization.GetMessage("UnknownCommand", user.Lang),
				showAlert: true);
			//await Task.FromResult(0);
		}

	}
}
