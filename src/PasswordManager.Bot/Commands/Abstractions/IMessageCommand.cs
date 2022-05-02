using System.Threading.Tasks;
using Telegram.Bot.Types;
using PasswordManager.Bot.Models;

namespace PasswordManager.Bot.Commands.Abstractions; 

/// <summary>
/// This command processes user messages that are starting with '/'
/// </summary>
public interface IMessageCommand : IBotCommand {
	Task ExecuteAsync(Message message, BotUser botUser);
}