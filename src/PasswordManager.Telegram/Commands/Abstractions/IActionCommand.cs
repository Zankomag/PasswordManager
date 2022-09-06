using System.Threading.Tasks;
using PasswordManager.Telegram.Models;
using Telegram.Bot.Types;

namespace PasswordManager.Telegram.Commands.Abstractions; 

/// <summary>
/// This command processes botUser messages that are not starting with '/' and does not contain reply
/// </summary>
public interface IActionCommand : IBotCommand {
	Task ExecuteAsync(Message message, BotUser botUser);
}