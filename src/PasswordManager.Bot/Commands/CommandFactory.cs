using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands; 

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
		//todo refactor this anyway, current solution is ONLY temporary for working build
		
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

		//todo later return back to strongly typed initializatio with genering method as was previously like
		// Add<IMessageCommand, HelpCommand>("/help");
		// this is about all commands
		
		messageCommands = new Dictionary<string, Type> {
			{ "/help", typeof(HelpCommand) },
			{ "/start", typeof(HelpCommand) },
			{ "/language", typeof(SelectLanguageCommand) },
			{ "/all", typeof(ShowAllAccountsCommand) },
			{ "/add", typeof(AddAccountCommand) },
			{ "/cancel", typeof(CancelCommand) },
			{ "/generator", typeof(SetUpPasswordGeneratorCommand) },
			{ "/adduser", typeof(AddUserCommand) },
			//todo consider camelCase for commands in code(config), make sure they are comparing ignoring case
			{ "/removeuser", typeof(RemoveUserCommand) },
			{ "/userlist", typeof(UserListCommand) }
		};

		//todo wtf is that?
		//messageCommands.Add<UserSettingsCommand>("/settings");
	}

	private void InitActionCommands() {
		actionCommands = new Dictionary<UserAction, Type>();

		actionCommands.Add(UserAction.Search, typeof(SearchCommand));
		actionCommands.Add(UserAction.AssembleAccount, typeof(AddAccountCommand));
		actionCommands.Add(UserAction.UpdateAccount, typeof(UpdateAccountCommand));
		actionCommands.Add(UserAction.SetUpPasswordGeneratorLength, typeof(SetUpPasswordGeneratorCommand));
		actionCommands.Add(UserAction.EnterDecryptionKey, typeof(ShowPasswordCommand));
		actionCommands.Add(UserAction.EncryptPassword, typeof(EncryptPasswordCommand));
		//todo wtf is that?
		//actionCommands.Add<IActionCommand, UpdateUserSettingsCommand>(UserAction.UpdateUserSettings);
	}

	private void InitReplyActionCommands() {
		replyActionCommands = new Dictionary<UserAction, Type>();
		replyActionCommands.Add(UserAction.EncryptPassword, typeof(EncryptPasswordCommand));
	}

	private void InitCallbackQueryCommands() {
		callbackQueryCommands = new Dictionary<CallbackQueryCommandCode, Type>();

		callbackQueryCommands.Add(CallbackQueryCommandCode.AddAccount, typeof(AddAccountCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.SelectLanguage, typeof(SelectLanguageCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.GeneratePassword, typeof(GeneratePasswordCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.Search, typeof(SearchCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.ShowPassword, typeof(ShowPasswordCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.ShowAccount, typeof(ShowAccountCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.DeleteMessage, typeof(DeleteMessageCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.UpdateAccount, typeof(UpdateAccountCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.DeleteAccount, typeof(DeleteAccountCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.SetUpPasswordGenerator, typeof(SetUpPasswordGeneratorCommand));
		callbackQueryCommands.Add(CallbackQueryCommandCode.EncryptPassword, typeof(EncryptPasswordCommand));
		
		//todo wtf is that?
		//callbackQueryCommands.Add<ShowEncryptionHintCommand/*Show hint in answer callback method as alert*/>(CallbackQueryCommandCode.ShowEncryptionKeyHint);
		//callbackQueryCommands.Add<UpdateUserSettingsCommand>(CallbackQueryCommandCode.UpdateUserSettings);
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