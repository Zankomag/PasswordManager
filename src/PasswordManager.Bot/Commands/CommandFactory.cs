using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands {
	public class CommandFactory : ICommandFactory {
		//We use Type instead of ICommand objects because we initialize commands using IServiceProvider
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

			//TODO add manually to dictionary
			Add<IMessageCommand, HelpCommand>("/help");
			Add<IMessageCommand, HelpCommand>("/start");
			Add<IMessageCommand, SelectLanguageCommand>("/language");
			Add<IMessageCommand, ShowAllAccountsCommand>("/all");
			Add<IMessageCommand, AddAccountCommand>("/add");
			Add<IMessageCommand, CancelCommand>("/cancel");
			Add<IMessageCommand, SetUpPasswordGeneratorCommand>("/generator");
			Add<IMessageCommand, AddUserCommand>("/adduser");
			Add<IMessageCommand, RemoveUserCommand>("/removeuser");
			Add<IMessageCommand, UserListCommand>("/userlist");
			Add<IMessageCommand, UserSettingsCommand>("/settings");
		}

		private void InitActionCommands() {
			actionCommands = new Dictionary<UserAction, Type>();

			Add<IActionCommand, SearchCommand>(UserAction.Search);
			Add<IActionCommand, AddAccountCommand>(UserAction.AssembleAccount);
			Add<IActionCommand, UpdateAccountCommand>(UserAction.UpdateAccount);
			Add<IActionCommand, SetUpPasswordGeneratorCommand>(UserAction.SetUpPasswordGeneratorLength);
			Add<IActionCommand, ShowPasswordCommand>(UserAction.EnterDecryptionKey);
			Add<IActionCommand, UpdateUserSettingsCommand>(UserAction.UpdateUserSettings);
			Add<IActionCommand, EncryptPasswordCommand>(UserAction.EncryptPassword);
		}

		private void InitReplyActionCommands() {
			replyActionCommands = new Dictionary<UserAction, Type>();
			Add<IReplyActionCommand, EncryptPasswordCommand>(UserAction.EncryptPassword);
		}

		private void InitCallbackQueryCommands() {
			callbackQueryCommands = new Dictionary<CallbackQueryCommandCode, Type>();

			Add<ICallbackQueryCommand, AddAccountCommand> (CallbackQueryCommandCode.AddAccount);
			Add<ICallbackQueryCommand, SelectLanguageCommand> (CallbackQueryCommandCode.SelectLanguage);
			Add<ICallbackQueryCommand, GeneratePasswordCommand>(CallbackQueryCommandCode.GeneratePassword);
			Add<ICallbackQueryCommand, SearchCommand>(CallbackQueryCommandCode.Search);
			Add<ICallbackQueryCommand, ShowPasswordCommand>(CallbackQueryCommandCode.ShowPassword);
			Add<ICallbackQueryCommand, ShowAccountCommand>(CallbackQueryCommandCode.ShowAccount);
			Add<ICallbackQueryCommand, DeleteMessageCommand>(CallbackQueryCommandCode.DeleteMessage);
			Add<ICallbackQueryCommand, UpdateAccountCommand>(CallbackQueryCommandCode.UpdateAccount);
			Add<ICallbackQueryCommand, DeleteAccountCommand>(CallbackQueryCommandCode.DeleteAccount);
			Add<ICallbackQueryCommand, SetUpPasswordGeneratorCommand>(CallbackQueryCommandCode.SetUpPasswordGenerator);
			Add<ICallbackQueryCommand, ShowEncryptionHintCommand/*Show hint in answer callback method as alert*/>(CallbackQueryCommandCode.ShowEncryptionKeyHint);
			Add<ICallbackQueryCommand, UpdateUserSettingsCommand>(CallbackQueryCommandCode.UpdateUserSettings);
			Add<ICallbackQueryCommand, EncryptPasswordCommand>(CallbackQueryCommandCode.EncryptPassword);
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
