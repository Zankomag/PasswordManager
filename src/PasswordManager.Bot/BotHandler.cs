using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using PasswordManager.Bot.Commands;
using PasswordManager.Bot.Types.Enums;
using MultiUserLocalization;
using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Core.Entities;
using PasswordManager.Bot.Abstractions;
using PasswordManager.Core.Repositories;
using System.Linq;
using PasswordManager.Bot.Models;
using PasswordManager.Application.Services.Abstractions;
using User = PasswordManager.Core.Entities.User;

namespace PasswordManager.Bot {
	public class BotHandler : IBotHandler {
		private readonly IBotService botService;
		private readonly IUserService userService;
		

		private static readonly SelectLanguageCommand selectLanguageCommand = new SelectLanguageCommand();

		public BotHandler(IBotService botService, IUserService userService) {
			this.botService = botService;
			this.userService = userService;
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
							await selectLanguageCommand.ExecuteAsync(message, new BotUser {
								Id = message.From.Id,
								Lang = Localization.DefaultLanguageCode
							});
							return;
						}
					} 
					else {
						return;
					}
				}
				BotUser user = new BotUser {
					Id = userEntity.Id,
					Lang = userEntity.Lang,
					Action = userEntity.Action
				};
				

				string commandText = null;

				//Command that starts with '/' may contain args and must be separated
				if (message.Text.StartsWith('/')) {
					string commandString = message.Text.ToLower();
					int cIndex = commandString.IndexOfAny(new char[] { ' ', '\n' });
					commandText = cIndex != -1 ? commandString.Substring(0, cIndex) : commandString;
				}

				if (commandText != null && messageCommands.TryGetValue(commandText, out IMessageCommand command)) {
					await command.ExecuteAsync(message, user);
				}
				else {
					try {
						await actionCommands[user.Action].ExecuteAsync(message, user);
					}
					catch (KeyNotFoundException exception) {
						userEntity.Action = UserAction.Search;
						user.Action = UserAction.Search;
						await userService.UpdateActionAsync(userEntity);
						await actionCommands[UserAction.Search].ExecuteAsync(message, user);
						//TODO: Log exception
					}
				}
			}
			catch (Exception ex) {
				botService.SendMessageToAllAdmins(ex.ToString());
				//TODO: Log Exception
				throw;
			}
		}


		private async Task HandleCallbackQuery(CallbackQuery callbackQuery) {
			BotUser logUser = null;
			try {
				BotUser user = null;
				User userEntity = await userService.GetLangAsync(callbackQuery.From.Id);
				if (userEntity == null) {
					//Add new user to db when he selected language for the first time
					//
					//ONLY If unauthorized user is admin - bot treats them as new user
					//Not-admin users must be added to bot by Admin manually
					//If you want to allow free registration in your bot for any user - disable this admin check
					if (botService.Admins.Contains(callbackQuery.From.Id))
					{
						//TODO:
						//Separate adding new account logic to othee method
						if (callbackQuery.Data[0] == (char)CallbackCommandCode.SelectLanguage)
						{
							userEntity = await userService
								.AddUserAsync(callbackQuery.From.Id, Localization.DefaultLanguageCode);
							user = new BotUser {
								Id = userEntity.Id,
								Lang = userEntity.Lang,
								Action = userEntity.Action
							};
							await selectLanguageCommand.ExecuteAsync(callbackQuery, user);
							return;
						}
					}
					return;
				}
				logUser = user;
				

				ICallbackQueryCommand command;
				if (callBackCommands.TryGetValue((CallbackCommandCode)callbackQuery.Data[0], out command)) {
					await command.ExecuteAsync(callbackQuery, user);
				}
				else {
					try {
						await botService.Client.AnswerCallbackQueryAsync(callbackQuery.Id, text: "Unknown command", showAlert: true);
					} catch { }
				}
				
			}
			catch (Telegram.Bot.Exceptions.InvalidParameterException exeption) {
				//TODO: Log Exception
			}
			catch (Exception exeption) {
				string message = "EXCEPTION: " + exeption.ToString();
				if(logUser != null)
					message += "\n\nUSER: [user](tg://user?id=" + logUser.Id.ToString() + ")";
				botService.SendMessageToAllAdmins(message);
				//TODO: Log exception
			}

		}

	}
}
