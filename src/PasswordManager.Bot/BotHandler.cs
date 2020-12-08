using MultiUserLocalization;
using PasswordManager.Application.Services.Abstractions;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Bot.Commands;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Models;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Core.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = PasswordManager.Core.Entities.User;
using PasswordManager.Bot.Extensions;

namespace PasswordManager.Bot {
	public class BotHandler : IBotHandler {
		private readonly IBotService botService;
		private readonly IUserService userService;
		private readonly ICommandFactory commandFactory;

		public BotHandler(IBotService botService, IUserService userService, ICommandFactory commandFactory) {
			this.botService = botService;
			this.userService = userService;
			this.commandFactory = commandFactory;
		}

		public async void HandleUpdate(Update update) {
			switch (update.Type) {
				case UpdateType.Message:
					if (update.Message.Type == MessageType.Text)
						await HandleMessage(update.Message);
					return;
				case UpdateType.CallbackQuery:
					await HandleCallbackQuery(update.CallbackQuery);
					return;
			}
		}

		private async Task HandleMessage(Message message) {
			try {
				BotUser user = null;
				User userEntity = await userService.GetLangAsync(message.From.Id);
				if (userEntity == null) {
					//User must choose language before be added to db and using any command
					//
					//ONLY If unauthorized user is admin - bot treats them as new user
					//Not-admin users must be added to bot by Admin manually
					//If you want to allow free registration in your bot for any user - disable this admin check
					if (botService.Admins.Contains(message.From.Id)) {
						//TODO:
						//Separate adding new account logic to othee method
						if (Localization.ContainsLanguage(message.From.LanguageCode)) {
							userEntity = await userService
								.AddUserAsync(message.From.Id, message.From.LanguageCode);
						} else {
							//TODO: 
							//Use key from new BotCommand system, not hardcoded
							//
							//User is don't added to DB here as in CallbackQuerry Handler
							//because he should not be treated as registered user
							//untill he selects language
							await commandFactory.GetMessageCommand("/language")
								.ExecuteAsync(message, new BotUser {
									Id = message.From.Id,
									Lang = Localization.DefaultLanguageCode
								});
							return;
						}
					} else {
						return;
					}
				}

				if(user == null) {
					user = new BotUser {
						Id = userEntity.Id,
						Lang = userEntity.Lang,
						Action = userEntity.Action
					};
				}

				string commandText = message.Text.GetTextCommand();

				if (commandText != null) {
					var messageCommand = commandFactory.GetMessageCommand(commandText);
					if (messageCommand != null) {
						await messageCommand.ExecuteAsync(message, user);
					} else {
						try {
							await botService.Client.SendTextMessageAsync(message.From.Id,
									text: Localization.GetMessage("UnknownCommand", user.Lang));
						} catch { }
					}
				} else {
					var actionCommand = commandFactory.GetActionCommand(user.Action);
					if (actionCommand != null) {
						await actionCommand.ExecuteAsync(message, user);
					} else {
						//If ActionCommand is unknown - hanlde it as Search
						if (user.Action != UserAction.Search) {
							userEntity.Action = UserAction.Search;
							user.Action = UserAction.Search;
							await userService.UpdateActionAsync(userEntity);
						}
						await commandFactory.GetActionCommand(UserAction.Search)
							.ExecuteAsync(message, user);
					}
				}
			} catch (Exception ex) {
				await botService.SendMessageToAllAdmins(ex.ToString());
				//TODO: Log Exception
				throw;
			}
		}


		private async Task HandleCallbackQuery(CallbackQuery callbackQuery) {
			BotUser user = null;
			try {
				User userEntity = await userService.GetLangAsync(callbackQuery.From.Id);
				if (userEntity == null) {
					//Add new user to db when he selected language for the first time
					//
					//ONLY If unauthorized user is admin - bot treats them as new user
					//Not-admin users must be added to bot by Admin manually
					//If you want to allow free registration in your bot for any user - disable this admin check
					if (botService.Admins.Contains(callbackQuery.From.Id)) {
						//TODO:
						//Separate adding new account logic to othee method
						if (callbackQuery.Data[0] == (char)CallbackCommandCode.SelectLanguage) {
							userEntity = await userService
								.AddUserAsync(callbackQuery.From.Id, Localization.DefaultLanguageCode);
							user = new BotUser {
								Id = userEntity.Id,
								Lang = userEntity.Lang,
								Action = userEntity.Action
							};
							await commandFactory.GetCallBackQueryCommand(CallbackCommandCode.SelectLanguage)
								.ExecuteAsync(callbackQuery, user);
							return;
						}
					}
					return;
				}

				if (user == null) {
					user = new BotUser {
						Id = userEntity.Id,
						Lang = userEntity.Lang,
						Action = userEntity.Action
					};
				}

				//TODO: Rename
				CallbackCommandCode callbackCommandCode;
				try {
					callbackCommandCode = (CallbackCommandCode)callbackQuery.Data[0];
				} catch(Exception exeption) {
					//TODO: Log Exception
					throw;
				}

				var callbackQueryCommand = commandFactory.GetCallBackQueryCommand(callbackCommandCode);
				if(callbackQueryCommand != null) {
					await callbackQueryCommand.ExecuteAsync(callbackQuery, user);
				} else {
					try {
						await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id,
							text: Localization.GetMessage("UnknownCommand", user.Lang),
							showAlert: true);
					} catch { }
				}

			} catch (Telegram.Bot.Exceptions.InvalidParameterException exeption) {
				//TODO: Log Exception
			} catch (Exception exeption) {
				string message = "EXCEPTION: " + exeption.ToString();
				if (user != null)
					message += "\n\nUSER: [user](tg://user?id=" + user.Id.ToString() + ")";
				await botService.SendMessageToAllAdmins(message);
				//TODO: Log exception
			}
		}

	}
}
