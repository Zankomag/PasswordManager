using PasswordManager.Bot.Commands.Enums;
using PasswordManager.Core.Entities;

namespace PasswordManager.Bot.Commands.Abstractions; 

public interface ICommandFactory {
	///<summary></summary>
	/// <returns>null in no command found.</returns>
	IMessageCommand GetMessageCommand(string messageCommand);
	///<summary></summary>
	/// <returns>null in no command found.</returns>
	IActionCommand GetActionCommand(UserAction action);
	///<summary></summary>
	/// <returns>null in no command found.</returns>
	IReplyActionCommand GetReplyActionCommand(UserAction action);
	///<summary></summary>
	/// <returns>null in no command found.</returns>
	ICallbackQueryCommand GetCallBackQueryCommand(CallbackQueryCommandCode callbackCommandCode);
}