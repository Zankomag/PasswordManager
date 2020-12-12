using PasswordManager.Bot.Models;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PasswordManager.Bot.Commands.Abstractions {
	/// <summary>
	/// This command processes user messages that are not starting with '/' and contain reply to message
	/// All ReplyActionCommands also implement IActionCommand in order to handle user messages with no reply
	/// (For example: send answer to user explaining that he should send message in reply to concrete message
	/// and explaining how to do it)
	/// </summary>
	public interface IReplyActionCommand : IActionCommand {
		new Task ExecuteAsync(Message message, BotUser user);
	}
}
