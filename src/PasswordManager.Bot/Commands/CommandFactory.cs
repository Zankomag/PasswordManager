﻿using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands {
	public class CommandFactory : ICommandFactory {
		private Dictionary<string, Type> messageCommands;
		private Dictionary<CallbackQueryCommandCode, Type> callbackQueryCommands;
		private Dictionary<UserAction, Type> actionCommands;
		private Dictionary<UserAction, Type> replyActionCommands;

		private readonly IServiceProvider serviceProvider;

		public CommandFactory(IServiceProvider serviceProvider) {
			this.serviceProvider = serviceProvider;
			InitCommands();
		}

		private void InitCommands() {
			InitMessageCommands();
			InitActionCommands();
			InitReplyActionCommands();
			InitCallbackQueryCommands();
		}

		private void InitMessageCommands() {
			//TODO:
			//Set commands to telegram bot from file using setMyCommands
			//Check them through getMeCommands first if they match local commands
			//and change on telegram if not
			//Store commands in json file in format
			//{
			//	"Help": {
			//		"Command": "help",
			//		"Description" : "Get help"
			//	}
			//}
			//And cast to Telegram.Bot.Types.BotCommand objects on startup
			//And then use them here by Telegram.Bot.Types.BotCommand.Command key and in HelpCommand command
			//
			//TODO:
			//Add feature to Re-Init commands at runtime

			//All message commands MUST be in lower case
			messageCommands = new Dictionary<string, Type>();

			AddMessageCommand("/help", typeof(HelpCommand));
			AddMessageCommand("/start", typeof(HelpCommand));
			AddMessageCommand("/language", typeof(SelectLanguageCommand));
			AddMessageCommand("/all", typeof(ShowAllAccountsCommand));
			AddMessageCommand("/add", typeof(AddAccountCommand));
			AddMessageCommand("/cancel", typeof(CancelCommand));
			AddMessageCommand("/generator", typeof(SetUpPasswordGeneratorCommand));
			AddMessageCommand("/adduser", typeof(AddUserCommand));
			AddMessageCommand("/removeuser", typeof(RemoveUserCommand));
			AddMessageCommand("/userlist", typeof(UserListCommand));
			AddMessageCommand("/settings", typeof(UserSettingsCommand));
		}

		private void InitActionCommands() {
			actionCommands = new Dictionary<UserAction, Type>();

			AddActionCommand(UserAction.Search, typeof(SearchCommand));
			AddActionCommand(UserAction.AssembleAccount, typeof(AddAccountCommand));
			AddActionCommand(UserAction.UpdateAccount, typeof(UpdateAccountCommand));
			AddActionCommand(UserAction.SetUpPasswordGeneratorLength, typeof(SetUpPasswordGeneratorCommand));
			AddActionCommand(UserAction.EnterDecryptionKey, typeof(ShowPasswordCommand));
			AddActionCommand(UserAction.UpdateUserSettings, typeof(UpdateUserSettingsCommand));
			AddActionCommand(UserAction.EncryptPassword, typeof(EncryptPasswordCommand));
		}

		private void InitReplyActionCommands() {
			replyActionCommands = new Dictionary<UserAction, Type>();
			AddReplyActionCommand(UserAction.EncryptPassword, typeof(EncryptPasswordCommand));
		}

		private void InitCallbackQueryCommands() {
			callbackQueryCommands = new Dictionary<CallbackQueryCommandCode, Type>();

			AddCallbackQueryCommand(CallbackQueryCommandCode.AddAccount, typeof(AddAccountCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.SelectLanguage, typeof(SelectLanguageCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.GeneratePassword, typeof(GeneratePasswordCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.Search, typeof(SearchCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.ShowPassword, typeof(ShowPasswordCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.ShowAccount, typeof(ShowAccountCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.DeleteMessage, typeof(DeleteMessageCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.UpdateAccount, typeof(UpdateAccountCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.DeleteAccount, typeof(DeleteAccountCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.SetUpPasswordGenerator, typeof(SetUpPasswordGeneratorCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.ShowEncryptionKeyHint, typeof(ShowEncryptionHintCommand/*Show hint in answer callback method as alert*/));
			AddCallbackQueryCommand(CallbackQueryCommandCode.UpdateUserSettings, typeof(UpdateUserSettingsCommand));
			AddCallbackQueryCommand(CallbackQueryCommandCode.EncryptPassword, typeof(EncryptPasswordCommand));
		}

		private void AddMessageCommand(string messageCommand, Type commandType) {
			if (commandType.IsAssignableTo(typeof(IMessageCommand))
				&& commandType.IsClass && !commandType.IsAbstract)
				messageCommands.Add(messageCommand, commandType);
		}

		private void AddCallbackQueryCommand(CallbackQueryCommandCode callbackCommandCode, Type commandType) {
			if (commandType.IsAssignableTo(typeof(ICallbackQueryCommand))
				&& commandType.IsClass && !commandType.IsAbstract)
				callbackQueryCommands.Add(callbackCommandCode, commandType);
		}

		private void AddActionCommand(UserAction action, Type commandType) {
			if (commandType.IsAssignableTo(typeof(IActionCommand))
				&& commandType.IsClass && !commandType.IsAbstract)
				actionCommands.Add(action, commandType);
		}

		private void AddReplyActionCommand(UserAction action, Type commandType) {
			if (commandType.IsAssignableTo(typeof(IReplyActionCommand))
				&& commandType.IsClass && !commandType.IsAbstract)
				replyActionCommands.Add(action, commandType);
		}

		public ICallbackQueryCommand GetCallBackQueryCommand(CallbackQueryCommandCode callbackCommandCode) {
			if (callbackQueryCommands.TryGetValue(callbackCommandCode, out Type type))
				return (ICallbackQueryCommand)serviceProvider.GetService(type);
			return null;
		}
		public IMessageCommand GetMessageCommand(string messageCommand) {
			if (messageCommands.TryGetValue(messageCommand, out Type type))
				return (IMessageCommand)serviceProvider.GetService(type);
			return null;
		}
		public IActionCommand GetActionCommand(UserAction action) {
			if (actionCommands.TryGetValue(action, out Type type))
				return (IActionCommand)serviceProvider.GetService(type);
			return null;
		}

		public IReplyActionCommand GetReplyActionCommand(UserAction action) {
			if (replyActionCommands.TryGetValue(action, out Type type))
				return (IReplyActionCommand)serviceProvider.GetService(type);
			return null;
		}
	}
}
