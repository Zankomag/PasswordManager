using PasswordManager.Bot.Enums;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Commands.Abstractions {
	public interface ICommandFactory {
		/// <returns>null in no command found.</returns>
		IMessageCommand GetMessageCommand(string messageCommand);
		/// <returns>null in no command found.</returns>
		IActionCommand GetActionCommand(UserAction action);
		/// <returns>null in no command found.</returns>
		ICallbackQueryCommand GetCallBackQueryCommand(CallbackQueryCommandCode callbackCommandCode);
	}
}
