using PasswordManager.Bot.Commands.Abstractions;
using PasswordManager.Bot.Types.Enums;
using PasswordManager.Core.Entities;
using System;
using System.Collections.Generic;

namespace PasswordManager.Bot.Commands {
	public class CommandFactory : ICommandFactory {
		private Dictionary<string, Type> messageCommands;
		private Dictionary<CallbackCommandCode, Type> callBackQueryCommands;
		private Dictionary<UserAction, Type> actionCommands;

		private readonly IServiceProvider serviceProvider;

		public CommandFactory(IServiceProvider serviceProvider) {
			this.serviceProvider = serviceProvider;
			InitCommands();
		}

		private void InitCommands() {
			InitMessageCommands();
			InitActionCommands();
			InitCallbackQueryCommands();
		}

		private void InitMessageCommands() {
			messageCommands = new Dictionary<string, Type> {
				{ "/help", typeof(HelpCommand) },
				{ "/start", typeof(HelpCommand) },
				{ "/language", typeof(SelectLanguageCommand) },
				{ "/all", typeof(ShowAllCommand) },
				{ "/add", typeof(AddAccountCommand) },
				{ "/cancel", typeof(CancelCommand) },
				{ "/generator", typeof(SetUpPasswordGeneratorCommand) },
				{ "/adduser", typeof(AddUserCommand) },
				{ "/removeuser", typeof(RemoveUserCommand) },
				{ "/userlist", typeof(UserListCommand) }
			};
		}

		private void InitActionCommands() {
			actionCommands = new Dictionary<UserAction, Type> {
				{ UserAction.AssembleAccount, typeof(AddAccountCommand) },
				{ UserAction.Search, typeof(SearchCommand) },
				{ UserAction.Update, typeof(UpdateAccountCommand) },
				{ UserAction.UpdatePasswordLength, typeof(SetUpPasswordGeneratorCommand) }
			};
		}

		private void InitCallbackQueryCommands() {
			callBackQueryCommands = new Dictionary<CallbackCommandCode, Type> {
				{ CallbackCommandCode.SelectLanguage, typeof(SelectLanguageCommand) },
				{ CallbackCommandCode.SkipLink, typeof(SkipLinkCommand) },
				{ CallbackCommandCode.AutoLink, typeof(AutoLinkCommand) },
				{ CallbackCommandCode.GeneratePassword, typeof(GeneratePasswordCommand) },
				{ CallbackCommandCode.AcceptPassword, typeof(AcceptPasswordCommand) },
				{ CallbackCommandCode.Search, typeof(SearchCommand) },
				{ CallbackCommandCode.ShowPassword, typeof(ShowPasswordCommand) },
				{ CallbackCommandCode.ShowAccount, typeof(ShowAccountCommand) },
				{ CallbackCommandCode.DeleteMessage, typeof(DeleteMessageCommand) },
				{ CallbackCommandCode.UpdateAccount, typeof(UpdateAccountCommand) },
				{ CallbackCommandCode.DeleteAccount, typeof(DeleteAccountCommand) },
				{ CallbackCommandCode.SetUpPasswordGenerator, typeof(SetUpPasswordGeneratorCommand) }
			};
		}

		public ICallbackQueryCommand GetCallBackQueryCommand(CallbackCommandCode callbackCommandCode) {
			if(callBackQueryCommands.TryGetValue(callbackCommandCode, out Type type)) 
				return (ICallbackQueryCommand)serviceProvider.GetService(type);
			return null;
		}
		public IMessageCommand GetMessageCommand(string messageCommand) {
			if (messageCommands.TryGetValue(messageCommand, out Type type))
				return (IMessageCommand)serviceProvider.GetService(type);
			return null;
		}
		public IMessageCommand GetActionCommand(UserAction action) {
			if (actionCommands.TryGetValue(action, out Type type))
				return (IMessageCommand)serviceProvider.GetService(type);
			return null;
		}
	}
}
