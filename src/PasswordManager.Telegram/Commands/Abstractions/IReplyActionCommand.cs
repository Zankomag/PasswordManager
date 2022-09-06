using System.Threading.Tasks;
using PasswordManager.Telegram.Models;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Commands.Abstractions; 

/// <summary>
/// This command processes botUser messages that are not starting with '/' and contain reply to message
/// All ReplyActionCommands also implement IActionCommand in order to handle botUser messages with no reply
/// (For example: send answer to botUser explaining that he should send message in reply to concrete message
/// and explaining how to do it)
/// </summary>
public interface IReplyActionCommand : IActionCommand {
	new Task ExecuteAsync(Message message, BotUser botUser);
}