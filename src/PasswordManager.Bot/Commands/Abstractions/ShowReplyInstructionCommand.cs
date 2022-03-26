using PasswordManager.Bot.Models;
using PasswordManager.Bot.Services.Abstractions;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using MultiUserLocalization;

namespace PasswordManager.Bot.Commands.Abstractions {

	/// <inheritdoc cref="PasswordManager.Bot.Commands.Abstractions.IActionCommand" />
	/// <summary>
	/// This command sends to user instruction How to send message in reply to other message
	/// Inherit from this class to include basic implementation of showing reply instruction
	/// if you command is <see cref="T:PasswordManager.Bot.Commands.Abstractions.IReplyActionCommand" /> 
	/// and you don't have any <see cref="T:PasswordManager.Bot.Commands.Abstractions.IActionCommand" /> logic
	/// </summary>
	public abstract class ShowReplyInstructionCommand : BotCommand, IActionCommand {

		protected ShowReplyInstructionCommand(IBot bot) : base(bot) { }

		//If this method was marked as IActionCommand.ExecuteAsync,
		//it would be unoverridable. Now to override in from derived class it has
		//to create IActionCommand.ExecuteAsync() and there it can call base.ExecuteAsync
		//and base implementation would be overriden
		//
		//In case when such base class implements only one interface this works fine,
		//but if it implements two interfaces that have same signatures,
		//there is is only kludge workarounds:
		//
		// 1: For each such methods create protected method (InterfaceAMethod) that do all needed work
		//and in InterfaceA.Method just call that method. So if derived class wants to
		//override InterfaceA.Method it just creates same InterfaceA.Method and there 
		//it can call base.InterfaceAMethod (one that is protected). (https://stackoverflow.com/a/5976311/11101834)
		//
		// 2: Using reflection: (More complex solution) (https://stackoverflow.com/a/12044000/11101834)

		public virtual async Task ExecuteAsync(Message message, BotUser botUser)
			=> await Bot.Client.SendTextMessageAsync(botUser.Id,
				Localization.GetMessage("SendMessageInReply", botUser.Lang));
	}
}
